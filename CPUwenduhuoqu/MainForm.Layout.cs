using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CPUwenduhuoqu.Communication;

namespace CPUwenduhuoqu
{
    partial class MainForm
    {
        // ====================================================================
        // 原 Designer.cs 字段声明
        // ====================================================================

        private IContainer components = null;
        private Label cpuTempLabel;
        private Label gpuTempLabel;
        private ComboBox comboBoxSerialPorts;
        private Button buttonConnect;
        private Label labelConnectionStatus;
        private CheckBox checkBoxUseAida64Mode;
        private ComboBox comboBoxChooseCpuMonitor;
        private ComboBox comboBoxChooseGpuMonitor;
        private Label labelNoticeCpuMonitor;
        private Label labelNoticeGpuMonitor;
        private Button buttonUseChosenMonitor;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel toolStripStatusAida64CpuMonitor;
        private ToolStripStatusLabel toolStripStatusAida64GpuMonitor;
        private DomainUpDown domainUpDownSelectRefreshTime;
        private Button buttonConfirmRefreshTime;
        private Label labelNoticeRefreshTimeAdjustmentWindow;
        private NotifyIcon notifyIcon;

        // ====================================================================
        // Layout.cs 字段声明
        // ====================================================================

        // 固件状态区
        private GroupBox _grpEsp32;
        private Panel _dashboardPanel;
        private Label _lblDashMode, _lblDashFan, _lblDashFreq;
        private Label _lblDashCpuTemp, _lblDashGpuTemp;
        private Label _lblDashCpuOk, _lblDashGpuOk;
        private Label _lblDashUpdate;
        private TextBox _txtStatusLog;
        private Button _btnToggleView;
        private Button _btnRemoteMode, _btnRemoteFreqUp, _btnRemoteFreqDn;
        private Button _btnRemoteDutyUp, _btnRemoteDutyDn;
        private bool _isDashboardMode = true;

        // 最小化到托盘
        private CheckBox _checkBoxMinimizeToTray;

        // 刷新间隔标签
        private Label _lblRefreshHint;

        // 风扇曲线编辑区
        private GroupBox _grpFanCurve;
        private DataGridView _fanCurveGrid;
        private Button _btnSendCurve;
        private Button _btnReadCurve;
        private Button _btnQueryStatus;

        // ====================================================================
        // Dispose
        // ====================================================================

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        // ====================================================================
        // UI 布局入口
        // ====================================================================

        private void BuildUi()
        {
            // ---- 控件实例化 ----
            components = new Container();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(MainForm));

            cpuTempLabel = new Label();
            gpuTempLabel = new Label();
            comboBoxSerialPorts = new ComboBox();
            buttonConnect = new Button();
            labelConnectionStatus = new Label();
            checkBoxUseAida64Mode = new CheckBox();
            comboBoxChooseCpuMonitor = new ComboBox();
            comboBoxChooseGpuMonitor = new ComboBox();
            labelNoticeCpuMonitor = new Label();
            labelNoticeGpuMonitor = new Label();
            buttonUseChosenMonitor = new Button();
            statusStrip = new StatusStrip();
            toolStripStatusAida64CpuMonitor = new ToolStripStatusLabel();
            toolStripStatusAida64GpuMonitor = new ToolStripStatusLabel();
            domainUpDownSelectRefreshTime = new DomainUpDown();
            buttonConfirmRefreshTime = new Button();
            labelNoticeRefreshTimeAdjustmentWindow = new Label();
            notifyIcon = new NotifyIcon(components);

            statusStrip.SuspendLayout();
            SuspendLayout();

            // ---- 基础控件属性设置 ----

            // cpuTempLabel
            cpuTempLabel.AutoSize = true;
            cpuTempLabel.Margin = new Padding(4, 0, 4, 0);
            cpuTempLabel.Name = "cpuTempLabel";
            cpuTempLabel.TabIndex = 0;
            cpuTempLabel.Text = "CPU 温度: ";

            // gpuTempLabel
            gpuTempLabel.AutoSize = true;
            gpuTempLabel.Margin = new Padding(4, 0, 4, 0);
            gpuTempLabel.Name = "gpuTempLabel";
            gpuTempLabel.TabIndex = 1;
            gpuTempLabel.Text = "GPU 温度: ";

            // comboBoxSerialPorts
            comboBoxSerialPorts.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxSerialPorts.FormattingEnabled = true;
            comboBoxSerialPorts.Margin = new Padding(4);
            comboBoxSerialPorts.Name = "comboBoxSerialPorts";
            comboBoxSerialPorts.TabIndex = 2;

            // buttonConnect
            buttonConnect.Margin = new Padding(4);
            buttonConnect.Name = "buttonConnect";
            buttonConnect.TabIndex = 3;
            buttonConnect.Text = "连接";
            buttonConnect.UseVisualStyleBackColor = true;
            buttonConnect.Click += buttonConnect_Click;

            // labelConnectionStatus
            labelConnectionStatus.AutoSize = true;
            labelConnectionStatus.Margin = new Padding(4, 0, 4, 0);
            labelConnectionStatus.Name = "labelConnectionStatus";
            labelConnectionStatus.TabIndex = 4;
            labelConnectionStatus.Text = "已断开";

            // checkBoxUseAida64Mode
            checkBoxUseAida64Mode.AutoSize = true;
            checkBoxUseAida64Mode.Name = "checkBoxUseAida64Mode";
            checkBoxUseAida64Mode.Size = new Size(376, 22);
            checkBoxUseAida64Mode.TabIndex = 5;
            checkBoxUseAida64Mode.Text = "使用AIDA64获取硬件温度信息";
            checkBoxUseAida64Mode.UseVisualStyleBackColor = true;
            checkBoxUseAida64Mode.CheckedChanged += checkBox_useAida64Mode;

            // comboBoxChooseCpuMonitor
            comboBoxChooseCpuMonitor.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxChooseCpuMonitor.Enabled = false;
            comboBoxChooseCpuMonitor.ForeColor = Color.Black;
            comboBoxChooseCpuMonitor.FormattingEnabled = true;
            comboBoxChooseCpuMonitor.Name = "comboBoxChooseCpuMonitor";
            comboBoxChooseCpuMonitor.TabIndex = 7;

            // comboBoxChooseGpuMonitor
            comboBoxChooseGpuMonitor.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxChooseGpuMonitor.Enabled = false;
            comboBoxChooseGpuMonitor.ForeColor = Color.Black;
            comboBoxChooseGpuMonitor.FormattingEnabled = true;
            comboBoxChooseGpuMonitor.Name = "comboBoxChooseGpuMonitor";
            comboBoxChooseGpuMonitor.TabIndex = 7;

            // labelNoticeCpuMonitor
            labelNoticeCpuMonitor.AutoSize = true;
            labelNoticeCpuMonitor.ForeColor = Color.Gray;
            labelNoticeCpuMonitor.Name = "labelNoticeCpuMonitor";
            labelNoticeCpuMonitor.TabIndex = 8;
            labelNoticeCpuMonitor.Text = " 选择CPU温度传感器";

            // labelNoticeGpuMonitor
            labelNoticeGpuMonitor.AutoSize = true;
            labelNoticeGpuMonitor.ForeColor = Color.Gray;
            labelNoticeGpuMonitor.Name = "labelNoticeGpuMonitor";
            labelNoticeGpuMonitor.TabIndex = 9;
            labelNoticeGpuMonitor.Text = " 选择GPU温度传感器";

            // buttonUseChosenMonitor
            buttonUseChosenMonitor.Enabled = false;
            buttonUseChosenMonitor.Name = "buttonUseChosenMonitor";
            buttonUseChosenMonitor.TabIndex = 10;
            buttonUseChosenMonitor.Text = "确认";
            buttonUseChosenMonitor.UseVisualStyleBackColor = true;
            buttonUseChosenMonitor.Click += buttonUseChosenMonitor_Click;

            // statusStrip
            statusStrip.ImageScalingSize = new Size(24, 24);
            statusStrip.Items.AddRange(new ToolStripItem[] {
                toolStripStatusAida64CpuMonitor,
                toolStripStatusAida64GpuMonitor });
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(673, 31);
            statusStrip.TabIndex = 11;
            statusStrip.Text = "statusStrip1";

            // toolStripStatusAida64CpuMonitor
            toolStripStatusAida64CpuMonitor.ForeColor = Color.Gray;
            toolStripStatusAida64CpuMonitor.Name = "toolStripStatusAida64CpuMonitor";
            toolStripStatusAida64CpuMonitor.Size = new Size(202, 24);
            toolStripStatusAida64CpuMonitor.Text = "AIDA64 CPU 传感器: ";

            // toolStripStatusAida64GpuMonitor
            toolStripStatusAida64GpuMonitor.ForeColor = Color.Gray;
            toolStripStatusAida64GpuMonitor.Name = "toolStripStatusAida64GpuMonitor";
            toolStripStatusAida64GpuMonitor.Size = new Size(203, 24);
            toolStripStatusAida64GpuMonitor.Text = "AIDA64 GPU 传感器: ";

            // domainUpDownSelectRefreshTime
            domainUpDownSelectRefreshTime.Name = "domainUpDownSelectRefreshTime";
            domainUpDownSelectRefreshTime.TabIndex = 12;

            // buttonConfirmRefreshTime
            buttonConfirmRefreshTime.Name = "buttonConfirmRefreshTime";
            buttonConfirmRefreshTime.TabIndex = 13;
            buttonConfirmRefreshTime.Text = "确认";
            buttonConfirmRefreshTime.UseVisualStyleBackColor = true;
            buttonConfirmRefreshTime.Click += buttonConfirmRefreshTime_Click;

            // labelNoticeRefreshTimeAdjustmentWindow
            labelNoticeRefreshTimeAdjustmentWindow.AutoSize = true;
            labelNoticeRefreshTimeAdjustmentWindow.Name = "labelNoticeRefreshTimeAdjustmentWindow";
            labelNoticeRefreshTimeAdjustmentWindow.TabIndex = 14;
            labelNoticeRefreshTimeAdjustmentWindow.Text = "选择刷新时间（3-30s）";

            // notifyIcon
            notifyIcon.Text = "硬件温度监测";
            notifyIcon.Icon = Properties.Resources.MainIcon;
            notifyIcon.Visible = false;
            notifyIcon.Click += NotifyIcon_Click;

            // ---- 窗体属性 ----
            AutoScaleDimensions = new SizeF(9F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(720, 360);
            Controls.Add(labelNoticeRefreshTimeAdjustmentWindow);
            Controls.Add(buttonConfirmRefreshTime);
            Controls.Add(domainUpDownSelectRefreshTime);
            Controls.Add(statusStrip);
            Controls.Add(buttonUseChosenMonitor);
            Controls.Add(labelNoticeGpuMonitor);
            Controls.Add(labelNoticeCpuMonitor);
            Controls.Add(comboBoxChooseGpuMonitor);
            Controls.Add(comboBoxChooseCpuMonitor);
            Controls.Add(checkBoxUseAida64Mode);
            Controls.Add(labelConnectionStatus);
            Controls.Add(buttonConnect);
            Controls.Add(comboBoxSerialPorts);
            Controls.Add(gpuTempLabel);
            Controls.Add(cpuTempLabel);
            Icon = (Icon)(resources.GetObject("$this.Icon"));
            Margin = new Padding(4);
            Name = "MainForm";
            Text = "硬件温度监测";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Resize += MainForm_Resize;
            MaximizeBox = false;
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

            // ---- 最终布局覆盖 ----
            ClientSize = new Size(720, 630);
            Font = new Font("Microsoft YaHei UI", 9F);

            RepositionDesignerControls();
            BuildEsp32Section();
            BuildFanCurveSection();
        }

        // ---- Designer 控件重定位 ----

        private void RepositionDesignerControls()
        {
            // Row 0 (y=8) 温度
            cpuTempLabel.Location = new Point(12, 10);
            gpuTempLabel.Location = new Point(380, 10);

            // Row 1 (y=40) 串口 + 刷新
            comboBoxSerialPorts.Location = new Point(12, 40);
            comboBoxSerialPorts.Size = new Size(120, 26);

            buttonConnect.Location = new Point(140, 40);
            buttonConnect.Size = new Size(70, 32);

            labelConnectionStatus.Location = new Point(218, 44);

            labelNoticeRefreshTimeAdjustmentWindow.Visible = false;

            _lblRefreshHint = new Label
            {
                Text = "刷新间隔:",
                Location = new Point(300, 44),
                AutoSize = true,
                Font = this.Font
            };
            Controls.Add(_lblRefreshHint);

            domainUpDownSelectRefreshTime.Location = new Point(385, 40);
            domainUpDownSelectRefreshTime.Size = new Size(56, 26);

            buttonConfirmRefreshTime.Location = new Point(448, 40);
            buttonConfirmRefreshTime.Size = new Size(55, 32);

            // Row 2a (y=76) AIDA64 勾选 + 最小化到托盘
            checkBoxUseAida64Mode.Location = new Point(12, 76);

            _checkBoxMinimizeToTray = new CheckBox
            {
                Text = "最小化到托盘",
                Location = new Point(420, 76),
                AutoSize = true,
                Checked = _config.MinimizeToTray,
                Font = this.Font
            };
            _checkBoxMinimizeToTray.CheckedChanged += (s, ev) =>
            {
                _config.MinimizeToTray = _checkBoxMinimizeToTray.Checked;
                _config.Save();
            };
            Controls.Add(_checkBoxMinimizeToTray);

            // Row 2b (y=104) AIDA64 传感器选择
            labelNoticeCpuMonitor.Text = "CPU传感器:";
            labelNoticeCpuMonitor.Location = new Point(12, 108);
            labelNoticeCpuMonitor.AutoSize = true;

            comboBoxChooseCpuMonitor.Location = new Point(98, 104);
            comboBoxChooseCpuMonitor.Size = new Size(155, 26);

            labelNoticeGpuMonitor.Text = "GPU传感器:";
            labelNoticeGpuMonitor.Location = new Point(262, 108);
            labelNoticeGpuMonitor.AutoSize = true;

            comboBoxChooseGpuMonitor.Location = new Point(348, 104);
            comboBoxChooseGpuMonitor.Size = new Size(155, 26);

            buttonUseChosenMonitor.Location = new Point(512, 102);
            buttonUseChosenMonitor.Size = new Size(55, 32);

            // 状态栏
            statusStrip.Location = new Point(0, 580);
        }

        // ---- 固件状态区 (y=136, h=180) ----

        private void BuildEsp32Section()
        {
            _grpEsp32 = new GroupBox
            {
                Text = "固件状态",
                Location = new Point(10, 136),
                Size = new Size(650, 190),
                Font = this.Font
            };

            _btnToggleView = new Button
            {
                Text = "日志模式",
                Location = new Point(544, 14),
                Size = new Size(95, 32),
                UseVisualStyleBackColor = true
            };
            _btnToggleView.Click += (s, e) => ToggleResponseView();

            _dashboardPanel = new Panel
            {
                Location = new Point(8, 24),
                Size = new Size(634, 140)
            };

            var headFont = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
            var dataFont = new Font("Consolas", 11F, FontStyle.Bold);
            var tinyFont = new Font("Microsoft YaHei UI", 8F);

            // Row A: 模式 / 风扇 / 频率
            DashLabel("模式:", 10, 8, headFont);
            _lblDashMode = DashValue("--", 64, 8, dataFont, Color.DarkBlue, 80);

            DashLabel("风扇:", 170, 8, headFont);
            _lblDashFan = DashValue("--", 222, 8, dataFont, Color.DarkGreen, 70);

            DashLabel("频率:", 330, 8, headFont);
            _lblDashFreq = DashValue("--", 382, 8, dataFont, Color.DarkGreen, 80);

            // 分隔线
            _dashboardPanel.Controls.Add(new Label
            {
                Location = new Point(8, 36),
                Size = new Size(666, 2),
                BorderStyle = BorderStyle.Fixed3D
            });

            // Row B: CPU / GPU
            DashLabel("CPU:", 10, 50, headFont);
            _lblDashCpuTemp = DashValue("--.- °C", 66, 50, dataFont, Color.DarkRed, 110);
            _lblDashCpuOk = new Label
            {
                Location = new Point(172, 52),
                Size = new Size(28, 20),
                Font = tinyFont,
                ForeColor = Color.Gray
            };
            _dashboardPanel.Controls.Add(_lblDashCpuOk);

            DashLabel("GPU:", 230, 50, headFont);
            _lblDashGpuTemp = DashValue("--.- °C", 286, 50, dataFont, Color.DarkRed, 110);
            _lblDashGpuOk = new Label
            {
                Location = new Point(392, 52),
                Size = new Size(28, 20),
                Font = tinyFont,
                ForeColor = Color.Gray
            };
            _dashboardPanel.Controls.Add(_lblDashGpuOk);

            // Row C: 更新时间
            DashLabel("最后更新:", 10, 82, tinyFont);
            _lblDashUpdate = new Label
            {
                Location = new Point(76, 82),
                Size = new Size(120, 18),
                Font = tinyFont,
                ForeColor = Color.Gray,
                Text = "等待数据..."
            };
            _dashboardPanel.Controls.Add(_lblDashUpdate);

            // ---- 远程控制按钮行 (y=110) ----
            var btnFont = new Font("Microsoft YaHei UI", 8F);
            var btnSize = new Size(62, 30);

            _btnRemoteMode = new Button
            {
                Text = "模式",
                Location = new Point(10, 110),
                Size = btnSize,
                Font = btnFont,
                UseVisualStyleBackColor = true
            };
            _btnRemoteMode.Click += BtnRemoteMode_Click;

            _btnRemoteFreqUp = new Button
            {
                Text = "频率+",
                Location = new Point(72, 110),
                Size = btnSize,
                Font = btnFont,
                UseVisualStyleBackColor = true
            };
            _btnRemoteFreqUp.Click += BtnRemoteFreqUp_Click;

            _btnRemoteFreqDn = new Button
            {
                Text = "频率-",
                Location = new Point(134, 110),
                Size = btnSize,
                Font = btnFont,
                UseVisualStyleBackColor = true
            };
            _btnRemoteFreqDn.Click += BtnRemoteFreqDn_Click;

            _btnRemoteDutyUp = new Button
            {
                Text = "占空+",
                Location = new Point(196, 110),
                Size = btnSize,
                Font = btnFont,
                UseVisualStyleBackColor = true
            };
            _btnRemoteDutyUp.Click += BtnRemoteDutyUp_Click;

            _btnRemoteDutyDn = new Button
            {
                Text = "占空-",
                Location = new Point(258, 110),
                Size = btnSize,
                Font = btnFont,
                UseVisualStyleBackColor = true
            };
            _btnRemoteDutyDn.Click += BtnRemoteDutyDn_Click;

            _dashboardPanel.Controls.Add(_btnRemoteMode);
            _dashboardPanel.Controls.Add(_btnRemoteFreqUp);
            _dashboardPanel.Controls.Add(_btnRemoteFreqDn);
            _dashboardPanel.Controls.Add(_btnRemoteDutyUp);
            _dashboardPanel.Controls.Add(_btnRemoteDutyDn);

            // 日志文本框（初始隐藏）
            _txtStatusLog = new TextBox
            {
                Location = new Point(8, 46),
                Size = new Size(580, 102),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 8F),
                Visible = false
            };

            _grpEsp32.Controls.Add(_btnToggleView);
            _grpEsp32.Controls.Add(_dashboardPanel);
            _grpEsp32.Controls.Add(_txtStatusLog);
            Controls.Add(_grpEsp32);
        }

        // ---- 风扇曲线区 (y=330, h=240) ----

        private void BuildFanCurveSection()
        {
            _grpFanCurve = new GroupBox
            {
                Text = "风扇曲线配置",
                Location = new Point(10, 330),
                Size = new Size(700, 240),
                Font = this.Font
            };

            _fanCurveGrid = new DataGridView
            {
                Location = new Point(10, 22),
                Size = new Size(240, 200),
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                RowHeadersVisible = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            };
            _fanCurveGrid.Columns.Add("TempCol", "温度 (°C)");
            _fanCurveGrid.Columns.Add("DutyCol", "占空比 (%)");
            _fanCurveGrid.Columns[0].Width = 105;
            _fanCurveGrid.Columns[1].Width = 105;
            _fanCurveGrid.Rows.Add(0.0f, 20);
            _fanCurveGrid.Rows.Add(35.0f, 25);
            _fanCurveGrid.Rows.Add(50.0f, 40);
            _fanCurveGrid.Rows.Add(65.0f, 70);
            _fanCurveGrid.Rows.Add(80.0f, 90);
            _fanCurveGrid.Rows.Add(100.0f, 100);

            _btnSendCurve = new Button
            {
                Text = "发送曲线",
                Location = new Point(285, 20),
                Size = new Size(105, 36),
                UseVisualStyleBackColor = true
            };
            _btnSendCurve.Click += BtnSendCurve_Click;

            _btnReadCurve = new Button
            {
                Text = "读取曲线",
                Location = new Point(285, 66),
                Size = new Size(105, 36),
                UseVisualStyleBackColor = true
            };
            _btnReadCurve.Click += BtnReadCurve_Click;

            _btnQueryStatus = new Button
            {
                Text = "查询状态",
                Location = new Point(285, 112),
                Size = new Size(105, 36),
                UseVisualStyleBackColor = true
            };
            _btnQueryStatus.Click += BtnQueryStatus_Click;

            _grpFanCurve.Controls.Add(_fanCurveGrid);
            _grpFanCurve.Controls.Add(_btnSendCurve);
            _grpFanCurve.Controls.Add(_btnReadCurve);
            _grpFanCurve.Controls.Add(_btnQueryStatus);
            Controls.Add(_grpFanCurve);
        }

        // ---- 仪表盘辅助 ----

        private void DashLabel(string text, int x, int y, Font font)
        {
            _dashboardPanel.Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, y),
                Font = font,
                AutoSize = true
            });
        }

        private Label DashValue(string text, int x, int y, Font font, Color color, int minWidth)
        {
            var lbl = new Label
            {
                Text = text,
                Location = new Point(x, y),
                Font = font,
                ForeColor = color,
                AutoSize = true,
                MinimumSize = new Size(minWidth, 22),
                TextAlign = ContentAlignment.MiddleLeft
            };
            _dashboardPanel.Controls.Add(lbl);
            return lbl;
        }

        private void ToggleResponseView()
        {
            _isDashboardMode = !_isDashboardMode;
            _dashboardPanel.Visible = _isDashboardMode;
            _txtStatusLog.Visible = !_isDashboardMode;
            _btnToggleView.Text = _isDashboardMode ? "日志模式" : "仪表盘";
        }

        // ---- 数据更新 ----

        private void UpdateDashboard(StatusData s)
        {
            if (!_isDashboardMode) return;
            _lastStatus = s;

            string modeStr;
            switch (s.Mode)
            {
                case 1: modeStr = "静音"; break;
                case 2: modeStr = "正常"; break;
                case 3: modeStr = "Turbo"; break;
                case 4: modeStr = "手动"; break;
                default: modeStr = "?"; break;
            }
            _lblDashMode.Text = modeStr;
            _lblDashFan.Text = $"{s.DutyPercent}%";
            _lblDashFreq.Text = $"{s.FreqHz / 1000}kHz";
            _lblDashCpuTemp.Text = $"{s.CpuTemp:F1} °C";
            _lblDashGpuTemp.Text = $"{s.GpuTemp:F1} °C";

            _lblDashCpuOk.Text = s.CpuValid ? "✓" : "✗";
            _lblDashCpuOk.ForeColor = s.CpuValid ? Color.Green : Color.Red;
            _lblDashGpuOk.Text = s.GpuValid ? "✓" : "✗";
            _lblDashGpuOk.ForeColor = s.GpuValid ? Color.Green : Color.Red;

            _lblDashUpdate.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void AppendStatusLog(string text)
        {
            _txtStatusLog.AppendText(text + Environment.NewLine);
            if (_txtStatusLog.Lines.Length > 200)
            {
                var lines = _txtStatusLog.Lines;
                var recent = new string[100];
                Array.Copy(lines, lines.Length - 100, recent, 0, 100);
                _txtStatusLog.Lines = recent;
            }
        }
    }
}
