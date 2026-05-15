using System;
using System.Linq;
using System.Windows.Forms;
using LibreHardwareMonitor.Hardware;

namespace CPUwenduhuoqu.Hardware
{
    public class LibreHardwareMonitorService : IHardwareMonitor
    {
        private Computer _computer;
        private ISensor _cpuSensor;
        private ISensor _gpuSensor;
        private bool _disposed;

        public string SourceName => "LibreHardwareMonitor";

        public LibreHardwareMonitorService()
        {
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true
            };

            try
            {
                _computer.Open();
                _computer.Accept(new UpdateVisitor());

                // 预枚举传感器引用，避免每次 ReadTemperatures() 遍历整个硬件树
                foreach (IHardware hardware in _computer.Hardware)
                {
                    foreach (ISensor sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Temperature)
                        {
                            if (hardware.HardwareType == HardwareType.Cpu && _cpuSensor == null)
                            {
                                _cpuSensor = sensor;
                            }
                            else if ((hardware.HardwareType == HardwareType.GpuAmd ||
                                      hardware.HardwareType == HardwareType.GpuNvidia) &&
                                     _gpuSensor == null)
                            {
                                _gpuSensor = sensor;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化 LibreHardwareMonitor 时出错:\n{ex.Message}",
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public (float? cpuTemp, float? gpuTemp) ReadTemperatures()
        {
            if (_disposed) return (null, null);

            try
            {
                _computer.Accept(new UpdateVisitor());

                float? cpu = null;
                float? gpu = null;

                if (_cpuSensor?.Value.HasValue == true)
                    cpu = (float?)Math.Round(_cpuSensor.Value.Value, 1);
                if (_gpuSensor?.Value.HasValue == true)
                    gpu = (float?)Math.Round(_gpuSensor.Value.Value, 1);

                return (cpu, gpu);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LibreHardwareMonitor read error: {ex.Message}");
                return (null, null);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                try
                {
                    _computer?.Close();
                }
                catch { }
                _computer = null;
            }
        }

        private class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer) => computer.Traverse(this);
            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (IHardware subHardware in hardware.SubHardware)
                    subHardware.Accept(this);
            }
            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }
    }
}
