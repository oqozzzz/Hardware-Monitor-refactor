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
        private int _timerCounter;
        private bool _isExiting;

        public MainForm()
        {
            InitializeComponent();
            BuildUi();
        }

        // ====================================================================
        // Form Lifecycle
        // ====================================================================

        private void MainForm_Load(object sender, EventArgs e)
        {
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
                this.WindowState = FormWindowState.Minimized;
                this.Hide();
                this.ShowInTaskbar = false;
                return;
            }

            _config.RefreshIntervalMs = _updateTimer?.Interval ?? 5000;
            _config.Save();

            _updateTimer?.Stop();
            _updateTimer = null;

            var svc = _serialService;
            var mon = _monitor;
            _serialService = null;
            _monitor = null;
            Task.Run(() =>
            {
                svc?.Dispose();
                mon?.Dispose();
            });

            notifyIcon.Visible = false;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.notifyIcon.Visible = true;
            }
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }

        private void OnExit(object sender, EventArgs e)
        {
            _isExiting = true;
            _config.Save();
            this.Close();
        }

        // ====================================================================
        // Hardware Monitor
        // ====================================================================

        private void SwitchToLibreHardwareMonitor()
        {
            _monitor?.Dispose();
            _monitor = new LibreHardwareMonitorService();
            toolStripStatusAida64CpuMonitor.Text = "来源: LibreHardwareMonitor";
            toolStripStatusAida64GpuMonitor.Text = "";
        }

        private void SwitchToAida64()
        {
            var aida = new Aida64MonitorService();
            if (!aida.LoadSensors())
            {
                MessageBox.Show("未在注册表中找到 AIDA64 传感器。\n请检查 AIDA64 配置。",
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
                if (match.valueName != null) comboBoxChooseCpuMonitor.SelectedItem = match.displayName;
            }
            if (!string.IsNullOrEmpty(lastGpu))
            {
                var match = aida.GpuSensors.FirstOrDefault(s => s.valueName == lastGpu);
                if (match.valueName != null) comboBoxChooseGpuMonitor.SelectedItem = match.displayName;
            }

            toolStripStatusAida64CpuMonitor.Text = "来源: AIDA64";
            toolStripStatusAida64GpuMonitor.Text = "";
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

                    cpuTempLabel.Invoke(new Action(() =>
                    {
                        cpuTempLabel.Text = $"CPU 温度: {(cpuTemp.HasValue ? cpuTemp.Value.ToString("F1") + " °C" : "无数据")}";
                        gpuTempLabel.Text = $"GPU 温度: {(gpuTemp.HasValue ? gpuTemp.Value.ToString("F1") + " °C" : "无数据")}";
                    }));

                    if (_timerCounter % 2 == 0 && cpuTemp.HasValue)
                        _serialService?.Send(Protocol.BuildTempFrame(true, cpuTemp.Value));
                    else if (_timerCounter % 2 == 1 && gpuTemp.HasValue)
                        _serialService?.Send(Protocol.BuildTempFrame(false, gpuTemp.Value));

                    _timerCounter++;
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
                        this.BeginInvoke(new Action(() =>
                        {
                            this.WindowState = FormWindowState.Minimized;
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
                    this.BeginInvoke(new Action(() => buttonConnect.Enabled = true));
                });
            }
            else if (comboBoxSerialPorts.SelectedItem != null)
            {
                string port = comboBoxSerialPorts.SelectedItem.ToString();
                buttonConnect.Enabled = false;
                Task.Run(() =>
                {
                    bool ok = _serialService.Open(port);
                    this.BeginInvoke(new Action(() =>
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
            if (this.IsDisposed) return;
            this.BeginInvoke(new Action(() =>
            {
                if (this.IsDisposed) return;
                labelConnectionStatus.Text = connected ? "已连接" : "已断开";
            }));
        }

        private void OnDataReceived(object s, string frame)
        {
            if (this.IsDisposed) return;
            this.BeginInvoke(new Action(() =>
            {
                if (this.IsDisposed) return;

                FrameType ft = Protocol.IdentifyFrame(frame);
                switch (ft)
                {
                    case FrameType.StatusResponse:
                        if (Protocol.TryParseStatusResponse(frame, out StatusData status))
                        {
                            UpdateDashboard(status);
                            AppendStatusLog($"[状态] 模式={status.Mode} 风扇={status.DutyPercent}% " +
                                $"频率={status.FreqHz}Hz CPU={status.CpuTemp}°C GPU={status.GpuTemp}°C");
                        }
                        break;
                    case FrameType.FcurveResponse:
                        if (Protocol.TryParseFcurveResponse(frame, out FanCurvePoint[] points))
                        {
                            _fanCurveGrid.Rows.Clear();
                            foreach (var p in points)
                                _fanCurveGrid.Rows.Add(p.Temperature, p.DutyPercent);
                            AppendStatusLog($"[风扇曲线] 收到 {points.Length} 个点");
                        }
                        break;
                    case FrameType.Ack:
                        AppendStatusLog("[ACK] 风扇曲线已接受");
                        break;
                    case FrameType.Nack:
                        if (Protocol.TryParseNack(frame, out int err))
                            AppendStatusLog($"[NACK] 错误码: {err}");
                        break;
                }
            }));
        }

        // ====================================================================
        // Fan Curve Buttons
        // ====================================================================

        private void BtnSendCurve_Click(object sender, EventArgs e)
        {
            if (_serialService == null || !_serialService.IsOpen)
            {
                MessageBox.Show("未连接到设备。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var points = new FanCurvePoint[_fanCurveGrid.Rows.Count - 1];
                for (int i = 0; i < points.Length; i++)
                {
                    var row = _fanCurveGrid.Rows[i];
                    points[i] = new FanCurvePoint
                    {
                        Temperature = float.Parse(row.Cells[0].Value?.ToString() ?? "0"),
                        DutyPercent = byte.Parse(row.Cells[1].Value?.ToString() ?? "0")
                    };
                }

                string frame = Protocol.BuildFcurveSet(points);
                Task.Run(() => _serialService.Send(frame));
                AppendStatusLog("[发送] 风扇曲线已发送");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无效的曲线数据: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnReadCurve_Click(object sender, EventArgs e)
        {
            if (_serialService == null || !_serialService.IsOpen)
            {
                MessageBox.Show("未连接到设备。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Task.Run(() => _serialService.Send(Protocol.BuildFcurveQuery()));
        }

        private void BtnQueryStatus_Click(object sender, EventArgs e)
        {
            if (_serialService == null || !_serialService.IsOpen)
            {
                MessageBox.Show("未连接到设备。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Task.Run(() => _serialService.Send(Protocol.BuildStatusQuery()));
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
        // Refresh Time
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
    }
}
