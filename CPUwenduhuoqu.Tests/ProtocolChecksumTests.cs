using Xunit;
using CPUwenduhuoqu.Communication;

namespace CPUwenduhuoqu.Tests
{
    public class ProtocolChecksumTests
    {
        [Fact]
        public void ComputeChecksum_KnownInput_ReturnsExpected()
        {
            // XOR of "CPU,45.5" = 'C'^'P'^'U'^','^'4'^'5'^'.'^'5'
            byte result = Protocol.ComputeChecksum("CPU,45.5");
            // Expected: 0x43 ^ 0x50 ^ 0x55 ^ 0x2C ^ 0x34 ^ 0x35 ^ 0x2E ^ 0x35 = 0x28
            Assert.Equal(0x28, result);
        }

        [Fact]
        public void ComputeChecksum_EmptyString_ReturnsZero()
        {
            byte result = Protocol.ComputeChecksum("");
            Assert.Equal(0, result);
        }

        [Fact]
        public void ComputeChecksum_SingleChar_ReturnsChar()
        {
            byte result = Protocol.ComputeChecksum("A");
            Assert.Equal((byte)'A', result);
        }

        [Fact]
        public void ComputeChecksum_SameInput_ProducesSameOutput()
        {
            byte r1 = Protocol.ComputeChecksum("STA,?");
            byte r2 = Protocol.ComputeChecksum("STA,?");
            Assert.Equal(r1, r2);
        }

        [Fact]
        public void ComputeChecksum_DifferentInputs_ProduceDifferentOutput()
        {
            byte r1 = Protocol.ComputeChecksum("CPU,45.5");
            byte r2 = Protocol.ComputeChecksum("GPU,72.1");
            Assert.NotEqual(r1, r2);
        }
    }
}
