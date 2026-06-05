using System;
using Xunit;
using CPUwenduhuoqu.Communication;

namespace CPUwenduhuoqu.Tests
{
    public class ProtocolBuildTests
    {
        [Fact]
        public void BuildTempFrame_ValidCpu_ProducesValidFrame()
        {
            string frame = Protocol.BuildTempFrame(true, 45.5f);
            Assert.StartsWith("$CPU,45.5*", frame);
            Assert.EndsWith("\n", frame);
            // Verify it's a valid identifiable frame
            Assert.Equal(FrameType.TempCpu, Protocol.IdentifyFrame(frame));
        }

        [Fact]
        public void BuildTempFrame_ValidGpu_ProducesValidFrame()
        {
            string frame = Protocol.BuildTempFrame(false, 72.1f);
            Assert.StartsWith("$GPU,72.1*", frame);
            Assert.EndsWith("\n", frame);
            Assert.Equal(FrameType.TempGpu, Protocol.IdentifyFrame(frame));
        }

        [Fact]
        public void BuildStatusQuery_ProducesValidFrame()
        {
            string frame = Protocol.BuildStatusQuery();
            Assert.StartsWith("$STA,?*", frame);
            Assert.Equal(FrameType.StatusQuery, Protocol.IdentifyFrame(frame));
        }

        [Fact]
        public void BuildFcurveSet_ValidPoints_ProducesValidFrame()
        {
            var points = new FanCurvePoint[]
            {
                new FanCurvePoint { Temperature = 0f, DutyPercent = 20 },
                new FanCurvePoint { Temperature = 50f, DutyPercent = 50 },
                new FanCurvePoint { Temperature = 100f, DutyPercent = 100 },
            };
            string frame = Protocol.BuildFcurveSet(points);
            Assert.StartsWith("$FCV,3,0.0,20,50.0,50,100.0,100*", frame);
            Assert.EndsWith("\n", frame);
            Assert.Equal(FrameType.FcurveSet, Protocol.IdentifyFrame(frame));
        }

        [Fact]
        public void BuildFcurveSet_NullPoints_Throws()
        {
            Assert.Throws<ArgumentException>(() => Protocol.BuildFcurveSet(null));
        }

        [Fact]
        public void BuildFcurveSet_TooFewPoints_Throws()
        {
            var points = new FanCurvePoint[] { new FanCurvePoint { Temperature = 0f, DutyPercent = 20 } };
            Assert.Throws<ArgumentException>(() => Protocol.BuildFcurveSet(points));
        }

        [Fact]
        public void BuildDutySet_ValidDuty_ProducesValidFrame()
        {
            string frame = Protocol.BuildDutySet(50);
            Assert.StartsWith("$DUT,50*", frame);
            Assert.EndsWith("\n", frame);
        }

        [Fact]
        public void BuildDutySet_BelowMinimum_Throws()
        {
            Assert.Throws<ArgumentException>(() => Protocol.BuildDutySet(19));
            Assert.Throws<ArgumentException>(() => Protocol.BuildDutySet(0));
        }

        [Fact]
        public void BuildDutySet_AboveMaximum_Throws()
        {
            Assert.Throws<ArgumentException>(() => Protocol.BuildDutySet(101));
        }

        [Fact]
        public void BuildDutySet_MinimumBoundary_Accepts()
        {
            string frame = Protocol.BuildDutySet(20);
            Assert.StartsWith("$DUT,20*", frame);
        }

        [Fact]
        public void BuildModeSet_ValidModes_ProducesValidFrame()
        {
            for (int mode = 1; mode <= 4; mode++)
            {
                string frame = Protocol.BuildModeSet(mode);
                Assert.StartsWith($"$MOD,{mode}*", frame);
            }
        }

        [Fact]
        public void BuildFreqSet_ValidFreq_ProducesValidFrame()
        {
            string frame = Protocol.BuildFreqSet(25000);
            Assert.StartsWith("$FRQ,25000*", frame);
            Assert.EndsWith("\n", frame);
        }

        [Fact]
        public void BuildFreqSet_OutOfRange_Throws()
        {
            Assert.Throws<ArgumentException>(() => Protocol.BuildFreqSet(999));
            Assert.Throws<ArgumentException>(() => Protocol.BuildFreqSet(40001));
        }

        [Fact]
        public void BuildSafetyReset_ProducesValidFrame()
        {
            string frame = Protocol.BuildSafetyReset();
            Assert.StartsWith("$SAF*", frame);
            Assert.Equal(FrameType.SafetyReset, Protocol.IdentifyFrame(frame));
        }
    }
}
