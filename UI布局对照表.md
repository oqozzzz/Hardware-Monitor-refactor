# UI 布局代码对照表

## 文件结构

| 文件 | 职责 |
|------|------|
| `MainForm.Designer.cs` | VS 设计器生成的基础控件定义 + `InitializeComponent()` |
| `MainForm.Layout.cs` | 程序化 UI 构建：重定位 Designer 控件、创建新控件、布局、仪表盘数据更新 |
| `MainForm.cs` | 业务逻辑：生命周期、定时器、串口通信、按钮事件处理 |

---

## 一、窗体总体 `BuildUi()` — (Layout.cs:39)

| 行号 | 代码 | 说明 |
|------|------|------|
| 41 | `ClientSize = (720, 608)` | 窗体客户区大小 |
| 42 | `Font = "Microsoft YaHei UI", 9F` | 全局字体 |
| 44 | `RepositionDesignerControls()` | 移动 Designer 控件到新位置 |
| 45 | `BuildEsp32Section()` | 构建"固件状态"区 |
| 46 | `BuildFanCurveSection()` | 构建"风扇曲线配置"区 |

---

## 二、Designer 控件重定位 `RepositionDesignerControls()` — (Layout.cs:51)

### Row 0 — 温度显示 (y=8)

| 行号 | 控件变量 | 目标位置 | 类型 | 显示内容 |
|------|----------|----------|------|----------|
| 54 | `cpuTempLabel` | (12, 10) | Label, AutoSize | "CPU 温度: XX.X °C" 或 "CPU 温度: 无数据" |
| 55 | `gpuTempLabel` | (380, 10) | Label, AutoSize | "GPU 温度: XX.X °C" 或 "GPU 温度: 无数据" |

### Row 1 — 串口连接 + 刷新间隔 (y=40)

| 行号 | 控件变量 | 目标位置/大小 | 类型 | 显示内容 |
|------|----------|---------------|------|----------|
| 58-59 | `comboBoxSerialPorts` | (12, 40) 120×26 | ComboBox | COM 端口选择下拉 |
| 61-62 | `buttonConnect` | (140, 40) 70×26 | Button | "连接" / "断开" |
| 64 | `labelConnectionStatus` | (218, 44) | Label, AutoSize | "已连接" / "已断开" |
| 66 | `labelNoticeRefreshTimeAdjustmentWindow` | `Visible = false` | Label | **隐藏**（原"选择刷新时间（3-30s）"） |
| 68-75 | `_lblRefreshHint` | (300, 44) AutoSize | Label, 程序化创建 | "刷新间隔:" |
| 77-78 | `domainUpDownSelectRefreshTime` | (385, 40) 56×26 | DomainUpDown | 刷新秒数选择（3-30） |
| 80-81 | `buttonConfirmRefreshTime` | (448, 40) 55×26 | Button | "确认" |

### Row 2a — AIDA64 勾选框 + 最小化到托盘 (y=76)

| 行号 | 控件变量 | 目标位置 | 类型 | 显示内容 |
|------|----------|----------|------|----------|
| 84 | `checkBoxUseAida64Mode` | (12, 76) | CheckBox, AutoSize | "使用AIDA64获取硬件温度信息" |
| 86-99 | `_checkBoxMinimizeToTray` | (420, 76) AutoSize | CheckBox, 程序化创建 | "最小化到托盘"，勾选状态绑定 `_config.MinimizeToTray` 并即时保存 |

### Row 2b — AIDA64 传感器选择 (y=104)

| 行号 | 控件变量 | 目标位置/大小 | 类型 | 显示内容 |
|------|----------|---------------|------|----------|
| 102-104 | `labelNoticeCpuMonitor` | (12, 108) AutoSize | Label | "CPU传感器:" |
| 106-107 | `comboBoxChooseCpuMonitor` | (98, 104) 155×26 | ComboBox | CPU 传感器下拉选择 |
| 109-111 | `labelNoticeGpuMonitor` | (262, 108) AutoSize | Label | "GPU传感器:" |
| 113-114 | `comboBoxChooseGpuMonitor` | (348, 104) 155×26 | ComboBox | GPU 传感器下拉选择 |
| 116-117 | `buttonUseChosenMonitor` | (512, 102) 55×28 | Button | "确认" |

### 底部状态栏

| 行号 | 控件变量 | 目标位置 | 类型 | 显示内容 |
|------|----------|----------|------|----------|
| 120 | `statusStrip` | (0, 580) | StatusStrip | 含两个 ToolStripStatusLabel（来源、传感器信息） |

---

## 三、固件状态区 `BuildEsp32Section()` — (Layout.cs:125)

外框 `_grpEsp32`：GroupBox，标题 "固件状态"，(10, 136) 700×155

```
┌─ 固件状态 ───────────────────────────────── [日志模式] ┐  ← _btnToggleView (594,14) 95×32
│                                                        │
│  模式: 正常       风扇: 65%        频率: 25kHz          │  ← Row A (y≈8 relative)
│  ─────────────────────────────────────────────────────  │  ← 分隔线 (y=36)
│  CPU: 55.2 °C  ✓              GPU: 48.0 °C  ✓         │  ← Row B (y≈50)
│  最后更新: 14:32:05                                    │  ← Row C (y≈82)
│                                                        │
│  (或：多行只读文本框，日志模式时显示)                      │  ← _txtStatusLog (8,46) 580×102, 隐藏
└────────────────────────────────────────────────────────┘
```

### 容器控件

| 行号 | 变量 | 类型 | 位置/大小 | 说明 |
|------|------|------|-----------|------|
| 127 | `_grpEsp32` | GroupBox | (10, 136) 700×155 | 固件状态区外框 |
| 144 | `_dashboardPanel` | Panel | (8, 16) 684×130 | 仪表盘面板，相对 GroupBox |
| 208-217 | `_txtStatusLog` | TextBox | (8, 46) 580×102 | 日志文本框，初始 `Visible=false` |

### 切换按钮

| 行号 | 变量 | 类型 | 位置 | 文字 | 事件 |
|------|------|------|------|------|------|
| 135-141 | `_btnToggleView` | Button | (594, 14) 95×32 | "日志模式" / "仪表盘" | `ToggleResponseView()` |

### 仪表盘数据行 A — 模式 / 风扇 / 频率 (y≈6-8)

| 行号 | 创建方式 | 变量 | 位置 | 字体 | 颜色 | 最小宽度 | 说明 |
|------|----------|------|------|------|------|----------|------|
| 155 | `DashLabel()` | — | (10, 8) | headFont 10pt Bold | 默认 | — | 静态标签 "模式:" |
| 156 | `DashValue()` | `_lblDashMode` | (60, 6) | dataFont Consolas 11pt Bold | DarkBlue | 80px | 动态值："静音"/"正常"/"Turbo"/"手动" |
| 158 | `DashLabel()` | — | (170, 8) | headFont | 默认 | — | 静态标签 "风扇:" |
| 159 | `DashValue()` | `_lblDashFan` | (222, 6) | dataFont | DarkGreen | 70px | 动态值："65%" |
| 161 | `DashLabel()` | — | (330, 8) | headFont | 默认 | — | 静态标签 "频率:" |
| 162 | `DashValue()` | `_lblDashFreq` | (382, 6) | dataFont | DarkGreen | 80px | 动态值："25kHz" |

### 分隔线 (y=36)

| 行号 | 创建方式 | 位置/大小 | 样式 |
|------|----------|-----------|------|
| 165-170 | `new Label` | (8, 36) 666×2 | `BorderStyle.Fixed3D` |

### 仪表盘数据行 B — CPU / GPU 温度 (y≈48-52)

| 行号 | 创建方式 | 变量 | 位置 | 字体 | 颜色 | 最小宽度 | 说明 |
|------|----------|------|------|------|------|----------|------|
| 173 | `DashLabel()` | — | (10, 50) | headFont | 默认 | — | 静态标签 "CPU:" |
| 174 | `DashValue()` | `_lblDashCpuTemp` | (58, 48) | dataFont | DarkRed | 110px | 动态值："55.2 °C" |
| 175-181 | `new Label` | `_lblDashCpuOk` | (172, 52) 28×20 | tinyFont 8pt | Gray | — | 有效标志："✓"绿 / "✗"红 |
| 184 | `DashLabel()` | — | (230, 50) | headFont | 默认 | — | 静态标签 "GPU:" |
| 185 | `DashValue()` | `_lblDashGpuTemp` | (278, 48) | dataFont | DarkRed | 110px | 动态值："48.0 °C" |
| 186-193 | `new Label` | `_lblDashGpuOk` | (392, 52) 28×20 | tinyFont | Gray | — | 有效标志："✓"绿 / "✗"红 |

### 仪表盘数据行 C — 最后更新时间 (y=82)

| 行号 | 创建方式 | 变量 | 位置 | 字体 | 说明 |
|------|----------|------|------|------|------|
| 196 | `DashLabel()` | — | (10, 82) | tinyFont 8pt | 静态标签 "最后更新:" |
| 197-205 | `new Label` | `_lblDashUpdate` | (76, 82) 120×18 | tinyFont, 灰色 | 动态值："14:32:05"，初始 "等待数据..." |

---

## 四、风扇曲线配置区 `BuildFanCurveSection()` — (Layout.cs:227)

外框 `_grpFanCurve`：GroupBox，标题 "风扇曲线配置"，(10, 299) 700×240

```
┌─ 风扇曲线配置 ─────────────────────────────────────────┐
│ ┌──────────────┐                                       │
│ │ 温度(°C)│占空比│  [发送曲线] 105×36  (285, 20)       │
│ │ 0.0    │ 20   │  [读取曲线] 105×36  (285, 66)       │
│ │ 35.0   │ 20   │  [查询状态] 105×36  (285, 112)      │
│ │ 50.0   │ 40   │                                      │
│ │ 65.0   │ 70   │                                      │
│ │ 80.0   │ 90   │                                      │
│ │ 100.0  │ 100  │                                      │
│ └──────────────┘                                       │
└────────────────────────────────────────────────────────┘
```

### 容器

| 行号 | 变量 | 类型 | 位置/大小 |
|------|------|------|-----------|
| 229 | `_grpFanCurve` | GroupBox | (10, 299) 700×240 |

### 风扇曲线表格

| 行号 | 变量 | 类型 | 位置/大小 | 属性 |
|------|------|------|-----------|------|
| 237-245 | `_fanCurveGrid` | DataGridView | (10, 20) 260×205 | 可增删行，无行头，自动列宽 |
| 246 | 列 "TempCol" | — | — | 表头 "温度 (°C)" |
| 247 | 列 "DutyCol" | — | — | 表头 "占空比 (%)" |
| 248-253 | 6行默认数据 | — | — | (0,20) (35,20) (50,40) (65,70) (80,90) (100,100) |

### 操作按钮

| 行号 | 变量 | 位置/大小 | 文字 | 点击事件 |
|------|------|-----------|------|----------|
| 255-262 | `_btnSendCurve` | (285, 20) 105×36 | "发送曲线" | `BtnSendCurve_Click` |
| 264-271 | `_btnReadCurve` | (285, 66) 105×36 | "读取曲线" | `BtnReadCurve_Click` |
| 273-280 | `_btnQueryStatus` | (285, 112) 105×36 | "查询状态" | `BtnQueryStatus_Click` |

---

## 五、数据更新方法

### `UpdateDashboard(StatusData s)` — (Layout.cs:328)

收到 ESP32 `STATUS_RSP` 帧时调用，更新仪表盘所有动态值：

| 行号 | 更新的控件 | 数据来源 | 格式 |
|------|-----------|----------|------|
| 341 | `_lblDashMode` | `s.Mode` | 1→"静音" 2→"正常" 3→"Turbo" 4→"手动" |
| 342 | `_lblDashFan` | `s.DutyPercent` | `"{DutyPercent}%"` |
| 343 | `_lblDashFreq` | `s.FreqHz` | `"{FreqHz/1000}kHz"` |
| 344 | `_lblDashCpuTemp` | `s.CpuTemp` | `"{CpuTemp:F1} °C"` |
| 345 | `_lblDashGpuTemp` | `s.GpuTemp` | `"{GpuTemp:F1} °C"` |
| 347-348 | `_lblDashCpuOk` | `s.CpuValid` | "✓"(绿) / "✗"(红) |
| 349-350 | `_lblDashGpuOk` | `s.GpuValid` | "✓"(绿) / "✗"(红) |
| 352 | `_lblDashUpdate` | `DateTime.Now` | "HH:mm:ss" |

### `AppendStatusLog(string text)` — (Layout.cs:355)

追加文本到 `_txtStatusLog`，超过 200 行时只保留最后 100 行。

### `ToggleResponseView()` — (Layout.cs:318)

切换 `_dashboardPanel` 和 `_txtStatusLog` 的 `Visible`，按钮文字在 "日志模式" / "仪表盘" 之间切换。

---

## 六、辅助工具方法

### `DashLabel(text, x, y, font)` — (Layout.cs:291)

| 参数 | 说明 |
|------|------|
| `text` | 标签文字 |
| `x, y` | 在 `_dashboardPanel` 内的坐标 |
| `font` | 字体 |

创建 AutoSize 的 Label 并添加到 `_dashboardPanel.Controls`。

### `DashValue(text, x, y, font, color, minWidth)` — (Layout.cs:302)

| 参数 | 说明 |
|------|------|
| `text` | 初始显示文字 |
| `x, y` | 在 `_dashboardPanel` 内的坐标 |
| `font` | 字体 |
| `color` | 前景色 |
| `minWidth` | 最小宽度 |

创建可动态更新的 Label（返回引用），添加到 `_dashboardPanel.Controls`。

---

## 七、完整布局纵览

```
y=8    cpuTempLabel (12,10)              gpuTempLabel (380,10)
y=40   [COM▼120] [连接70] 状态  [刷新间隔:] [3s▼56] [确认55]
y=76   [☐ 使用AIDA64获取硬件温度信息]      [☐ 最小化到托盘]
y=104  CPU传感器: [▼155]   GPU传感器: [▼155]   [确认55]
y=136  ┌ 固件状态 (700×155) ──────────────────────────────┐
       │  模式: --  风扇: --  频率: --          [日志模式] │
       │  ──────────────────────────────────────          │
       │  CPU: --.- °C         GPU: --.- °C               │
       │  最后更新: 等待数据...                             │
       └──────────────────────────────────────────────────┘
y=299  ┌ 风扇曲线配置 (700×240) ──────────────────────────┐
       │  [表格 260×205]    [发送曲线]                     │
       │                    [读取曲线]                     │
       │                    [查询状态]                     │
       └──────────────────────────────────────────────────┘
y=580  [StatusStrip 状态栏]
```
