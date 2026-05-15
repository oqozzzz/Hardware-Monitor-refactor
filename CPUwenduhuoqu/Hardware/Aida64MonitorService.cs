using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Win32;

namespace CPUwenduhuoqu.Hardware
{
    public class Aida64MonitorService : IHardwareMonitor
    {
        private const string RegistryPath = @"Software\FinalWire\AIDA64\SensorValues";

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private string _cpuValueName;
        private string _gpuValueName;
        private bool _disposed;

        public string SourceName => "AIDA64 Registry";
        public string CpuSensorName { get; private set; }
        public string GpuSensorName { get; private set; }

        public List<(string valueName, string displayName)> CpuSensors { get; } = new List<(string, string)>();
        public List<(string valueName, string displayName)> GpuSensors { get; } = new List<(string, string)>();

        public bool LoadSensors()
        {
            CpuSensors.Clear();
            GpuSensors.Clear();

            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath);
                if (key == null) return false;

                foreach (string valueName in key.GetValueNames())
                {
                    if (valueName.StartsWith("Label", StringComparison.OrdinalIgnoreCase))
                    {
                        string displayName = key.GetValue(valueName)?.ToString();
                        if (string.IsNullOrEmpty(displayName)) continue;

                        if (valueName.StartsWith("Label.TC", StringComparison.OrdinalIgnoreCase))
                            CpuSensors.Add((valueName, displayName));
                        else if (valueName.StartsWith("Label.TG", StringComparison.OrdinalIgnoreCase))
                            GpuSensors.Add((valueName, displayName));
                    }
                }

                key.Close();
                return CpuSensors.Count > 0 || GpuSensors.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        public void SelectSensors(string cpuLabelName, string gpuLabelName)
        {
            _lock.EnterWriteLock();
            try
            {
                _cpuValueName = cpuLabelName?.Replace("Label", "Value");
                _gpuValueName = gpuLabelName?.Replace("Label", "Value");
                CpuSensorName = cpuLabelName;
                GpuSensorName = gpuLabelName;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public (float? cpuTemp, float? gpuTemp) ReadTemperatures()
        {
            if (_disposed) return (null, null);

            _lock.EnterReadLock();
            string cpuValName, gpuValName;
            try
            {
                cpuValName = _cpuValueName;
                gpuValName = _gpuValueName;
            }
            finally
            {
                _lock.ExitReadLock();
            }

            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath);
                if (key == null) return (null, null);

                float? cpu = null;
                float? gpu = null;

                if (cpuValName != null)
                {
                    string cpuStr = key.GetValue(cpuValName)?.ToString();
                    if (float.TryParse(cpuStr, out float cpuVal)) cpu = cpuVal;
                }

                if (gpuValName != null)
                {
                    string gpuStr = key.GetValue(gpuValName)?.ToString();
                    if (float.TryParse(gpuStr, out float gpuVal)) gpu = gpuVal;
                }

                key.Close();
                return (cpu, gpu);
            }
            catch
            {
                return (null, null);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _lock?.Dispose();
            }
        }
    }
}
