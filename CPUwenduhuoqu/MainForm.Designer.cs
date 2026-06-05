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
        private Button btnSafetyReset;

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
            if (disposing)
            {
                components?.Dispose();
                _headFont?.Dispose();
                _tinyFont?.Dispose();
                _dashFont?.Dispose();
                _btnFont?.Dispose();
                _logFont?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.cpuTempLabel = new System.Windows.Forms.Label();
            this.gpuTempLabel = new System.Windows.Forms.Label();
            this.comboBoxSerialPorts = new System.Windows.Forms.ComboBox();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.labelConnectionStatus = new System.Windows.Forms.Label();
            this.checkBoxUseAida64Mode = new System.Windows.Forms.CheckBox();
            this.comboBoxChooseCpuMonitor = new System.Windows.Forms.ComboBox();
            this.comboBoxChooseGpuMonitor = new System.Windows.Forms.ComboBox();
            this.labelNoticeCpuMonitor = new System.Windows.Forms.Label();
            this.labelNoticeGpuMonitor = new System.Windows.Forms.Label();
            this.buttonUseChosenMonitor = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusAida64CpuMonitor = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusAida64GpuMonitor = new System.Windows.Forms.ToolStripStatusLabel();
            this.domainUpDownSelectRefreshTime = new System.Windows.Forms.DomainUpDown();
            this.buttonConfirmRefreshTime = new System.Windows.Forms.Button();
            this.labelNoticeRefreshTimeAdjustmentWindow = new System.Windows.Forms.Label();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.checkBoxMinimizeToTray = new System.Windows.Forms.CheckBox();
            this.lblRefreshHint = new System.Windows.Forms.Label();
            this.grpEsp32 = new System.Windows.Forms.GroupBox();
            this.btnToggleView = new System.Windows.Forms.Button();
            this.dashboardPanel = new System.Windows.Forms.Panel();
            this.lblDashUpdate = new System.Windows.Forms.Label();
            this.lblDashGpuOk = new System.Windows.Forms.Label();
            this.lblDashGpuTemp = new System.Windows.Forms.Label();
            this.lblDashCpuOk = new System.Windows.Forms.Label();
            this.btnSafetyReset = new System.Windows.Forms.Button();
            this.lblDashCpuTemp = new System.Windows.Forms.Label();
            this.lblDashFreq = new System.Windows.Forms.Label();
            this.lblDashFan = new System.Windows.Forms.Label();
            this.lblDashMode = new System.Windows.Forms.Label();
            this.btnRemoteDutyDn = new System.Windows.Forms.Button();
            this.btnRemoteDutyUp = new System.Windows.Forms.Button();
            this.btnRemoteFreqDn = new System.Windows.Forms.Button();
            this.btnRemoteFreqUp = new System.Windows.Forms.Button();
            this.btnRemoteMode = new System.Windows.Forms.Button();
            this.txtStatusLog = new System.Windows.Forms.TextBox();
            this.grpFanCurve = new System.Windows.Forms.GroupBox();
            this.fanCurveGrid = new System.Windows.Forms.DataGridView();
            this.btnSendCurve = new System.Windows.Forms.Button();
            this.btnReadCurve = new System.Windows.Forms.Button();
            this.btnQueryStatus = new System.Windows.Forms.Button();
            this.statusStrip.SuspendLayout();
            this.grpEsp32.SuspendLayout();
            this.dashboardPanel.SuspendLayout();
            this.grpFanCurve.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fanCurveGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // cpuTempLabel
            // 
            this.cpuTempLabel.AutoSize = true;
            this.cpuTempLabel.Location = new System.Drawing.Point(12, 10);
            this.cpuTempLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.cpuTempLabel.Name = "cpuTempLabel";
            this.cpuTempLabel.Size = new System.Drawing.Size(96, 24);
            this.cpuTempLabel.TabIndex = 0;
            this.cpuTempLabel.Text = "CPU 温度: ";
            // 
            // gpuTempLabel
            // 
            this.gpuTempLabel.AutoSize = true;
            this.gpuTempLabel.Location = new System.Drawing.Point(380, 10);
            this.gpuTempLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.gpuTempLabel.Name = "gpuTempLabel";
            this.gpuTempLabel.Size = new System.Drawing.Size(97, 24);
            this.gpuTempLabel.TabIndex = 1;
            this.gpuTempLabel.Text = "GPU 温度: ";
            // 
            // comboBoxSerialPorts
            // 
            this.comboBoxSerialPorts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSerialPorts.FormattingEnabled = true;
            this.comboBoxSerialPorts.Location = new System.Drawing.Point(12, 40);
            this.comboBoxSerialPorts.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxSerialPorts.Name = "comboBoxSerialPorts";
            this.comboBoxSerialPorts.Size = new System.Drawing.Size(120, 32);
            this.comboBoxSerialPorts.TabIndex = 2;
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(140, 40);
            this.buttonConnect.Margin = new System.Windows.Forms.Padding(4);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(70, 32);
            this.buttonConnect.TabIndex = 3;
            this.buttonConnect.Text = "连接";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // labelConnectionStatus
            // 
            this.labelConnectionStatus.AutoSize = true;
            this.labelConnectionStatus.Location = new System.Drawing.Point(212, 44);
            this.labelConnectionStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelConnectionStatus.Name = "labelConnectionStatus";
            this.labelConnectionStatus.Size = new System.Drawing.Size(64, 24);
            this.labelConnectionStatus.TabIndex = 4;
            this.labelConnectionStatus.Text = "已断开";
            // 
            // checkBoxUseAida64Mode
            // 
            this.checkBoxUseAida64Mode.AutoSize = true;
            this.checkBoxUseAida64Mode.Location = new System.Drawing.Point(12, 76);
            this.checkBoxUseAida64Mode.Name = "checkBoxUseAida64Mode";
            this.checkBoxUseAida64Mode.Size = new System.Drawing.Size(283, 28);
            this.checkBoxUseAida64Mode.TabIndex = 5;
            this.checkBoxUseAida64Mode.Text = "使用AIDA64获取硬件温度信息";
            this.checkBoxUseAida64Mode.UseVisualStyleBackColor = true;
            this.checkBoxUseAida64Mode.CheckedChanged += new System.EventHandler(this.checkBoxUseAida64Mode_CheckedChanged);
            // 
            // comboBoxChooseCpuMonitor
            // 
            this.comboBoxChooseCpuMonitor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxChooseCpuMonitor.Enabled = false;
            this.comboBoxChooseCpuMonitor.ForeColor = System.Drawing.Color.Black;
            this.comboBoxChooseCpuMonitor.FormattingEnabled = true;
            this.comboBoxChooseCpuMonitor.Location = new System.Drawing.Point(116, 104);
            this.comboBoxChooseCpuMonitor.Name = "comboBoxChooseCpuMonitor";
            this.comboBoxChooseCpuMonitor.Size = new System.Drawing.Size(155, 32);
            this.comboBoxChooseCpuMonitor.TabIndex = 7;
            // 
            // comboBoxChooseGpuMonitor
            // 
            this.comboBoxChooseGpuMonitor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxChooseGpuMonitor.Enabled = false;
            this.comboBoxChooseGpuMonitor.ForeColor = System.Drawing.Color.Black;
            this.comboBoxChooseGpuMonitor.FormattingEnabled = true;
            this.comboBoxChooseGpuMonitor.Location = new System.Drawing.Point(378, 104);
            this.comboBoxChooseGpuMonitor.Name = "comboBoxChooseGpuMonitor";
            this.comboBoxChooseGpuMonitor.Size = new System.Drawing.Size(155, 32);
            this.comboBoxChooseGpuMonitor.TabIndex = 7;
            // 
            // labelNoticeCpuMonitor
            // 
            this.labelNoticeCpuMonitor.AutoSize = true;
            this.labelNoticeCpuMonitor.ForeColor = System.Drawing.Color.Gray;
            this.labelNoticeCpuMonitor.Location = new System.Drawing.Point(12, 108);
            this.labelNoticeCpuMonitor.Name = "labelNoticeCpuMonitor";
            this.labelNoticeCpuMonitor.Size = new System.Drawing.Size(104, 24);
            this.labelNoticeCpuMonitor.TabIndex = 8;
            this.labelNoticeCpuMonitor.Text = "CPU传感器:";
            // 
            // labelNoticeGpuMonitor
            // 
            this.labelNoticeGpuMonitor.AutoSize = true;
            this.labelNoticeGpuMonitor.ForeColor = System.Drawing.Color.Gray;
            this.labelNoticeGpuMonitor.Location = new System.Drawing.Point(272, 108);
            this.labelNoticeGpuMonitor.Name = "labelNoticeGpuMonitor";
            this.labelNoticeGpuMonitor.Size = new System.Drawing.Size(105, 24);
            this.labelNoticeGpuMonitor.TabIndex = 9;
            this.labelNoticeGpuMonitor.Text = "GPU传感器:";
            // 
            // buttonUseChosenMonitor
            // 
            this.buttonUseChosenMonitor.Enabled = false;
            this.buttonUseChosenMonitor.Location = new System.Drawing.Point(536, 104);
            this.buttonUseChosenMonitor.Name = "buttonUseChosenMonitor";
            this.buttonUseChosenMonitor.Size = new System.Drawing.Size(55, 32);
            this.buttonUseChosenMonitor.TabIndex = 10;
            this.buttonUseChosenMonitor.Text = "确认";
            this.buttonUseChosenMonitor.UseVisualStyleBackColor = true;
            this.buttonUseChosenMonitor.Click += new System.EventHandler(this.buttonUseChosenMonitor_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusAida64CpuMonitor,
            this.toolStripStatusAida64GpuMonitor});
            this.statusStrip.Location = new System.Drawing.Point(0, 581);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(673, 31);
            this.statusStrip.TabIndex = 11;
            this.statusStrip.Text = "statusStrip";
            // 
            // toolStripStatusAida64CpuMonitor
            // 
            this.toolStripStatusAida64CpuMonitor.ForeColor = System.Drawing.Color.Gray;
            this.toolStripStatusAida64CpuMonitor.Name = "toolStripStatusAida64CpuMonitor";
            this.toolStripStatusAida64CpuMonitor.Size = new System.Drawing.Size(186, 24);
            this.toolStripStatusAida64CpuMonitor.Text = "AIDA64 CPU 传感器: ";
            // 
            // toolStripStatusAida64GpuMonitor
            // 
            this.toolStripStatusAida64GpuMonitor.ForeColor = System.Drawing.Color.Gray;
            this.toolStripStatusAida64GpuMonitor.Name = "toolStripStatusAida64GpuMonitor";
            this.toolStripStatusAida64GpuMonitor.Size = new System.Drawing.Size(187, 24);
            this.toolStripStatusAida64GpuMonitor.Text = "AIDA64 GPU 传感器: ";
            // 
            // domainUpDownSelectRefreshTime
            // 
            this.domainUpDownSelectRefreshTime.Location = new System.Drawing.Point(477, 40);
            this.domainUpDownSelectRefreshTime.Name = "domainUpDownSelectRefreshTime";
            this.domainUpDownSelectRefreshTime.Size = new System.Drawing.Size(56, 30);
            this.domainUpDownSelectRefreshTime.TabIndex = 12;
            // 
            // buttonConfirmRefreshTime
            // 
            this.buttonConfirmRefreshTime.Location = new System.Drawing.Point(536, 40);
            this.buttonConfirmRefreshTime.Name = "buttonConfirmRefreshTime";
            this.buttonConfirmRefreshTime.Size = new System.Drawing.Size(55, 32);
            this.buttonConfirmRefreshTime.TabIndex = 13;
            this.buttonConfirmRefreshTime.Text = "确认";
            this.buttonConfirmRefreshTime.UseVisualStyleBackColor = true;
            this.buttonConfirmRefreshTime.Click += new System.EventHandler(this.buttonConfirmRefreshTime_Click);
            // 
            // labelNoticeRefreshTimeAdjustmentWindow
            // 
            this.labelNoticeRefreshTimeAdjustmentWindow.AutoSize = true;
            this.labelNoticeRefreshTimeAdjustmentWindow.Location = new System.Drawing.Point(10, 141);
            this.labelNoticeRefreshTimeAdjustmentWindow.Name = "labelNoticeRefreshTimeAdjustmentWindow";
            this.labelNoticeRefreshTimeAdjustmentWindow.Size = new System.Drawing.Size(203, 24);
            this.labelNoticeRefreshTimeAdjustmentWindow.TabIndex = 14;
            this.labelNoticeRefreshTimeAdjustmentWindow.Text = "选择刷新时间（3-30s）";
            this.labelNoticeRefreshTimeAdjustmentWindow.Visible = false;
            // 
            // notifyIcon
            // 
            this.notifyIcon.Icon = global::CPUwenduhuoqu.Properties.Resources.MainIcon;
            this.notifyIcon.Text = "硬件温度监测";
            this.notifyIcon.Click += new System.EventHandler(this.NotifyIcon_Click);
            // 
            // checkBoxMinimizeToTray
            // 
            this.checkBoxMinimizeToTray.AutoSize = true;
            this.checkBoxMinimizeToTray.Location = new System.Drawing.Point(420, 76);
            this.checkBoxMinimizeToTray.Name = "checkBoxMinimizeToTray";
            this.checkBoxMinimizeToTray.Size = new System.Drawing.Size(144, 28);
            this.checkBoxMinimizeToTray.TabIndex = 21;
            this.checkBoxMinimizeToTray.Text = "最小化到托盘";
            this.checkBoxMinimizeToTray.CheckedChanged += new System.EventHandler(this.checkBoxMinimizeToTray_CheckedChanged);
            // 
            // lblRefreshHint
            // 
            this.lblRefreshHint.AutoSize = true;
            this.lblRefreshHint.Location = new System.Drawing.Point(380, 43);
            this.lblRefreshHint.Name = "lblRefreshHint";
            this.lblRefreshHint.Size = new System.Drawing.Size(86, 24);
            this.lblRefreshHint.TabIndex = 20;
            this.lblRefreshHint.Text = "刷新间隔:";
            // 
            // grpEsp32
            // 
            this.grpEsp32.Controls.Add(this.btnToggleView);
            this.grpEsp32.Controls.Add(this.dashboardPanel);
            this.grpEsp32.Controls.Add(this.txtStatusLog);
            this.grpEsp32.Location = new System.Drawing.Point(10, 136);
            this.grpEsp32.Name = "grpEsp32";
            this.grpEsp32.Size = new System.Drawing.Size(650, 190);
            this.grpEsp32.TabIndex = 30;
            this.grpEsp32.TabStop = false;
            this.grpEsp32.Text = "固件状态";
            // 
            // btnToggleView
            // 
            this.btnToggleView.Location = new System.Drawing.Point(544, 14);
            this.btnToggleView.Name = "btnToggleView";
            this.btnToggleView.Size = new System.Drawing.Size(95, 32);
            this.btnToggleView.TabIndex = 0;
            this.btnToggleView.Text = "日志模式";
            this.btnToggleView.UseVisualStyleBackColor = true;
            this.btnToggleView.Click += new System.EventHandler(this.btnToggleView_Click);
            // 
            // dashboardPanel
            // 
            this.dashboardPanel.Controls.Add(this.lblDashUpdate);
            this.dashboardPanel.Controls.Add(this.lblDashGpuOk);
            this.dashboardPanel.Controls.Add(this.lblDashGpuTemp);
            this.dashboardPanel.Controls.Add(this.lblDashCpuOk);
            this.dashboardPanel.Controls.Add(this.lblDashCpuTemp);
            this.dashboardPanel.Controls.Add(this.lblDashFreq);
            this.dashboardPanel.Controls.Add(this.lblDashFan);
            this.dashboardPanel.Controls.Add(this.lblDashMode);
            this.dashboardPanel.Controls.Add(this.btnRemoteDutyDn);
            this.dashboardPanel.Controls.Add(this.btnRemoteDutyUp);
            this.dashboardPanel.Controls.Add(this.btnRemoteFreqDn);
            this.dashboardPanel.Controls.Add(this.btnRemoteFreqUp);
            this.dashboardPanel.Controls.Add(this.btnRemoteMode);
            this.dashboardPanel.Location = new System.Drawing.Point(8, 24);
            this.dashboardPanel.Name = "dashboardPanel";
            this.dashboardPanel.Size = new System.Drawing.Size(634, 140);
            this.dashboardPanel.TabIndex = 1;
            // 
            // lblDashUpdate
            // 
            this.lblDashUpdate.ForeColor = System.Drawing.Color.Gray;
            this.lblDashUpdate.Location = new System.Drawing.Point(76, 82);
            this.lblDashUpdate.Name = "lblDashUpdate";
            this.lblDashUpdate.Size = new System.Drawing.Size(120, 25);
            this.lblDashUpdate.TabIndex = 0;
            this.lblDashUpdate.Text = "等待数据...";
            // 
            // lblDashGpuOk
            // 
            this.lblDashGpuOk.ForeColor = System.Drawing.Color.Gray;
            this.lblDashGpuOk.Location = new System.Drawing.Point(392, 52);
            this.lblDashGpuOk.Name = "lblDashGpuOk";
            this.lblDashGpuOk.Size = new System.Drawing.Size(28, 20);
            this.lblDashGpuOk.TabIndex = 1;
            // 
            // lblDashGpuTemp
            // 
            this.lblDashGpuTemp.Font = new System.Drawing.Font("Consolas", 11F, System.Drawing.FontStyle.Bold);
            this.lblDashGpuTemp.ForeColor = System.Drawing.Color.DarkRed;
            this.lblDashGpuTemp.Location = new System.Drawing.Point(286, 50);
            this.lblDashGpuTemp.Name = "lblDashGpuTemp";
            this.lblDashGpuTemp.Size = new System.Drawing.Size(110, 22);
            this.lblDashGpuTemp.TabIndex = 2;
            this.lblDashGpuTemp.Text = "--.- °C";
            // 
            // lblDashCpuOk
            // 
            this.lblDashCpuOk.ForeColor = System.Drawing.Color.Gray;
            this.lblDashCpuOk.Location = new System.Drawing.Point(172, 52);
            this.lblDashCpuOk.Name = "lblDashCpuOk";
            this.lblDashCpuOk.Size = new System.Drawing.Size(28, 20);
            this.lblDashCpuOk.TabIndex = 3;
            // 
            // btnSafetyReset
            // 
            this.btnSafetyReset.BackColor = System.Drawing.SystemColors.Control;
            this.btnSafetyReset.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnSafetyReset.Location = new System.Drawing.Point(279, 40);
            this.btnSafetyReset.Name = "btnSafetyReset";
            this.btnSafetyReset.Size = new System.Drawing.Size(95, 32);
            this.btnSafetyReset.TabIndex = 7;
            this.btnSafetyReset.Text = "安全重置";
            this.btnSafetyReset.UseVisualStyleBackColor = false;
            this.btnSafetyReset.Click += new System.EventHandler(this.BtnSafetyReset_Click);
            // 
            // lblDashCpuTemp
            // 
            this.lblDashCpuTemp.Font = new System.Drawing.Font("Consolas", 11F, System.Drawing.FontStyle.Bold);
            this.lblDashCpuTemp.ForeColor = System.Drawing.Color.DarkRed;
            this.lblDashCpuTemp.Location = new System.Drawing.Point(66, 50);
            this.lblDashCpuTemp.Name = "lblDashCpuTemp";
            this.lblDashCpuTemp.Size = new System.Drawing.Size(110, 22);
            this.lblDashCpuTemp.TabIndex = 4;
            this.lblDashCpuTemp.Text = "--.- °C";
            // 
            // lblDashFreq
            // 
            this.lblDashFreq.Font = new System.Drawing.Font("Consolas", 11F, System.Drawing.FontStyle.Bold);
            this.lblDashFreq.ForeColor = System.Drawing.Color.DarkGreen;
            this.lblDashFreq.Location = new System.Drawing.Point(382, 8);
            this.lblDashFreq.Name = "lblDashFreq";
            this.lblDashFreq.Size = new System.Drawing.Size(80, 22);
            this.lblDashFreq.TabIndex = 5;
            this.lblDashFreq.Text = "--";
            // 
            // lblDashFan
            // 
            this.lblDashFan.Font = new System.Drawing.Font("Consolas", 11F, System.Drawing.FontStyle.Bold);
            this.lblDashFan.ForeColor = System.Drawing.Color.DarkGreen;
            this.lblDashFan.Location = new System.Drawing.Point(222, 8);
            this.lblDashFan.Name = "lblDashFan";
            this.lblDashFan.Size = new System.Drawing.Size(70, 22);
            this.lblDashFan.TabIndex = 6;
            this.lblDashFan.Text = "--";
            // 
            // lblDashMode
            // 
            this.lblDashMode.Font = new System.Drawing.Font("Consolas", 11F, System.Drawing.FontStyle.Bold);
            this.lblDashMode.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblDashMode.Location = new System.Drawing.Point(64, 8);
            this.lblDashMode.Name = "lblDashMode";
            this.lblDashMode.Size = new System.Drawing.Size(80, 22);
            this.lblDashMode.TabIndex = 7;
            this.lblDashMode.Text = "--";
            // 
            // btnRemoteDutyDn
            // 
            this.btnRemoteDutyDn.Location = new System.Drawing.Point(258, 110);
            this.btnRemoteDutyDn.Name = "btnRemoteDutyDn";
            this.btnRemoteDutyDn.Size = new System.Drawing.Size(62, 30);
            this.btnRemoteDutyDn.TabIndex = 6;
            this.btnRemoteDutyDn.Text = "占空-";
            this.btnRemoteDutyDn.UseVisualStyleBackColor = true;
            this.btnRemoteDutyDn.Click += new System.EventHandler(this.BtnRemoteDutyDn_Click);
            // 
            // btnRemoteDutyUp
            // 
            this.btnRemoteDutyUp.Location = new System.Drawing.Point(196, 110);
            this.btnRemoteDutyUp.Name = "btnRemoteDutyUp";
            this.btnRemoteDutyUp.Size = new System.Drawing.Size(62, 30);
            this.btnRemoteDutyUp.TabIndex = 5;
            this.btnRemoteDutyUp.Text = "占空+";
            this.btnRemoteDutyUp.UseVisualStyleBackColor = true;
            this.btnRemoteDutyUp.Click += new System.EventHandler(this.BtnRemoteDutyUp_Click);
            // 
            // btnRemoteFreqDn
            // 
            this.btnRemoteFreqDn.Location = new System.Drawing.Point(134, 110);
            this.btnRemoteFreqDn.Name = "btnRemoteFreqDn";
            this.btnRemoteFreqDn.Size = new System.Drawing.Size(62, 30);
            this.btnRemoteFreqDn.TabIndex = 4;
            this.btnRemoteFreqDn.Text = "频率-";
            this.btnRemoteFreqDn.UseVisualStyleBackColor = true;
            this.btnRemoteFreqDn.Click += new System.EventHandler(this.BtnRemoteFreqDn_Click);
            // 
            // btnRemoteFreqUp
            // 
            this.btnRemoteFreqUp.Location = new System.Drawing.Point(72, 110);
            this.btnRemoteFreqUp.Name = "btnRemoteFreqUp";
            this.btnRemoteFreqUp.Size = new System.Drawing.Size(62, 30);
            this.btnRemoteFreqUp.TabIndex = 3;
            this.btnRemoteFreqUp.Text = "频率+";
            this.btnRemoteFreqUp.UseVisualStyleBackColor = true;
            this.btnRemoteFreqUp.Click += new System.EventHandler(this.BtnRemoteFreqUp_Click);
            // 
            // btnRemoteMode
            // 
            this.btnRemoteMode.Location = new System.Drawing.Point(10, 110);
            this.btnRemoteMode.Name = "btnRemoteMode";
            this.btnRemoteMode.Size = new System.Drawing.Size(62, 30);
            this.btnRemoteMode.TabIndex = 2;
            this.btnRemoteMode.Text = "模式";
            this.btnRemoteMode.UseVisualStyleBackColor = true;
            this.btnRemoteMode.Click += new System.EventHandler(this.BtnRemoteMode_Click);
            // 
            // txtStatusLog
            // 
            this.txtStatusLog.Location = new System.Drawing.Point(8, 46);
            this.txtStatusLog.Multiline = true;
            this.txtStatusLog.Name = "txtStatusLog";
            this.txtStatusLog.ReadOnly = true;
            this.txtStatusLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtStatusLog.Size = new System.Drawing.Size(580, 102);
            this.txtStatusLog.TabIndex = 7;
            this.txtStatusLog.Visible = false;
            // 
            // grpFanCurve
            // 
            this.grpFanCurve.Controls.Add(this.fanCurveGrid);
            this.grpFanCurve.Controls.Add(this.btnSendCurve);
            this.grpFanCurve.Controls.Add(this.btnReadCurve);
            this.grpFanCurve.Controls.Add(this.btnQueryStatus);
            this.grpFanCurve.Location = new System.Drawing.Point(10, 330);
            this.grpFanCurve.Name = "grpFanCurve";
            this.grpFanCurve.Size = new System.Drawing.Size(650, 240);
            this.grpFanCurve.TabIndex = 40;
            this.grpFanCurve.TabStop = false;
            this.grpFanCurve.Text = "风扇曲线配置";
            // 
            // fanCurveGrid
            // 
            this.fanCurveGrid.ColumnHeadersHeight = 34;
            this.fanCurveGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.fanCurveGrid.Location = new System.Drawing.Point(10, 22);
            this.fanCurveGrid.Name = "fanCurveGrid";
            this.fanCurveGrid.RowHeadersVisible = false;
            this.fanCurveGrid.RowHeadersWidth = 62;
            this.fanCurveGrid.Size = new System.Drawing.Size(240, 200);
            this.fanCurveGrid.TabIndex = 0;
            // 
            // btnSendCurve
            // 
            this.btnSendCurve.Location = new System.Drawing.Point(285, 20);
            this.btnSendCurve.Name = "btnSendCurve";
            this.btnSendCurve.Size = new System.Drawing.Size(105, 36);
            this.btnSendCurve.TabIndex = 1;
            this.btnSendCurve.Text = "发送曲线";
            this.btnSendCurve.UseVisualStyleBackColor = true;
            this.btnSendCurve.Click += new System.EventHandler(this.BtnSendCurve_Click);
            // 
            // btnReadCurve
            // 
            this.btnReadCurve.Location = new System.Drawing.Point(285, 66);
            this.btnReadCurve.Name = "btnReadCurve";
            this.btnReadCurve.Size = new System.Drawing.Size(105, 36);
            this.btnReadCurve.TabIndex = 2;
            this.btnReadCurve.Text = "读取曲线";
            this.btnReadCurve.UseVisualStyleBackColor = true;
            this.btnReadCurve.Click += new System.EventHandler(this.BtnReadCurve_Click);
            // 
            // btnQueryStatus
            // 
            this.btnQueryStatus.Location = new System.Drawing.Point(285, 112);
            this.btnQueryStatus.Name = "btnQueryStatus";
            this.btnQueryStatus.Size = new System.Drawing.Size(105, 36);
            this.btnQueryStatus.TabIndex = 3;
            this.btnQueryStatus.Text = "查询状态";
            this.btnQueryStatus.UseVisualStyleBackColor = true;
            this.btnQueryStatus.Click += new System.EventHandler(this.BtnQueryStatus_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(673, 612);
            this.Controls.Add(this.grpFanCurve);
            this.Controls.Add(this.grpEsp32);
            this.Controls.Add(this.checkBoxMinimizeToTray);
            this.Controls.Add(this.lblRefreshHint);
            this.Controls.Add(this.btnSafetyReset);
            this.Controls.Add(this.labelNoticeRefreshTimeAdjustmentWindow);
            this.Controls.Add(this.buttonConfirmRefreshTime);
            this.Controls.Add(this.domainUpDownSelectRefreshTime);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.buttonUseChosenMonitor);
            this.Controls.Add(this.labelNoticeGpuMonitor);
            this.Controls.Add(this.labelNoticeCpuMonitor);
            this.Controls.Add(this.comboBoxChooseGpuMonitor);
            this.Controls.Add(this.comboBoxChooseCpuMonitor);
            this.Controls.Add(this.checkBoxUseAida64Mode);
            this.Controls.Add(this.labelConnectionStatus);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.comboBoxSerialPorts);
            this.Controls.Add(this.gpuTempLabel);
            this.Controls.Add(this.cpuTempLabel);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "硬件温度监测";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.grpEsp32.ResumeLayout(false);
            this.grpEsp32.PerformLayout();
            this.dashboardPanel.ResumeLayout(false);
            this.grpFanCurve.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.fanCurveGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}