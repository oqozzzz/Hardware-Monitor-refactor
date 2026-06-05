using Xunit;
using CPUwenduhuoqu.Communication;

namespace CPUwenduhuoqu.Tests
{
    public class ProtocolParseTests
    {
        [Fact]
        public void TryParseStatusResponse_ValidFrame_ReturnsCorrectValues()
        {
            // Build a valid STP frame manually
            var data = "STP,2,50,25000,45.5,72.1,1,1";
            byte cs = Protocol.ComputeChecksum(data);
            string frame = $"${data}*{cs:X2}\n";

            bool ok = Protocol.TryParseStatusResponse(frame, out StatusData status);
            Assert.True(ok);
            Assert.Equal(2, status.Mode);
            Assert.Equal(50, status.DutyPercent);
            Assert.Equal(25000, status.FreqHz);
            Assert.Equal(45.5f, status.CpuTemp);
            Assert.Equal(72.1f, status.GpuTemp);
            Assert.True(status.CpuValid);
            Assert.True(status.GpuValid);
        }

        [Fact]
        public void TryParseStatusResponse_BothInvalid_ReturnsCorrectValues()
        {
            var data = "STP,1,20,25000,45.5,72.1,0,0";
            byte cs = Protocol.ComputeChecksum(data);
            string frame = $"${data}*{cs:X2}\n";

            bool ok = Protocol.TryParseStatusResponse(frame, out StatusData status);
            Assert.True(ok);
            Assert.False(status.CpuValid);
            Assert.False(status.GpuValid);
        }

        [Fact]
        public void TryParseStatusResponse_NotStatusResponse_ReturnsFalse()
        {
            string frame = Protocol.BuildStatusQuery();
            bool ok = Protocol.TryParseStatusResponse(frame, out _);
            Assert.False(ok);
        }

        [Fact]
        public void TryParseStatusResponse_InvalidChecksum_ReturnsFalse()
        {
            string tampered = "$STP,2,50,25000,45.5,72.1,1,1*FF\n";
            bool ok = Protocol.TryParseStatusResponse(tampered, out _);
            Assert.False(ok);
        }

        [Fact]
        public void TryParseAck_ValidAck_ReturnsTrue()
        {
            var data = "ACK";
            byte cs = Protocol.ComputeChecksum(data);
            string frame = $"${data}*{cs:X2}\n";
            Assert.True(Protocol.TryParseAck(frame));
        }

        [Fact]
        public void TryParseAck_NotAck_ReturnsFalse()
        {
            string frame = Protocol.BuildStatusQuery();
            Assert.False(Protocol.TryParseAck(frame));
        }

        [Fact]
        public void TryParseNack_ValidNack_ReturnsCode()
        {
            var data = "NAK,03";
            byte cs = Protocol.ComputeChecksum(data);
            string frame = $"${data}*{cs:X2}\n";
            bool ok = Protocol.TryParseNack(frame, out int code);
            Assert.True(ok);
            Assert.Equal(3, code);
        }

        [Fact]
        public void TryParseNack_NotNack_ReturnsFalse()
        {
            string frame = Protocol.BuildStatusQuery();
            bool ok = Protocol.TryParseNack(frame, out _);
            Assert.False(ok);
        }

        [Fact]
        public void TryParseFcurveResponse_ValidFrame_ReturnsPoints()
        {
            var data = "FCP,2,0.0,20,100.0,100";
            byte cs = Protocol.ComputeChecksum(data);
            string frame = $"${data}*{cs:X2}\n";

            bool ok = Protocol.TryParseFcurveResponse(frame, out FanCurvePoint[] points);
            Assert.True(ok);
            Assert.Equal(2, points.Length);
            Assert.Equal(0f, points[0].Temperature);
            Assert.Equal((byte)20, points[0].DutyPercent);
            Assert.Equal(100f, points[1].Temperature);
            Assert.Equal((byte)100, points[1].DutyPercent);
        }

        [Fact]
        public void TryParseFcurveResponse_NotFcurveResponse_ReturnsFalse()
        {
            string frame = Protocol.BuildStatusQuery();
            bool ok = Protocol.TryParseFcurveResponse(frame, out _);
            Assert.False(ok);
        }
    }
}
