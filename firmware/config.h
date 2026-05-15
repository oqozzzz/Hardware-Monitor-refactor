#ifndef CONFIG_H
#define CONFIG_H

#include <Arduino.h>

// ============================================================================
// 硬件引脚定义
// ============================================================================
constexpr int PIN_PWM_FAN      = 5;
constexpr int PIN_BTN_MODE     = 12;
constexpr int PIN_BTN_FREQ_UP  = 13;
constexpr int PIN_BTN_FREQ_DN  = 14;
constexpr int PIN_BTN_DUTY_UP  = 15;
constexpr int PIN_BTN_DUTY_DN  = 16;

constexpr int BTN_COUNT = 5;
extern const int BUTTON_PINS[BTN_COUNT];

// ============================================================================
// OLED 显示屏参数
// ============================================================================
constexpr int SCREEN_WIDTH  = 128;
constexpr int SCREEN_HEIGHT = 64;
constexpr int OLED_RESET    = -1;

// ============================================================================
// PWM 参数（25kHz 是 PC 风扇标准 PWM 频率，静音且兼容性好）
// ============================================================================
constexpr int PWM_CHANNEL    = 0;
constexpr int PWM_FREQ_HZ    = 25000;
constexpr int PWM_RES_BITS   = 8;
constexpr int PWM_MAX_DUTY   = 255;
constexpr int PWM_FREQ_MIN   = 1000;
constexpr int PWM_FREQ_MAX   = 40000;

// ============================================================================
// 默认风扇曲线：温度 -> 目标占空比(%)
// 用作 FanCurve::reset_to_default() 的初始值，运行时可被 FCURVE_SET 协议覆盖
// ============================================================================
struct DefaultFanCurvePoint {
    float temp;
    uint8_t duty_percent;
};

static const DefaultFanCurvePoint DEFAULT_FAN_CURVE[] = {
    {0.0f,   20},
    {35.0f,  20},
    {50.0f,  40},
    {65.0f,  70},
    {80.0f,  90},
    {100.0f, 100},
};
constexpr int DEFAULT_FAN_CURVE_POINTS = sizeof(DEFAULT_FAN_CURVE) / sizeof(DEFAULT_FAN_CURVE[0]);

constexpr float TEMP_HYSTERESIS = 3.0f; // 滞回带，防止在阈值附近抖动

// ============================================================================
// PWM 斜坡限制：防止风扇转速突变产生噪声
// ============================================================================
constexpr int PWM_RAMP_STEP         = 3;    // 每周期最大变化量 (0-255)
constexpr uint32_t PWM_PERIOD_MS    = 50;   // PWM 任务周期

// ============================================================================
// 任务堆栈大小（字节）
// 注意：UI 任务需要较大堆栈，因为 Adafruit GFX 的 display() 较耗栈
// ============================================================================
constexpr uint32_t STACK_BT_RX    = 4096;
constexpr uint32_t STACK_BT_TX    = 4096;
constexpr uint32_t STACK_CONTROL  = 3072;
constexpr uint32_t STACK_PWM      = 2048;
constexpr uint32_t STACK_UI       = 4096;
constexpr uint32_t STACK_BUTTON   = 2048;
constexpr uint32_t STACK_SAFETY   = 2048;

// ============================================================================
// 任务优先级（数字越大优先级越高，范围 0-24）
// ============================================================================
constexpr UBaseType_t PRIO_BT_RX   = 3;
constexpr UBaseType_t PRIO_BT_TX   = 2;
constexpr UBaseType_t PRIO_CONTROL = 3;
constexpr UBaseType_t PRIO_PWM     = 2;
constexpr UBaseType_t PRIO_UI      = 1;
constexpr UBaseType_t PRIO_BUTTON  = 3;
constexpr UBaseType_t PRIO_SAFETY  = 2;

// ============================================================================
// 任务周期（毫秒）
// ============================================================================
constexpr uint32_t INTERVAL_CONTROL_MS = 100;
constexpr uint32_t INTERVAL_UI_MS      = 100;
constexpr uint32_t INTERVAL_BUTTON_MS  = 20;
constexpr uint32_t INTERVAL_SAFETY_MS  = 1000;

// ============================================================================
// 按钮消抖参数
// ============================================================================
constexpr uint8_t BTN_DEBOUNCE_COUNT = 3; // 3 * 20ms = 60ms 消抖时间

// ============================================================================
// 蓝牙协议缓冲区
// ============================================================================
constexpr size_t RX_BUF_SIZE = 128;
constexpr size_t TX_BUF_SIZE = 80;
constexpr size_t TX_QUEUE_SIZE = 8;

// ============================================================================
// 安全与看门狗
// ============================================================================
constexpr uint32_t WATCHDOG_TIMEOUT_S   = 5;
constexpr uint32_t HEARTBEAT_TIMEOUT_MS = 3000; // 任务心跳超时判定
constexpr uint32_t DATA_TIMEOUT_MS      = 5000; // 温度数据超时（蓝牙断开保护）

// ============================================================================
// 运行模式
// ============================================================================
enum class OpMode : uint8_t {
    QUIET  = 1, // 50% 曲线输出
    NORMAL = 2, // 75% 曲线输出
    TURBO  = 3, // 100% 曲线输出
    MANUAL = 4  // 按钮手动控制占空比
};

#endif // CONFIG_H
