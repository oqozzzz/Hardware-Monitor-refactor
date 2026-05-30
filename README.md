# 硬件温度监控器

PC 端 CPU / GPU 温度监控 + ESP32 风扇自动控制器。基于 B 站用户[垃圾研究社](https://space.bilibili.com/376404862) 开源的 [DIY 压风式散热器](https://www.bilibili.com/video/BV1Lr421M7u2) 方案重构而来。

![UI 截图](屏幕截图%202026-05-30%20221547.png)

---

## 功能概览

- 实时读取 CPU / GPU 温度（LibreHardwareMonitor / AIDA64 双数据源）
- 蓝牙 SPP 串口通信，远程控制 ESP32 风扇
- 仪表盘显示 ESP32 遥测（模式、风扇占空比、PWM 频率、温度）
- 远程切换运行模式（静音 / 正常 / Turbo / 手动）
- 远程调节 PWM 频率和占空比
- 在线上传 / 读取风扇曲线（最多 10 点，Catmull-Rom 样条插值）
- 最小化到系统托盘，后台运行
- 可持久化配置（串口、刷新间隔、AIDA64 模式、风扇曲线）

---

## 系统架构

```
┌─────────────────────────────┐    蓝牙 SPP            ┌────────────────────────────┐
│  PC (C# WinForms .NET 4.8)  │ ◄──── 双向 ────► │  ESP32 (FreeRTOS v3.0)     │
│                             │     $TYPE,PAYLOAD*XX  │                            │
│  LibreHardwareMonitor       │                       │  7 个 FreeRTOS 任务         │
│  / AIDA64 注册表            │                       │  PWM 风扇控制 (25kHz)       │
│  串口 (115200 baud)         │                       │  SSD1306 OLED (128×64)      │
│  远程控制 + 风扇曲线配置    │                       │  5 个物理按钮              │
└─────────────────────────────┘                       └────────────────────────────┘
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
| MODE_SET | `$MOD,<1-4>*XX\n` | 远程设置运行模式 (1=静音 2=正常 3=Turbo 4=手动) |
| FREQ_SET | `$FRQ,<hz>*XX\n` | 远程设置 PWM 频率 (1000~40000 Hz) |
| DUTY_SET | `$DUT,<0-100>*XX\n` | 远程设置目标占空比 (%) |

### ESP32 → PC

| 帧类型 | 格式 | 说明 |
|--------|------|------|
| STATUS_RSP | `$STP,M,D%,F,CT,GT,CV,GV*XX\n` | 状态响应：模式/占空比/频率/温度/有效标志 |
| FCURVE_RSP | `$FCP,N,t1,d1,...,tN,dN*XX\n` | 当前风扇曲线 |
| ACK | `$ACK*XX\n` | 操作成功 |
| NACK | `$NAK,CC*XX\n` | 错误：01=帧错误 02=队列满 03=曲线无效 |

ESP32 每 2 秒自动发送 STATUS_RSP 遥测帧。

---

## PC 端代码结构

```
CPUwenduhuoqu/
├── MainForm.cs              # 业务逻辑（生命周期、定时器、串口通信、远程控制、事件处理）
├── MainForm.Designer.cs     # VS 设计器管理的 UI 控件布局
├── Program.cs               # 程序入口
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

---

## ESP32 固件代码结构

```
firmware/
├── firmware.ino             # 主入口（引脚定义、任务创建、setup/loop）
├── config.h                 # 全局常量（风扇曲线、阈值、任务参数）
├── protocol.h/cpp           # 帧编解码、校验和、命令路由
├── system_state.h/cpp       # 全局共享状态 + Mutex
├── fan_curve.h/cpp          # Catmull-Rom 样条插值 + Gamma 校正
├── safety.h/cpp             # 任务看门狗 + 心跳监控 + 故障强制全速
├── task_bt_rx.h/cpp         # 蓝牙接收任务 (10ms, 轮询 + 帧路由)
├── task_bt_tx.h/cpp         # 蓝牙发送任务 (50ms, ACK/NACK/遥测)
├── task_control.h/cpp       # 温度→占空比控制任务 (100ms)
├── task_pwm.h/cpp           # PWM 输出任务 (50ms, 斜坡逼近)
├── task_ui.h/cpp            # OLED 显示任务 (100ms, 脏标记优化)
└── task_button.h/cpp        # 按钮输入任务 (20ms, 60ms 消抖)
```

### FreeRTOS 任务一览

| 任务 | 周期 | 栈 (B) | 优先级 | 职责 |
|------|------|--------|--------|------|
| bt_rx | 10ms | 4096 | 3 | 轮询蓝牙缓冲区，提取完整帧，校验和验证，按帧类型路由 |
| bt_tx | 50ms | 4096 | 2 | 处理 tx_queue 中的 ACK/NACK 待发送帧，每 2 秒自动遥测 |
| control | 100ms | 3072 | 3 | 读温度取最大值，Catmull-Rom 样条查表，Gamma 校正模式映射 |
| pwm | 50ms | 2048 | 2 | 斜坡逼近 target_duty（每周期 ±3 步） |
| ui | 100ms | 4096 | 1 | OLED 四象限刷新，display_dirty + 各路独立超时判定触发 |
| button | 20ms | 2048 | 3 | 5 按钮读取 + 60ms 消抖 + 按键事件分发 |
| safety | 1s | 2048 | 2 | 任务看门狗 (5s panic) + 心跳监控 (3s) + 安全覆盖 |

### 硬件连接

| ESP32 GPIO | 连接 |
|------------|------|
| 5 | PWM 风扇控制信号 (25kHz, 8-bit) |
| 12 | 按钮 0 — 模式切换 |
| 13 | 按钮 1 — PWM 频率 +200Hz |
| 14 | 按钮 2 — PWM 频率 -200Hz |
| 15 | 按钮 3 — 占空比 +10% (仅手动模式) |
| 16 | 按钮 4 — 占空比 -10% (仅手动模式) |
| 21 (SDA) | SSD1306 OLED I2C 数据 |
| 22 (SCL) | SSD1306 OLED I2C 时钟 |

### 运行模式

| 模式 | 编号 | Gamma γ | 效果 |
|------|------|---------|------|
| 静音 Quiet | 1 | 1.6 | 低负载极安静，高负载仍有散热 |
| 正常 Normal | 2 | 1.2 | 略偏静音，全程平滑线性加速 |
| Turbo | 3 | 0.85 | 提前提速响应，高温段精细控制 |
| 手动 Manual | 4 | — | 按钮或远程直接调节占空比（±10%） |

占空比通过幂函数 `duty = 255 × (pct/100)^γ` 计算（Gamma 校正）。

### 默认风扇曲线

| 温度 (°C) | 占空比 (%) |
|-----------|------------|
| 0.0 | 20 |
| 30.0 | 20 |
| 45.0 | 35 |
| 60.0 | 55 |
| 75.0 | 75 |
| 90.0 | 95 |
| 100.0 | 100 |

控制任务使用 **Catmull-Rom 样条插值**（C1 连续）计算中间温度对应的目标占空比。

### 安全机制（三层防护）

1. **ESP32 任务看门狗** (5s) — 系统死锁时自动复位
2. **任务心跳监控** (3s) — 核心任务卡死时设置 `safety_override`，PWM 强制保持 255
3. **温度数据超时** (5s) — 各路温度源独立判定，双路均失效时 `effective_temp = 100°C`

---

## 构建与部署

### PC 端

1. Visual Studio 2022 打开 `CPU_Temperture_Monitor.sln`
2. NuGet 恢复包依赖
3. 目标框架 .NET Framework 4.8 / AnyCPU，编译

### ESP32 固件

1. Arduino IDE 打开 `firmware/firmware.ino`
2. 库依赖：**Adafruit SSD1306** (^2.5.7)、**Adafruit GFX Library** (^1.11.9)
3. 开发板选 ESP32 Dev Module
4. 编译、烧录

### 连接步骤

1. ESP32 上电
2. Windows 蓝牙设置中搜索并配对 "ESP32_FanController"
3. PC 端程序 COM 端口下拉选择蓝牙虚拟串口（通常是 COM8）
4. 点击"连接"，等待状态显示"已连接"
5. 仪表盘开始显示 ESP32 遥测数据，OLED 显示 CPU/GPU 温度
6. 使用远程控制按钮或物理按钮调节模式/频率/占空比

---

## 致谢

原始项目：[垃圾研究社](https://space.bilibili.com/376404862) | [Payton9000/Hardware-Monitor](https://github.com/Payton9000/Hardware-Monitor)