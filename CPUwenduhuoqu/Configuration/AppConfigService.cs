using System;
using System.Collections.Generic;
using System.Configuration;

namespace CPUwenduhuoqu.Configuration
{
    public class AppConfigService
    {
        private const int DefaultRefreshIntervalMs = 5000;
        private const string DefaultSerialPortName = "COM3";
        private const int DefaultBaudRate = 115200;

        // CR #4: buffer writes to avoid I/O storm on repeated setter calls
        private readonly Dictionary<string, string> _pending = new Dictionary<string, string>();

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
            if (_pending.Count == 0) return;  // CR #4: skip if nothing changed

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            foreach (var kv in _pending)
            {
                if (config.AppSettings.Settings[kv.Key] != null)
                    config.AppSettings.Settings[kv.Key].Value = kv.Value;
                else
                    config.AppSettings.Settings.Add(kv.Key, kv.Value);
            }
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            _pending.Clear();
        }

        private string GetStringValue(string key, string defaultValue)
        {
            if (_pending.TryGetValue(key, out string pendingVal)) return pendingVal;
            string val = ConfigurationManager.AppSettings[key];
            return string.IsNullOrEmpty(val) ? defaultValue : val;
        }

        private int GetIntValue(string key, int defaultValue)
        {
            if (_pending.TryGetValue(key, out string pendingVal))
                return int.TryParse(pendingVal, out int result) ? result : defaultValue;
            string val = ConfigurationManager.AppSettings[key];
            return int.TryParse(val, out int result) ? result : defaultValue;
        }

        private bool GetBoolValue(string key, bool defaultValue)
        {
            if (_pending.TryGetValue(key, out string pendingVal))
                return bool.TryParse(pendingVal, out bool result) ? result : defaultValue;
            string val = ConfigurationManager.AppSettings[key];
            return bool.TryParse(val, out bool result) ? result : defaultValue;
        }

        private void SetValue(string key, string value)
        {
            // CR #4: buffer writes — only flush on explicit Save()
            _pending[key] = value;
        }
    }
}
