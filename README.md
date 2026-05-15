# 硬件温度监控器

PC 端 CPU / GPU 温度监控 + ESP32 风扇自动控制器。基于 B 站用户[垃圾研究社](https://space.bilibili.com/376404862) 开源的 [DIY 压风式散热器](https://www.bilibili.com/video/BV1Lr421M7u2) 方案重构而来。

---

## 系统架构

```
┌─────────────────────────┐    蓝牙 SPP            ┌────────────────────────┐
│  PC (C# WinForms .NET 4.8)  │ ◄──── 双向 ────► │  ESP32 (FreeRTOS v3.0) │
│                         │     $TYPE,PAYLOAD*XX  │                        │
│  LibreHardwareMonitor   │                       │  6 个 FreeRTOS 任务     │
│  / AIDA64 注册表        │                       │  PWM 风扇控制 (25kHz)  │
│  串口 (115200 baud)      │                       │  SSD1306 OLED (128×64) │
│  风扇曲线在线配置        │                       │  5 个物理按钮           │
└─────────────────────────┘                       └────────────────────────┘
```

---

## 通信协议

所有帧采用 `$TYPE,PAYLOAD*XX\n` 格式，`XX` 为 XOR 校验和（两字节十六进制大写）。

### PC → ESP32

| 帧类型 | 格式 | 说明 |
|--------|------|------|
| TEMP_CPU | `$CPU,65.4*XX\n` | CPU 温度上报 |
| TEMP_GPU | `$GPU,72.1*XX\n` | GPU 温度上报 |
| STATUS_QUERY | `$STA,?*XX\n` | 查询 ESP32 当前状态 |
| FCURVE_SET | `$FCV,N,t1,d1,...,tN,dN*XX\n` | 上传风扇曲线 (N∈[2,10]) |
| FCURVE_QUERY | `$FCQ,?*XX\n` | 查询当前风扇曲线 |

### ESP32 → PC

| 帧类型 | 格式 | 说明 |
|--------|------|------|
| STATUS_RSP | `$STP,M,D%,F,CT,GT,CV,GV*XX\n` | 状态响应：模式/占空比/频率/温度/有效标志 |
| FCURVE_RSP | `$FCP,N,t1,d1,...,tN,dN*XX\n` | 当前风扇曲线 |
| ACK | `$ACK*XX\n` | 操作成功 |
| NACK | `$NAK,CC*XX\n` | 错误：01=帧错误 02=点数超限 03=曲线无效 |

ESP32 每 2 秒自动发送 STATUS_RSP 遥测帧。

---

## PC 端程序

### 技术栈

- C# WinForms, .NET Framework 4.8
- [LibreHardwareMonitorLib 0.9.3](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor)
- AIDA64 注册表读取（可选）

### 代码结构

```
CPUwenduhuoqu/
├── MainForm.cs              # 业务逻辑（生命周期、定时器、串口通信、事件处理）
├── MainForm.Layout.cs       # UI 构建（控件创建、布局定位、仪表盘、数据更新）
├── MainForm.Designer.cs     # VS 设计器管理的基础控件
├── App.config               # 可持久化配置
├── Hardware/
│   ├── IHardwareMonitor.cs          # 硬件监控接口
│   ├── LibreHardwareMonitorService.cs  # LibreHardwareMonitor 实现
│   └── Aida64MonitorService.cs      # AIDA64 注册表实现
├── Communication/
│   ├── Protocol.cs           # 协议帧编码/解码/校验和
│   ├── SerialPortService.cs  # 串口封装（线程安全、超时保护）
│   └── FanCurvePoint.cs      # 风扇曲线数据模型
└── Configuration/
    └── AppConfigService.cs   # 类型化配置读写
```

### UI 布局

```
┌─ 720×608 ──────────────────────────────────────────────┐
│ CPU 温度: XX.X °C                GPU 温度: XX.X °C      │
│ [COM端口▼] [连接] 状态     刷新间隔: [▼] [确认]          │
│ [☐ 使用AIDA64获取温度信息]        [☐ 最小化到托盘]       │
│ CPU传感器: [▼]      GPU传感器: [▼]     [确认]            │
│ ┌─ 固件状态 ────────────────────────────── [日志模式] ┐  │
│ │  模式: 正常      风扇: 65%       频率: 25kHz         │  │
│ │  ──────────────────────────────────────────         │  │
│ │  CPU: 55.2°C ✓                GPU: 48.0°C ✓        │  │
│ │  最后更新: 14:32:05                                 │  │
│ └────────────────────────────────────────────────────┘  │
│ ┌─ 风扇曲线配置 ──────────────────────────────────────┐  │
│ │  [温度(°C) │ 占空比(%)]     [发送曲线]              │  │
│ │  [可编辑表格 6行 ]          [读取曲线]              │  │
│ │                            [查询状态]              │  │
│ └────────────────────────────────────────────────────┘  │
│ [状态栏: 来源信息]                                      │
└────────────────────────────────────────────────────────┘
```

### 主要功能

- **温度监控**：LibreHardwareMonitor（默认）或 AIDA64 注册表两种数据源
- **串口通信**：蓝牙 SPP 虚拟 COM 端口，115200 baud，线程安全带超时保护
- **固件状态面板**：仪表盘/日志双模式，一键切换
  - 仪表盘模式：固定位置原地刷新，显示模式/风扇/频率/CPU/GPU/有效标志/更新时间
  - 日志模式：滚动文本，保留最近 200 行通信记录
- **风扇曲线在线配置**：可编辑表格，支持发送/读取 ESP32 风扇曲线
- **托盘常驻**：始终显示托盘图标，可选"最小化到托盘"（关闭按钮行为可配置）
- **配置持久化**：刷新间隔、串口名称、传感器选择、风扇曲线等写入 App.config

### 配置项 (App.config)

| 键 | 默认值 | 说明 |
|----|--------|------|
| RefreshInterval | 5000 | 温度刷新间隔 (ms) |
| SerialPortName | COM3 | 蓝牙串口名称 |
| BaudRate | 115200 | 串口波特率 |
| UseAida64Mode | false | 是否使用 AIDA64 |
| SelectedCpuSensor | Label.TCPU | 上次选择的 CPU 传感器 |
| SelectedGpuSensor | Label.TGPU1 | 上次选择的 GPU 传感器 |
| MinimizeToTray | true | 关闭按钮是否最小化到托盘 |
| LastFanCurve | (空) | 上次发送的风扇曲线 |

---

## ESP32 固件

### 技术栈

- ESP32 Dev Module + Arduino 框架
- FreeRTOS（Arduino 内核内置）
- BluetoothSerial（经典蓝牙 SPP）
- Adafruit SSD1306 + Adafruit GFX（OLED 显示）

### 代码结构

```
firmware/
├── firmware.ino           # 主入口，硬件初始化 + 7 任务创建
├── config.h               # 引脚/PWM/默认风扇曲线/任务栈大小/优先级/协议缓冲区
├── protocol.h/cpp         # 帧解析 (parse_frame) + 帧构建 (build_*)
├── fan_curve.h/cpp        # 运行时可变风扇曲线（线性插值查表、在线更新）
├── system_state.h/cpp     # 全局共享状态 + 互斥锁 + 快捷读写 API
├── task_bt_rx.h/cpp       # 蓝牙接收任务 (10ms, 帧类型路由)
├── task_bt_tx.h/cpp       # 蓝牙发送任务 (50ms, 遥测 + 查询应答)
├── task_control.h/cpp     # 温度控制任务 (100ms, 查表 + 模式系数)
├── task_pwm.h/cpp         # PWM 输出任务 (50ms, 斜坡逼近目标值)
├── task_ui.h/cpp          # OLED 显示任务 (100ms, 脏标记优化)
├── task_button.h/cpp      # 按钮输入任务 (20ms, 60ms 消抖)
└── safety.h/cpp           # 安全监控任务 (1s, 看门狗 + 心跳 + 故障强制全速)
```

### FreeRTOS 任务一览

| 任务 | 周期 | 栈 (B) | 优先级 | 职责 |
|------|------|--------|--------|------|
| bt_rx | 10ms | 4096 | 3 | 轮询蓝牙缓冲区，提取完整帧，校验和验证，按帧类型路由到状态更新或 tx_queue |
| bt_tx | 50ms | 4096 | 2 | 处理 tx_queue 中的 ACK/NACK 待发送帧，响应 STATUS_QUERY/FCURVE_QUERY，每 2 秒自动遥测 |
| control | 100ms | 3072 | 3 | 从全局状态读温度，取 CPU/GPU 最大值，查运行时风扇曲线，应用模式系数，写入 target_duty |
| pwm | 50ms | 2048 | 2 | 从 target_duty 斜坡逼近 current_duty（每周期最多 ±3 步），写入 LEDC 硬件 |
| ui | 100ms | 4096 | 1 | OLED 四象限刷新，display_dirty 标记触发，避免无变化时重绘 |
| button | 20ms | 2048 | 3 | 5 按钮读取 + 连续 3 次一致确认 (60ms 消抖) + 按键事件分发 |
| safety | 1s | 2048 | 2 | ESP32 任务看门狗 (5s panic) + bt_rx/control/pwm 心跳监控 (3s 超时触发 100% 风扇) |

### 硬件连接

| ESP32 GPIO | 连接 |
|------------|------|
| 5 | PWM 风扇控制信号 (25kHz, 8-bit) |
| 12 | 按钮 0 — 模式切换 (静音→正常→Turbo→手动) |
| 13 | 按钮 1 — PWM 频率 +200Hz |
| 14 | 按钮 2 — PWM 频率 -200Hz |
| 15 | 按钮 3 — 占空比 +10% (仅手动模式) |
| 16 | 按钮 4 — 占空比 -10% (仅手动模式) |
| 21 (SDA) | SSD1306 OLED I2C 数据 |
| 22 (SCL) | SSD1306 OLED I2C 时钟 |

### 运行模式

| 模式 | 编号 | 系数 | 说明 |
|------|------|------|------|
| 静音 Quiet | 1 | 50% | 风扇曲线基准占空比 × 0.5 |
| 正常 Normal | 2 | 75% | 风扇曲线基准占空比 × 0.75 |
| Turbo | 3 | 100% | 风扇曲线基准占空比 × 1.0 |
| 手动 Manual | 4 | — | 按钮 3/4 直接调节占空比（±10%），控制任务不干预 |

### 默认风扇曲线

编译期默认值，运行时可通过 FCURVE_SET 协议在线覆盖（最多 10 个点）：

| 温度 (°C) | 占空比 (%) |
|-----------|------------|
| 0.0 | 20 |
| 35.0 | 20 |
| 50.0 | 40 |
| 65.0 | 70 |
| 80.0 | 90 |
| 100.0 | 100 |

控制任务使用**分段线性插值**计算任意中间温度对应的目标占空比。

### 安全机制（三层防护）

1. **ESP32 任务看门狗** (5s) — 系统死锁时自动复位
2. **任务心跳监控** (3s) — 三大核心任务 (bt_rx / control / pwm) 任意卡死，强制风扇 100%
3. **温度数据超时** (5s) — 蓝牙断开或 PC 停止发送数据，`effective_temp` 强制 = 100°C，风扇全速

---

## 构建与部署

### PC 端

1. Visual Studio 2022 打开 `CPU_Temperture_Monitor.sln`
2. NuGet 恢复包依赖
3. 目标框架 .NET Framework 4.8 / AnyCPU，编译

### ESP32 固件

1. Arduino IDE（打开 `firmware.ino`）或 PlatformIO
2. 库依赖：**Adafruit SSD1306** (^2.5.7)、**Adafruit GFX Library** (^1.11.9)
3. 开发板选 ESP32 Dev Module
4. 编译、烧录

### 连接步骤

1. ESP32 上电
2. Windows 蓝牙设置中搜索并配对 "ESP32_FanController"
3. PC 端程序 COM 端口下拉选择蓝牙虚拟串口（通常是 COM8）
4. 点击"连接"，等待状态显示"已连接"
5. 仪表盘开始显示 ESP32 遥测数据，OLED 显示 CPU/GPU 温度

### 串口调试

ESP32 的 USB 串口 (115200 baud) 输出运行日志，每 10 秒打印一次状态。启动时应看到：

```
============================================
  ESP32 Fan Controller - Production FW v3.0
============================================
[INIT] Bluetooth started
[INIT] PWM initialized @ 25kHz
[INIT] Buttons initialized
[INIT] OLED initialized
[INIT] TX queue created
[INIT] All tasks started successfully
[STATUS] Mode=2 Duty=65% Target=65% CPU=55.2 GPU=48.0 Max=55.2 CurvePts=6
```
## 致谢

本项目 v3.0 重构（通信协议升级、代码分层架构、UI 改版、文档编写）由 AI 辅助完成。

原始项目：[垃圾研究社](https://space.bilibili.com/376404862) | [Payton9000/Hardware-Monitor](https://github.com/Payton9000/Hardware-Monitor)