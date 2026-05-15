using System;

namespace CPUwenduhuoqu.Hardware
{
    public interface IHardwareMonitor : IDisposable
    {
        (float? cpuTemp, float? gpuTemp) ReadTemperatures();
        string SourceName { get; }
    }
}
