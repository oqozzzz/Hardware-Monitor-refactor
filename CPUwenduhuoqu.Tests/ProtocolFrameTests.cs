using Xunit;
using CPUwenduhuoqu.Communication;

namespace CPUwenduhuoqu.Tests
{
    public class ProtocolFrameTests
    {
        [Fact]
        public void IdentifyFrame_ValidTempCpu_ReturnsTempCpu()
        {
            // Build a valid frame and verify it identifies correctly
            string frame = Protocol.BuildTempFrame(true, 45.5f);
            Assert.Equal(FrameType.TempCpu, Protocol.IdentifyFrame(frame));
        }

        [Fact]
        public void IdentifyFrame_ValidTempGpu_ReturnsTempGpu()
        {
            string frame = Protocol.BuildTempFrame(false, 72.1f);
            Assert.Equal(FrameType.TempGpu, Protocol.IdentifyFrame(frame));
        }

        [Fact]
        public void IdentifyFrame_ValidStatusQuery_ReturnsStatusQuery()
        {
            string frame = Protocol.BuildStatusQuery();
            Assert.Equal(FrameType.StatusQuery, Protocol.IdentifyFrame(frame));
        }

        [Fact]
        public void IdentifyFrame_ValidFcurveQuery_ReturnsFcurveQuery()
        {
            string frame = Protocol.BuildFcurveQuery();
            Assert.Equal(FrameType.FcurveQuery, Protocol.IdentifyFrame(frame));
        }

        [Fact]
        public void IdentifyFrame_ValidModeSet_ReturnsUnknown()
        {
            // MOD frames are not in IdentifyFrame type map (recognized as protocol frames
            // with valid checksum but no specific FrameType match)
            string frame = Protocol.BuildModeSet(2);
            Assert.Equal(FrameType.Unknown, Protocol.IdentifyFrame(frame));
        }

        [Fact]
        public void IdentifyFrame_InvalidChecksum_ReturnsUnknown()
        {
            // Tampered checksum
            string tampered = "$CPU,45.5*FF\n";
            Assert.Equal(FrameType.Unknown, Protocol.IdentifyFrame(tampered));
        }

        [Fact]
        public void IdentifyFrame_NullOrEmpty_ReturnsUnknown()
        {
            Assert.Equal(FrameType.Unknown, Protocol.IdentifyFrame(null));
            Assert.Equal(FrameType.Unknown, Protocol.IdentifyFrame(""));
        }

        [Fact]
        public void IdentifyFrame_NoStartMarker_ReturnsUnknown()
        {
            Assert.Equal(FrameType.Unknown, Protocol.IdentifyFrame("CPU,45.5*28\n"));
        }

        [Fact]
        public void IdentifyFrame_NoChecksumSeparator_ReturnsUnknown()
        {
            Assert.Equal(FrameType.Unknown, Protocol.IdentifyFrame("$CPU,45.5\n"));
        }

        [Fact]
        public void IdentifyFrame_TruncatedChecksum_ReturnsUnknown()
        {
            Assert.Equal(FrameType.Unknown, Protocol.IdentifyFrame("$CPU,45.5*2\n"));
        }

        [Fact]
        public void IdentifyFrame_DutySet_ReturnsKnownType()
        {
            string frame = Protocol.BuildDutySet(50);
            Assert.NotEqual(FrameType.Unknown, Protocol.IdentifyFrame(frame));
        }

        [Fact]
        public void IdentifyFrame_SafetyReset_ReturnsSafetyReset()
        {
            string frame = Protocol.BuildSafetyReset();
            Assert.Equal(FrameType.SafetyReset, Protocol.IdentifyFrame(frame));
        }

        [Fact]
        public void IdentifyFrame_Ack_ReturnsAck()
        {
            // Build an ACK-style frame for testing
            // $ACK*XX\n
            var data = "ACK";
            byte cs = Protocol.ComputeChecksum(data);
            string frame = $"${data}*{cs:X2}\n";
            Assert.Equal(FrameType.Ack, Protocol.IdentifyFrame(frame));
        }
    }
}
