using System;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CPUwenduhuoqu.Communication;
using CPUwenduhuoqu.Configuration;
using CPUwenduhuoqu.Hardware;

namespace CPUwenduhuoqu
{
    public partial class MainForm : Form
    {
        private IHardwareMonitor _monitor;
        private SerialPortService _serialService;
        private readonly AppConfigService _config = new AppConfigService();
        private System.Windows.Forms.Timer _updateTimer;
        private StatusData _lastStatus;
        private bool _isExiting;
        private bool _hasReceivedStatus;

        public MainForm()
        {
            InitializeComponent();
            BuildDashboard();
            BuildFanCurveData();
        }

        // ====================================================================
        // Form Lifecycle
        // ====================================================================

        private void MainForm_Load(object sender, EventArgs e)
        {
            checkBoxMinimizeToTray.Checked = _config.MinimizeToTray;

            if (_config.UseAida64Mode)
                SwitchToAida64();
            else
                SwitchToLibreHardwareMonitor();

            _serialService = new SerialPortService(_config.SerialPortName, _config.BaudRate);
            _serialService.ConnectionChanged += OnConnectionChanged;
            _serialService.DataReceived += OnDataReceived;

            RefreshSerialPorts();
            TryAutoConnect();

            InitializeOrUpdateTimer(_config.RefreshIntervalMs);
            InitializeDomainUpDown();

            notifyIcon.Visible = true;
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[]
            {
                new MenuItem("显示主窗口", (s, ev) => { Show(); WindowState = FormWindowState.Normal; ShowInTaskbar = true; }),
                new MenuItem("-"),
                new MenuItem("退出程序", OnExit)
            });
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_isExiting && _config.MinimizeToTray)
            {
                e.Cancel = true;
                WindowState = FormWindowState.Minimized;
                Hide();
                ShowInTaskbar = false;
                return;
            }

            _config.RefreshIntervalMs = _updateTimer?.Interval ?? 5000;
            _config.Save();

            _updateTimer?.Stop();
            _updateTimer = null;

            // 2. Unsubscribe events before disposing to prevent callbacks into disposed form
            if (_serialService != null)
            {
                _serialService.ConnectionChanged -= OnConnectionChanged;
                _serialService.DataReceived -= OnDataReceived;
            }

            // 3. Dispose synchronously (both are fast operations, no need for Task.Run)
            _serialService?.Dispose();
            _monitor?.Dispose();
            _serialService = null;
            _monitor = null;

            notifyIcon.Visible = false;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon.Visible = true;
            }
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
        }

        private void OnExit(object sender, EventArgs e)
        {
            _isExiting = true;
            _config.Save();
            Close();
        }

        // ====================================================================
        // Hardware Monitor
        // ====================================================================

        private void SwitchToLibreHardwareMonitor()
        {
            try
            {
                _monitor?.Dispose();
                _monitor = new LibreHardwareMonitorService();
                toolStripStatusAida64CpuMonitor.Text = "来源: LibreHardwareMonitor";
                toolStripStatusAida64GpuMonitor.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("初始化 LibreHardwareMonitor 时出错\n" + ex.Message,
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SwitchToAida64()
        {
            var aida = new Aida64MonitorService();
            if (!aida.LoadSensors())
            {
                MessageBox.Show("未找到 AIDA64 传感器。\n" + aida.LastErrorMessage + "\n请检查 AIDA64 是否正在运行且已启用注册表共享。",
                    "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                checkBoxUseAida64Mode.Checked = false;
                aida.Dispose();
                return;
            }

            _monitor?.Dispose();
            _monitor = aida;

            comboBoxChooseCpuMonitor.Items.Clear();
            comboBoxChooseGpuMonitor.Items.Clear();
            foreach (var s in aida.CpuSensors) comboBoxChooseCpuMonitor.Items.Add(s.displayName);
            foreach (var s in aida.GpuSensors) comboBoxChooseGpuMonitor.Items.Add(s.displayName);

            string lastCpu = _config.SelectedCpuSensor;
            string lastGpu = _config.SelectedGpuSensor;
            if (!string.IsNullOrEmpty(lastCpu))
            {
                var match = aida.CpuSensors.FirstOrDefault(s => s.valueName == lastCpu);
                if (match.valueName != null)
                {
                    int idx = aida.CpuSensors.IndexOf(match);
                    comboBoxChooseCpuMonitor.SelectedIndex = idx;
                    aida.SelectSensors(match.valueName, aida.GpuSensorName);
                }
            }
            if (!string.IsNullOrEmpty(lastGpu))
            {
                var match = aida.GpuSensors.FirstOrDefault(s => s.valueName == lastGpu);
                if (match.valueName != null)
                {
                    int idx = aida.GpuSensors.IndexOf(match);
                    comboBoxChooseGpuMonitor.SelectedIndex = idx;
                    aida.SelectSensors(aida.CpuSensorName, match.valueName);
                }
            }
        }

        private bool CheckConnected()
        {
            if (_serialService == null || !_serialService.IsOpen)
            {
                MessageBox.Show("请先连接串口。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private bool CheckReadyForRemote()
        {
            if (!CheckConnected()) return false;
            if (!_hasReceivedStatus)
            {
                MessageBox.Show("尚未收到固件状态数据，请稍候。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
        }

        // ====================================================================
        // Timer & Data Sending
        // ====================================================================

        private void InitializeOrUpdateTimer(int intervalMs)
        {
            _updateTimer?.Stop();
            _updateTimer?.Dispose();

            _updateTimer = new System.Windows.Forms.Timer { Interval = intervalMs };
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();

            int sec = intervalMs / 1000;
            domainUpDownSelectRefreshTime.SelectedItem = sec.ToString();
        }

        private void InitializeDomainUpDown()
        {
            string[] options = Enumerable.Range(3, 28).Select(i => i.ToString()).ToArray();
            Array.Reverse(options);
            domainUpDownSelectRefreshTime.Items.AddRange(options);
            int currentSec = _updateTimer.Interval / 1000;
            int idx = Array.IndexOf(options, currentSec.ToString());
            domainUpDownSelectRefreshTime.SelectedIndex = idx >= 0 ? idx : 0;
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if (_monitor == null) return;

            Task.Run(() =>
            {
                try
                {
                    var (cpuTemp, gpuTemp) = _monitor.ReadTemperatures();

                    BeginInvoke(new Action(() =>
                    {
                        cpuTempLabel.Text = $"CPU 温度: {(cpuTemp.HasValue ? cpuTemp.Value.ToString("F1") + " °C" : "无数据")}";
                        gpuTempLabel.Text = $"GPU 温度: {(gpuTemp.HasValue ? gpuTemp.Value.ToString("F1") + " °C" : "无数据")}";
                    }));

                    if (cpuTemp.HasValue)
                        _serialService?.Send(Protocol.BuildTempFrame(true, cpuTemp.Value));
                    if (gpuTemp.HasValue)
                        _serialService?.Send(Protocol.BuildTempFrame(false, gpuTemp.Value));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Timer tick error: {ex.Message}");
                }
            });
        }

        // ====================================================================
        // Serial Port
        // ====================================================================

        private void RefreshSerialPorts()
        {
            comboBoxSerialPorts.Items.Clear();
            comboBoxSerialPorts.Items.AddRange(SerialPort.GetPortNames());
            if (comboBoxSerialPorts.Items.Count > 0)
                comboBoxSerialPorts.SelectedIndex = 0;
        }

        private void TryAutoConnect()
        {
            string port = _config.SerialPortName;
            if (!string.IsNullOrEmpty(port) && SerialPort.GetPortNames().Contains(port))
            {
                comboBoxSerialPorts.SelectedItem = port;
                Task.Run(() =>
                {
                    if (_serialService.Open(port))
                    {
                        BeginInvoke(new Action(() =>
                        {
                            WindowState = FormWindowState.Minimized;
                        }));
                    }
                });
            }
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (_serialService == null) return;

            if (_serialService.IsOpen)
            {
                buttonConnect.Enabled = false;
                Task.Run(() =>
                {
                    _serialService.Close();
                    BeginInvoke(new Action(() => buttonConnect.Enabled = true));
                });
            }
            else if (comboBoxSerialPorts.SelectedItem != null)
            {
                string port = comboBoxSerialPorts.SelectedItem.ToString();
                buttonConnect.Enabled = false;
                Task.Run(() =>
                {
                    bool ok = _serialService.Open(port);
                    BeginInvoke(new Action(() =>
                    {
                        if (ok)
                        {
                            _config.SerialPortName = port;
                            _config.Save();
                        }
                        buttonConnect.Enabled = true;
                    }));
                });
            }
        }

        private void OnConnectionChanged(object s, bool connected)
        {
            if (IsDisposed) return;
            try
            {
            BeginInvoke(new Action(() =>
            {
                if (IsDisposed) return;
                labelConnectionStatus.Text = connected ? "已连接" : "已断开";
            }));
            }
            catch (ObjectDisposedException) { }
        }

        private void OnDataReceived(object s, string frame)
        {
            if (IsDisposed) return;
            try
            {
            BeginInvoke(new Action(() =>
            {
                if (IsDisposed) return;

                AppendStatusLog($"RX: {frame}");

                StatusData status;
                if (Protocol.TryParseStatusResponse(frame, out status))
                    {
                        UpdateDashboard(status);
                    }
            }));
            }
            catch (ObjectDisposedException) { }
        }

        // ====================================================================
        // Fan Curve
        // ====================================================================

        private void BtnSendCurve_Click(object sender, EventArgs e)
        {
            if (!CheckConnected()) return;

            var points = new System.Collections.Generic.List<FanCurvePoint>();
            foreach (DataGridViewRow row in fanCurveGrid.Rows)
            {
                if (row.IsNewRow) continue;
                if (float.TryParse(row.Cells[0].Value?.ToString(), out float temp) &&
                    byte.TryParse(row.Cells[1].Value?.ToString(), out byte duty))
                {
                    points.Add(new FanCurvePoint { Temperature = temp, DutyPercent = duty });
                }
            }

            if (points.Count == 0)
            {
                MessageBox.Show("风扇曲线为空。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Task.Run(() => _serialService.Send(Protocol.BuildFcurveSet(points.ToArray())));
            AppendStatusLog($"TX: 风扇曲线已发送 ({points.Count} 点)");
        }

        private void BtnReadCurve_Click(object sender, EventArgs e)
        {
            if (!CheckConnected()) return;
            Task.Run(() => _serialService.Send(Protocol.BuildFcurveQuery()));
        }

        private void BtnQueryStatus_Click(object sender, EventArgs e)
        {
            if (!CheckConnected()) return;
            Task.Run(() => _serialService.Send(Protocol.BuildStatusQuery()));
        }

        // ====================================================================
        // Remote Control
        // ====================================================================

        private void BtnRemoteMode_Click(object sender, EventArgs e)
        {
            if (!CheckReadyForRemote()) return;
            int nextMode = (_lastStatus.Mode % 4) + 1;
            Task.Run(() => _serialService.Send(Protocol.BuildModeSet(nextMode)));
        }

        private void BtnRemoteFreqUp_Click(object sender, EventArgs e)
        {
            if (!CheckReadyForRemote()) return;
            int newFreq = Math.Min(_lastStatus.FreqHz + 200, 40000);
            Task.Run(() => _serialService.Send(Protocol.BuildFreqSet(newFreq)));
        }

        private void BtnRemoteFreqDn_Click(object sender, EventArgs e)
        {
            if (!CheckReadyForRemote()) return;
            int newFreq = Math.Max(_lastStatus.FreqHz - 200, 1000);
            Task.Run(() => _serialService.Send(Protocol.BuildFreqSet(newFreq)));
        }

        private void BtnRemoteDutyUp_Click(object sender, EventArgs e)
        {
            if (!CheckReadyForRemote()) return;
            int newDuty = Math.Min(_lastStatus.DutyPercent + 10, 100);
            Task.Run(() => _serialService.Send(Protocol.BuildDutySet(newDuty)));
        }

        private void BtnRemoteDutyDn_Click(object sender, EventArgs e)
        {
            if (!CheckReadyForRemote()) return;
            int newDuty = Math.Max(_lastStatus.DutyPercent - 10, 0);
            Task.Run(() => _serialService.Send(Protocol.BuildDutySet(newDuty)));
        }

        // ====================================================================
        // AIDA64 Mode & Sensor Selection
        // ====================================================================

        private void checkBox_useAida64Mode(object sender, EventArgs e)
        {
            if (checkBoxUseAida64Mode.Checked)
            {
                SwitchToAida64();

                if (_monitor is Aida64MonitorService)
                {
                    checkBoxUseAida64Mode.Checked = true;
                    _config.UseAida64Mode = true;
                    comboBoxChooseCpuMonitor.Enabled = true;
                    comboBoxChooseGpuMonitor.Enabled = true;
                    labelNoticeCpuMonitor.ForeColor = Color.Black;
                    labelNoticeGpuMonitor.ForeColor = Color.Black;
                    buttonUseChosenMonitor.Enabled = true;
                    toolStripStatusAida64CpuMonitor.ForeColor = Color.Black;
                    toolStripStatusAida64GpuMonitor.ForeColor = Color.Black;
                }
                else
                {
                    checkBoxUseAida64Mode.Checked = false;
                }
            }
            else
            {
                _config.UseAida64Mode = false;
                comboBoxChooseCpuMonitor.Enabled = false;
                comboBoxChooseGpuMonitor.Enabled = false;
                buttonUseChosenMonitor.Enabled = false;
                labelNoticeCpuMonitor.ForeColor = Color.Gray;
                labelNoticeGpuMonitor.ForeColor = Color.Gray;
                toolStripStatusAida64CpuMonitor.ForeColor = Color.Gray;
                toolStripStatusAida64GpuMonitor.ForeColor = Color.Gray;
                SwitchToLibreHardwareMonitor();
            }
        }

        private void buttonUseChosenMonitor_Click(object sender, EventArgs e)
        {
            if (_monitor is Aida64MonitorService aida)
            {
                int cpuIdx = comboBoxChooseCpuMonitor.SelectedIndex;
                int gpuIdx = comboBoxChooseGpuMonitor.SelectedIndex;

                if (cpuIdx < 0 || gpuIdx < 0)
                {
                    MessageBox.Show("请同时选择 CPU 和 GPU 传感器。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string cpuName = aida.CpuSensors[cpuIdx].valueName;
                string gpuName = aida.GpuSensors[gpuIdx].valueName;
                aida.SelectSensors(cpuName, gpuName);

                _config.SelectedCpuSensor = cpuName;
                _config.SelectedGpuSensor = gpuName;
                _config.Save();

                toolStripStatusAida64CpuMonitor.Text = "CPU: " + aida.CpuSensors[cpuIdx].displayName;
                toolStripStatusAida64GpuMonitor.Text = "GPU: " + aida.GpuSensors[gpuIdx].displayName;
            }
        }

        // ====================================================================
        // Refresh Time & Minimize
        // ====================================================================

        private void buttonConfirmRefreshTime_Click(object sender, EventArgs e)
        {
            if (int.TryParse(domainUpDownSelectRefreshTime.Text, out int sec) && sec >= 3 && sec <= 30)
            {
                InitializeOrUpdateTimer(sec * 1000);
                MessageBox.Show($"刷新间隔已设置为 {sec} 秒。", "已确认", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("请选择 3 到 30 秒之间的值。", "无效", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void checkBoxMinimizeToTray_CheckedChanged(object sender, EventArgs e)
        {
            _config.MinimizeToTray = checkBoxMinimizeToTray.Checked;
            _config.Save();
        }

        // ====================================================================
        // Dashboard & Fan Curve Runtime Setup
        // ====================================================================

        private void BuildDashboard()
        {
            var headFont = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
            var tinyFont = new Font("Microsoft YaHei UI", 8F);

            // Row A: 标题标签
            DashLabel("模式:", 10, 8, headFont);
            DashLabel("风扇:", 170, 8, headFont);
            DashLabel("频率:", 330, 8, headFont);

            // 分隔线
            dashboardPanel.Controls.Add(new Label
            {
                Location = new Point(8, 36),
                Size = new Size(666, 2),
                BorderStyle = BorderStyle.Fixed3D
            });

            // Row B: CPU / GPU 标题
            DashLabel("CPU:", 10, 50, headFont);
            DashLabel("GPU:", 230, 50, headFont);

            // Row C: 更新时间标题
            DashLabel("最后更新:", 10, 82, tinyFont);

            // 设置运行时字体
            lblDashMode.Font = new Font("Consolas", 11F, FontStyle.Bold);
            lblDashFan.Font = new Font("Consolas", 11F, FontStyle.Bold);
            lblDashFreq.Font = new Font("Consolas", 11F, FontStyle.Bold);
            lblDashCpuTemp.Font = new Font("Consolas", 11F, FontStyle.Bold);
            lblDashGpuTemp.Font = new Font("Consolas", 11F, FontStyle.Bold);
            lblDashCpuOk.Font = tinyFont;
            lblDashGpuOk.Font = tinyFont;
            lblDashUpdate.Font = tinyFont;

            // 设置按钮字体
            var btnFont = new Font("Microsoft YaHei UI", 8F);
            btnRemoteMode.Font = btnFont;
            btnRemoteFreqUp.Font = btnFont;
            btnRemoteFreqDn.Font = btnFont;
            btnRemoteDutyUp.Font = btnFont;
            btnRemoteDutyDn.Font = btnFont;

            // 日志文本框字体
            txtStatusLog.Font = new Font("Consolas", 8F);
        }

        private void BuildFanCurveData()
        {
            fanCurveGrid.Columns.Add("TempCol", "温度 (°C)");
            fanCurveGrid.Columns.Add("DutyCol", "占空比 (%)");
            fanCurveGrid.Columns[0].Width = 105;
            fanCurveGrid.Columns[1].Width = 105;
            fanCurveGrid.Rows.Add(0.0f, 20);
            fanCurveGrid.Rows.Add(35.0f, 25);
            fanCurveGrid.Rows.Add(50.0f, 40);
            fanCurveGrid.Rows.Add(65.0f, 70);
            fanCurveGrid.Rows.Add(80.0f, 90);
            fanCurveGrid.Rows.Add(100.0f, 100);
        }

        // ====================================================================
        // Dashboard Helpers
        // ====================================================================

        private void DashLabel(string text, int x, int y, Font font)
        {
            dashboardPanel.Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, y),
                Font = font,
                AutoSize = true
            });
        }

        private void btnToggleView_Click(object sender, EventArgs e)
        {
            isDashboardMode = !isDashboardMode;
            dashboardPanel.Visible = isDashboardMode;
            txtStatusLog.Visible = !isDashboardMode;
            btnToggleView.Text = isDashboardMode ? "日志模式" : "仪表盘";
        }

        // ====================================================================
        // Data Update
        // ====================================================================

        private void UpdateDashboard(StatusData s)
        {
            _lastStatus = s;
            _hasReceivedStatus = true;

            if (!isDashboardMode) return;

            string modeStr;
            switch (s.Mode)
            {
                case 1: modeStr = "静音"; break;
                case 2: modeStr = "正常"; break;
                case 3: modeStr = "Turbo"; break;
                case 4: modeStr = "手动"; break;
                default: modeStr = "?"; break;
            }
            lblDashMode.Text = modeStr;
            lblDashFan.Text = $"{s.DutyPercent}%";
            lblDashFreq.Text = $"{s.FreqHz / 1000}kHz";
            lblDashCpuTemp.Text = $"{s.CpuTemp:F1} °C";
            lblDashGpuTemp.Text = $"{s.GpuTemp:F1} °C";

            lblDashCpuOk.Text = s.CpuValid ? "✓" : "✗";
            lblDashCpuOk.ForeColor = s.CpuValid ? Color.Green : Color.Red;
            lblDashGpuOk.Text = s.GpuValid ? "✓" : "✗";
            lblDashGpuOk.ForeColor = s.GpuValid ? Color.Green : Color.Red;

            lblDashUpdate.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void AppendStatusLog(string text)
        {
            txtStatusLog.AppendText(text + Environment.NewLine);
            if (txtStatusLog.Lines.Length > 200)
            {
                var lines = txtStatusLog.Lines;
                var recent = new string[100];
                Array.Copy(lines, lines.Length - 100, recent, 0, 100);
                txtStatusLog.Lines = recent;
            }
        }
    }
}