using System;
using System.Configuration;

namespace CPUwenduhuoqu.Configuration
{
    public class AppConfigService
    {
        private const int DefaultRefreshIntervalMs = 5000;
        private const string DefaultSerialPortName = "COM3";
        private const int DefaultBaudRate = 115200;

        public int RefreshIntervalMs
        {
            get => GetIntValue("RefreshInterval", DefaultRefreshIntervalMs);
            set => SetValue("RefreshInterval", value.ToString());
        }

        public string SelectedCpuSensor
        {
            get => GetStringValue("SelectedCpuSensor", "Label.TCPU");
            set => SetValue("SelectedCpuSensor", value);
        }

        public string SelectedGpuSensor
        {
            get => GetStringValue("SelectedGpuSensor", "Label.TGPU1");
            set => SetValue("SelectedGpuSensor", value);
        }

        public bool UseAida64Mode
        {
            get => GetBoolValue("UseAida64Mode", false);
            set => SetValue("UseAida64Mode", value ? "true" : "false");
        }

        public string SerialPortName
        {
            get => GetStringValue("SerialPortName", DefaultSerialPortName);
            set => SetValue("SerialPortName", value);
        }

        public int BaudRate
        {
            get => GetIntValue("BaudRate", DefaultBaudRate);
            set => SetValue("BaudRate", value.ToString());
        }

        public bool MinimizeToTray
        {
            get => GetBoolValue("MinimizeToTray", true);
            set => SetValue("MinimizeToTray", value ? "true" : "false");
        }

        public string LastFanCurve
        {
            get => GetStringValue("LastFanCurve", "");
            set => SetValue("LastFanCurve", value ?? "");
        }

        public void Save()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private string GetStringValue(string key, string defaultValue)
        {
            string val = ConfigurationManager.AppSettings[key];
            return string.IsNullOrEmpty(val) ? defaultValue : val;
        }

        private int GetIntValue(string key, int defaultValue)
        {
            string val = ConfigurationManager.AppSettings[key];
            return int.TryParse(val, out int result) ? result : defaultValue;
        }

        private bool GetBoolValue(string key, bool defaultValue)
        {
            string val = ConfigurationManager.AppSettings[key];
            return bool.TryParse(val, out bool result) ? result : defaultValue;
        }

        private void SetValue(string key, string value)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config.AppSettings.Settings[key] != null)
                config.AppSettings.Settings[key].Value = value;
            else
                config.AppSettings.Settings.Add(key, value);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
