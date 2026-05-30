using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CPUwenduhuoqu
{
    partial class MainForm
    {
        private IContainer components = null;

        // ---- 基础控件 ----
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
        private CheckBox checkBoxMinimizeToTray;
        private Label lblRefreshHint;

        // ---- 固件状态区 ----
        private GroupBox grpEsp32;
        private Panel dashboardPanel;
        private Label lblDashMode, lblDashFan, lblDashFreq;
        private Label lblDashCpuTemp, lblDashGpuTemp;
        private Label lblDashCpuOk, lblDashGpuOk;
        private Label lblDashUpdate;
        private TextBox txtStatusLog;
        private Button btnToggleView;
        private Button btnRemoteMode, btnRemoteFreqUp, btnRemoteFreqDn;
        private Button btnRemoteDutyUp, btnRemoteDutyDn;

        // ---- 风扇曲线区 ----
        private GroupBox grpFanCurve;
        private DataGridView fanCurveGrid;
        private Button btnSendCurve;
        private Button btnReadCurve;
        private Button btnQueryStatus;

        // ---- 运行时状态 ----
        private bool isDashboardMode = true;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new Container();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(MainForm));

            // ================================================================
            // 控件实例化
            // ================================================================

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
            checkBoxMinimizeToTray = new CheckBox();
            lblRefreshHint = new Label();

            grpEsp32 = new GroupBox();
            dashboardPanel = new Panel();
            lblDashMode = new Label();
            lblDashFan = new Label();
            lblDashFreq = new Label();
            lblDashCpuTemp = new Label();
            lblDashGpuTemp = new Label();
            lblDashCpuOk = new Label();
            lblDashGpuOk = new Label();
            lblDashUpdate = new Label();
            txtStatusLog = new TextBox();
            btnToggleView = new Button();
            btnRemoteMode = new Button();
            btnRemoteFreqUp = new Button();
            btnRemoteFreqDn = new Button();
            btnRemoteDutyUp = new Button();
            btnRemoteDutyDn = new Button();

            grpFanCurve = new GroupBox();
            fanCurveGrid = new DataGridView();
            btnSendCurve = new Button();
            btnReadCurve = new Button();
            btnQueryStatus = new Button();

            statusStrip.SuspendLayout();
            grpEsp32.SuspendLayout();
            dashboardPanel.SuspendLayout();
            grpFanCurve.SuspendLayout();
            SuspendLayout();

            // ================================================================
            // 温度标签 (Row 0, y=10)
            // ================================================================

            cpuTempLabel.AutoSize = true;
            cpuTempLabel.Location = new Point(12, 10);
            cpuTempLabel.Margin = new Padding(4, 0, 4, 0);
            cpuTempLabel.Name = "cpuTempLabel";
            cpuTempLabel.Size = new Size(98, 18);
            cpuTempLabel.TabIndex = 0;
            cpuTempLabel.Text = "CPU 温度: ";

            gpuTempLabel.AutoSize = true;
            gpuTempLabel.Location = new Point(380, 10);
            gpuTempLabel.Margin = new Padding(4, 0, 4, 0);
            gpuTempLabel.Name = "gpuTempLabel";
            gpuTempLabel.Size = new Size(98, 18);
            gpuTempLabel.TabIndex = 1;
            gpuTempLabel.Text = "GPU 温度: ";

            // ================================================================
            // 串口 + 刷新间隔 (Row 1, y=40)
            // ================================================================

            comboBoxSerialPorts.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxSerialPorts.FormattingEnabled = true;
            comboBoxSerialPorts.Location = new Point(12, 40);
            comboBoxSerialPorts.Margin = new Padding(4);
            comboBoxSerialPorts.Name = "comboBoxSerialPorts";
            comboBoxSerialPorts.Size = new Size(120, 26);
            comboBoxSerialPorts.TabIndex = 2;

            buttonConnect.Location = new Point(140, 40);
            buttonConnect.Margin = new Padding(4);
            buttonConnect.Name = "buttonConnect";
            buttonConnect.Size = new Size(70, 32);
            buttonConnect.TabIndex = 3;
            buttonConnect.Text = "连接";
            buttonConnect.UseVisualStyleBackColor = true;
            buttonConnect.Click += new EventHandler(buttonConnect_Click);

            labelConnectionStatus.AutoSize = true;
            labelConnectionStatus.Location = new Point(218, 44);
            labelConnectionStatus.Margin = new Padding(4, 0, 4, 0);
            labelConnectionStatus.Name = "labelConnectionStatus";
            labelConnectionStatus.Size = new Size(44, 18);
            labelConnectionStatus.TabIndex = 4;
            labelConnectionStatus.Text = "已断开";

            lblRefreshHint.AutoSize = true;
            lblRefreshHint.Location = new Point(300, 44);
            lblRefreshHint.Name = "lblRefreshHint";
            lblRefreshHint.Size = new Size(76, 18);
            lblRefreshHint.TabIndex = 20;
            lblRefreshHint.Text = "刷新间隔:";

            domainUpDownSelectRefreshTime.Location = new Point(385, 40);
            domainUpDownSelectRefreshTime.Name = "domainUpDownSelectRefreshTime";
            domainUpDownSelectRefreshTime.Size = new Size(56, 26);
            domainUpDownSelectRefreshTime.TabIndex = 12;

            buttonConfirmRefreshTime.Location = new Point(448, 40);
            buttonConfirmRefreshTime.Name = "buttonConfirmRefreshTime";
            buttonConfirmRefreshTime.Size = new Size(55, 32);
            buttonConfirmRefreshTime.TabIndex = 13;
            buttonConfirmRefreshTime.Text = "确认";
            buttonConfirmRefreshTime.UseVisualStyleBackColor = true;
            buttonConfirmRefreshTime.Click += new EventHandler(buttonConfirmRefreshTime_Click);

            // ================================================================
            // AIDA64 + 最小化到托盘 (Row 2a, y=76)
            // ================================================================

            checkBoxUseAida64Mode.AutoSize = true;
            checkBoxUseAida64Mode.Location = new Point(12, 76);
            checkBoxUseAida64Mode.Name = "checkBoxUseAida64Mode";
            checkBoxUseAida64Mode.Size = new Size(260, 22);
            checkBoxUseAida64Mode.TabIndex = 5;
            checkBoxUseAida64Mode.Text = "使用AIDA64获取硬件温度信息";
            checkBoxUseAida64Mode.UseVisualStyleBackColor = true;
            checkBoxUseAida64Mode.CheckedChanged += new EventHandler(checkBox_useAida64Mode);

            checkBoxMinimizeToTray.AutoSize = true;
            checkBoxMinimizeToTray.Location = new Point(420, 76);
            checkBoxMinimizeToTray.Name = "checkBoxMinimizeToTray";
            checkBoxMinimizeToTray.Size = new Size(124, 22);
            checkBoxMinimizeToTray.TabIndex = 21;
            checkBoxMinimizeToTray.Text = "最小化到托盘";
            checkBoxMinimizeToTray.CheckedChanged += new EventHandler(checkBoxMinimizeToTray_CheckedChanged);

            // ================================================================
            // AIDA64 传感器选择 (Row 2b, y=104)
            // ================================================================

            labelNoticeCpuMonitor.AutoSize = true;
            labelNoticeCpuMonitor.ForeColor = Color.Gray;
            labelNoticeCpuMonitor.Location = new Point(12, 108);
            labelNoticeCpuMonitor.Name = "labelNoticeCpuMonitor";
            labelNoticeCpuMonitor.Size = new Size(88, 18);
            labelNoticeCpuMonitor.TabIndex = 8;
            labelNoticeCpuMonitor.Text = "CPU传感器:";

            comboBoxChooseCpuMonitor.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxChooseCpuMonitor.Enabled = false;
            comboBoxChooseCpuMonitor.ForeColor = Color.Black;
            comboBoxChooseCpuMonitor.FormattingEnabled = true;
            comboBoxChooseCpuMonitor.Location = new Point(98, 104);
            comboBoxChooseCpuMonitor.Name = "comboBoxChooseCpuMonitor";
            comboBoxChooseCpuMonitor.Size = new Size(155, 26);
            comboBoxChooseCpuMonitor.TabIndex = 7;

            labelNoticeGpuMonitor.AutoSize = true;
            labelNoticeGpuMonitor.ForeColor = Color.Gray;
            labelNoticeGpuMonitor.Location = new Point(262, 108);
            labelNoticeGpuMonitor.Name = "labelNoticeGpuMonitor";
            labelNoticeGpuMonitor.Size = new Size(88, 18);
            labelNoticeGpuMonitor.TabIndex = 9;
            labelNoticeGpuMonitor.Text = "GPU传感器:";

            comboBoxChooseGpuMonitor.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxChooseGpuMonitor.Enabled = false;
            comboBoxChooseGpuMonitor.ForeColor = Color.Black;
            comboBoxChooseGpuMonitor.FormattingEnabled = true;
            comboBoxChooseGpuMonitor.Location = new Point(348, 104);
            comboBoxChooseGpuMonitor.Name = "comboBoxChooseGpuMonitor";
            comboBoxChooseGpuMonitor.Size = new Size(155, 26);
            comboBoxChooseGpuMonitor.TabIndex = 7;

            buttonUseChosenMonitor.Enabled = false;
            buttonUseChosenMonitor.Location = new Point(512, 102);
            buttonUseChosenMonitor.Name = "buttonUseChosenMonitor";
            buttonUseChosenMonitor.Size = new Size(55, 32);
            buttonUseChosenMonitor.TabIndex = 10;
            buttonUseChosenMonitor.Text = "确认";
            buttonUseChosenMonitor.UseVisualStyleBackColor = true;
            buttonUseChosenMonitor.Click += new EventHandler(buttonUseChosenMonitor_Click);

            labelNoticeRefreshTimeAdjustmentWindow.AutoSize = true;
            labelNoticeRefreshTimeAdjustmentWindow.Location = new Point(10, 141);
            labelNoticeRefreshTimeAdjustmentWindow.Name = "labelNoticeRefreshTimeAdjustmentWindow";
            labelNoticeRefreshTimeAdjustmentWindow.Size = new Size(160, 18);
            labelNoticeRefreshTimeAdjustmentWindow.TabIndex = 14;
            labelNoticeRefreshTimeAdjustmentWindow.Text = "选择刷新时间（3-30s）";
            labelNoticeRefreshTimeAdjustmentWindow.Visible = false;

            // ================================================================
            // 状态栏
            // ================================================================

            statusStrip.ImageScalingSize = new Size(24, 24);
            statusStrip.Items.AddRange(new ToolStripItem[] {
                toolStripStatusAida64CpuMonitor,
                toolStripStatusAida64GpuMonitor });
            statusStrip.Location = new Point(0, 580);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(720, 31);
            statusStrip.TabIndex = 11;
            statusStrip.Text = "statusStrip";

            toolStripStatusAida64CpuMonitor.ForeColor = Color.Gray;
            toolStripStatusAida64CpuMonitor.Name = "toolStripStatusAida64CpuMonitor";
            toolStripStatusAida64CpuMonitor.Size = new Size(202, 24);
            toolStripStatusAida64CpuMonitor.Text = "AIDA64 CPU 传感器: ";

            toolStripStatusAida64GpuMonitor.ForeColor = Color.Gray;
            toolStripStatusAida64GpuMonitor.Name = "toolStripStatusAida64GpuMonitor";
            toolStripStatusAida64GpuMonitor.Size = new Size(203, 24);
            toolStripStatusAida64GpuMonitor.Text = "AIDA64 GPU 传感器: ";

            // ================================================================
            // 托盘图标
            // ================================================================

            notifyIcon.Text = "硬件温度监测";
            notifyIcon.Icon = Properties.Resources.MainIcon;
            notifyIcon.Visible = false;
            notifyIcon.Click += new EventHandler(NotifyIcon_Click);

            // ================================================================
            // 固件状态区 (y=136, h=190)
            // ================================================================

            grpEsp32.Location = new Point(10, 136);
            grpEsp32.Name = "grpEsp32";
            grpEsp32.Size = new Size(650, 190);
            grpEsp32.TabIndex = 30;
            grpEsp32.TabStop = false;
            grpEsp32.Text = "固件状态";

            btnToggleView.Location = new Point(544, 14);
            btnToggleView.Name = "btnToggleView";
            btnToggleView.Size = new Size(95, 32);
            btnToggleView.TabIndex = 0;
            btnToggleView.Text = "日志模式";
            btnToggleView.UseVisualStyleBackColor = true;
            btnToggleView.Click += new EventHandler(btnToggleView_Click);

            dashboardPanel.Location = new Point(8, 24);
            dashboardPanel.Name = "dashboardPanel";
            dashboardPanel.Size = new Size(634, 140);
            dashboardPanel.TabIndex = 1;

            // -- 仪表盘标签 (运行时通过 BuildDashboard 填充) --

            lblDashMode.Font = new Font("Consolas", 11F, FontStyle.Bold);
            lblDashMode.ForeColor = Color.DarkBlue;
            lblDashMode.Location = new Point(64, 8);
            lblDashMode.Name = "lblDashMode";
            lblDashMode.Size = new Size(80, 22);
            lblDashMode.Text = "--";

            lblDashFan.Font = new Font("Consolas", 11F, FontStyle.Bold);
            lblDashFan.ForeColor = Color.DarkGreen;
            lblDashFan.Location = new Point(222, 8);
            lblDashFan.Name = "lblDashFan";
            lblDashFan.Size = new Size(70, 22);
            lblDashFan.Text = "--";

            lblDashFreq.Font = new Font("Consolas", 11F, FontStyle.Bold);
            lblDashFreq.ForeColor = Color.DarkGreen;
            lblDashFreq.Location = new Point(382, 8);
            lblDashFreq.Name = "lblDashFreq";
            lblDashFreq.Size = new Size(80, 22);
            lblDashFreq.Text = "--";

            lblDashCpuTemp.Font = new Font("Consolas", 11F, FontStyle.Bold);
            lblDashCpuTemp.ForeColor = Color.DarkRed;
            lblDashCpuTemp.Location = new Point(66, 50);
            lblDashCpuTemp.Name = "lblDashCpuTemp";
            lblDashCpuTemp.Size = new Size(110, 22);
            lblDashCpuTemp.Text = "--.- °C";

            lblDashCpuOk.Location = new Point(172, 52);
            lblDashCpuOk.Name = "lblDashCpuOk";
            lblDashCpuOk.Size = new Size(28, 20);
            lblDashCpuOk.ForeColor = Color.Gray;

            lblDashGpuTemp.Font = new Font("Consolas", 11F, FontStyle.Bold);
            lblDashGpuTemp.ForeColor = Color.DarkRed;
            lblDashGpuTemp.Location = new Point(286, 50);
            lblDashGpuTemp.Name = "lblDashGpuTemp";
            lblDashGpuTemp.Size = new Size(110, 22);
            lblDashGpuTemp.Text = "--.- °C";

            lblDashGpuOk.Location = new Point(392, 52);
            lblDashGpuOk.Name = "lblDashGpuOk";
            lblDashGpuOk.Size = new Size(28, 20);
            lblDashGpuOk.ForeColor = Color.Gray;

            lblDashUpdate.ForeColor = Color.Gray;
            lblDashUpdate.Location = new Point(76, 82);
            lblDashUpdate.Name = "lblDashUpdate";
            lblDashUpdate.Size = new Size(120, 18);
            lblDashUpdate.Text = "等待数据...";

            // -- 远程控制按钮 --

            btnRemoteMode.Location = new Point(10, 110);
            btnRemoteMode.Name = "btnRemoteMode";
            btnRemoteMode.Size = new Size(62, 30);
            btnRemoteMode.TabIndex = 2;
            btnRemoteMode.Text = "模式";
            btnRemoteMode.UseVisualStyleBackColor = true;
            btnRemoteMode.Click += new EventHandler(BtnRemoteMode_Click);

            btnRemoteFreqUp.Location = new Point(72, 110);
            btnRemoteFreqUp.Name = "btnRemoteFreqUp";
            btnRemoteFreqUp.Size = new Size(62, 30);
            btnRemoteFreqUp.TabIndex = 3;
            btnRemoteFreqUp.Text = "频率+";
            btnRemoteFreqUp.UseVisualStyleBackColor = true;
            btnRemoteFreqUp.Click += new EventHandler(BtnRemoteFreqUp_Click);

            btnRemoteFreqDn.Location = new Point(134, 110);
            btnRemoteFreqDn.Name = "btnRemoteFreqDn";
            btnRemoteFreqDn.Size = new Size(62, 30);
            btnRemoteFreqDn.TabIndex = 4;
            btnRemoteFreqDn.Text = "频率-";
            btnRemoteFreqDn.UseVisualStyleBackColor = true;
            btnRemoteFreqDn.Click += new EventHandler(BtnRemoteFreqDn_Click);

            btnRemoteDutyUp.Location = new Point(196, 110);
            btnRemoteDutyUp.Name = "btnRemoteDutyUp";
            btnRemoteDutyUp.Size = new Size(62, 30);
            btnRemoteDutyUp.TabIndex = 5;
            btnRemoteDutyUp.Text = "占空+";
            btnRemoteDutyUp.UseVisualStyleBackColor = true;
            btnRemoteDutyUp.Click += new EventHandler(BtnRemoteDutyUp_Click);

            btnRemoteDutyDn.Location = new Point(258, 110);
            btnRemoteDutyDn.Name = "btnRemoteDutyDn";
            btnRemoteDutyDn.Size = new Size(62, 30);
            btnRemoteDutyDn.TabIndex = 6;
            btnRemoteDutyDn.Text = "占空-";
            btnRemoteDutyDn.UseVisualStyleBackColor = true;
            btnRemoteDutyDn.Click += new EventHandler(BtnRemoteDutyDn_Click);

            // -- 日志文本框 --

            txtStatusLog.Location = new Point(8, 46);
            txtStatusLog.Multiline = true;
            txtStatusLog.Name = "txtStatusLog";
            txtStatusLog.ReadOnly = true;
            txtStatusLog.ScrollBars = ScrollBars.Vertical;
            txtStatusLog.Size = new Size(580, 102);
            txtStatusLog.TabIndex = 7;
            txtStatusLog.Visible = false;

            // -- 组装固件状态区 --

            dashboardPanel.Controls.Add(lblDashUpdate);
            dashboardPanel.Controls.Add(lblDashGpuOk);
            dashboardPanel.Controls.Add(lblDashGpuTemp);
            dashboardPanel.Controls.Add(lblDashCpuOk);
            dashboardPanel.Controls.Add(lblDashCpuTemp);
            dashboardPanel.Controls.Add(lblDashFreq);
            dashboardPanel.Controls.Add(lblDashFan);
            dashboardPanel.Controls.Add(lblDashMode);
            dashboardPanel.Controls.Add(btnRemoteDutyDn);
            dashboardPanel.Controls.Add(btnRemoteDutyUp);
            dashboardPanel.Controls.Add(btnRemoteFreqDn);
            dashboardPanel.Controls.Add(btnRemoteFreqUp);
            dashboardPanel.Controls.Add(btnRemoteMode);

            grpEsp32.Controls.Add(btnToggleView);
            grpEsp32.Controls.Add(dashboardPanel);
            grpEsp32.Controls.Add(txtStatusLog);

            // ================================================================
            // 风扇曲线区 (y=330, h=240)
            // ================================================================

            grpFanCurve.Location = new Point(10, 330);
            grpFanCurve.Name = "grpFanCurve";
            grpFanCurve.Size = new Size(700, 240);
            grpFanCurve.TabIndex = 40;
            grpFanCurve.TabStop = false;
            grpFanCurve.Text = "风扇曲线配置";

            fanCurveGrid.AllowUserToAddRows = true;
            fanCurveGrid.AllowUserToDeleteRows = true;
            fanCurveGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            fanCurveGrid.Location = new Point(10, 22);
            fanCurveGrid.Name = "fanCurveGrid";
            fanCurveGrid.RowHeadersVisible = false;
            fanCurveGrid.Size = new Size(240, 200);
            fanCurveGrid.TabIndex = 0;

            btnSendCurve.Location = new Point(285, 20);
            btnSendCurve.Name = "btnSendCurve";
            btnSendCurve.Size = new Size(105, 36);
            btnSendCurve.TabIndex = 1;
            btnSendCurve.Text = "发送曲线";
            btnSendCurve.UseVisualStyleBackColor = true;
            btnSendCurve.Click += new EventHandler(BtnSendCurve_Click);

            btnReadCurve.Location = new Point(285, 66);
            btnReadCurve.Name = "btnReadCurve";
            btnReadCurve.Size = new Size(105, 36);
            btnReadCurve.TabIndex = 2;
            btnReadCurve.Text = "读取曲线";
            btnReadCurve.UseVisualStyleBackColor = true;
            btnReadCurve.Click += new EventHandler(BtnReadCurve_Click);

            btnQueryStatus.Location = new Point(285, 112);
            btnQueryStatus.Name = "btnQueryStatus";
            btnQueryStatus.Size = new Size(105, 36);
            btnQueryStatus.TabIndex = 3;
            btnQueryStatus.Text = "查询状态";
            btnQueryStatus.UseVisualStyleBackColor = true;
            btnQueryStatus.Click += new EventHandler(BtnQueryStatus_Click);

            grpFanCurve.Controls.Add(fanCurveGrid);
            grpFanCurve.Controls.Add(btnSendCurve);
            grpFanCurve.Controls.Add(btnReadCurve);
            grpFanCurve.Controls.Add(btnQueryStatus);

            // ================================================================
            // 窗体属性
            // ================================================================

            AutoScaleDimensions = new SizeF(9F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(720, 630);
            Font = new Font("Microsoft YaHei UI", 9F);

            Controls.Add(grpFanCurve);
            Controls.Add(grpEsp32);
            Controls.Add(checkBoxMinimizeToTray);
            Controls.Add(lblRefreshHint);
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

            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)(resources.GetObject("$this.Icon"));
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "MainForm";
            Text = "硬件温度监测";

            FormClosing += new FormClosingEventHandler(MainForm_FormClosing);
            Load += new EventHandler(MainForm_Load);
            Resize += new EventHandler(MainForm_Resize);

            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            grpEsp32.ResumeLayout(false);
            grpEsp32.PerformLayout();
            dashboardPanel.ResumeLayout(false);
            dashboardPanel.PerformLayout();
            grpFanCurve.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}