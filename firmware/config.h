#ifndef CONFIG_H
#define CONFIG_H

#include <Arduino.h>

// ============================================================================
// Hardware pin definitions
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
// OLED display parameters
// ============================================================================
constexpr int SCREEN_WIDTH  = 128;
constexpr int SCREEN_HEIGHT = 64;
constexpr int OLED_RESET    = -1;

// ============================================================================
// PWM parameters (25kHz is the standard PC fan PWM frequency)
// ============================================================================
constexpr int PWM_CHANNEL    = 0;
constexpr int PWM_FREQ_HZ    = 25000;
constexpr int PWM_RES_BITS   = 8;
constexpr int PWM_MAX_DUTY   = 255;
constexpr int PWM_FREQ_MIN   = 1000;
constexpr int PWM_FREQ_MAX   = 40000;

// ============================================================================
// Default fan curve: temperature -> target duty (%)
// Used as initial values for FanCurve::reset_to_default(), overridable at
// runtime via FCURVE_SET protocol command
// ============================================================================
struct DefaultFanCurvePoint {
    float temp;
    uint8_t duty_percent;
};

static const DefaultFanCurvePoint DEFAULT_FAN_CURVE[] = {
    {0.0f,   20},
    {30.0f,  20},
    {45.0f,  35},
    {60.0f,  55},
    {75.0f,  75},
    {90.0f,  95},
    {100.0f, 100},
};
constexpr int DEFAULT_FAN_CURVE_POINTS = sizeof(DEFAULT_FAN_CURVE) / sizeof(DEFAULT_FAN_CURVE[0]);

constexpr float TEMP_HYSTERESIS = 3.0f; // Hysteresis to prevent oscillation near thresholds

// ============================================================================
// PWM ramp rate limit: prevents sudden speed changes that cause noise
// ============================================================================
constexpr int PWM_RAMP_STEP         = 3;    // Max change per cycle (0-255)
constexpr uint32_t PWM_PERIOD_MS    = 50;   // PWM task period

// ============================================================================
// Task stack sizes (bytes)
// Note: UI task needs larger stack because Adafruit GFX display() is stack-heavy
// ============================================================================
constexpr uint32_t STACK_BT_RX    = 4096;
constexpr uint32_t STACK_BT_TX    = 4096;
constexpr uint32_t STACK_CONTROL  = 3072;
constexpr uint32_t STACK_PWM      = 2048;
constexpr uint32_t STACK_UI       = 4096;
constexpr uint32_t STACK_BUTTON   = 2048;
constexpr uint32_t STACK_SAFETY   = 2048;

// ============================================================================
// Task priorities (higher number = higher priority, range 0-24)
// ============================================================================
constexpr UBaseType_t PRIO_BT_RX   = 3;
constexpr UBaseType_t PRIO_BT_TX   = 2;
constexpr UBaseType_t PRIO_CONTROL = 3;
constexpr UBaseType_t PRIO_PWM     = 2;
constexpr UBaseType_t PRIO_UI      = 1;
constexpr UBaseType_t PRIO_BUTTON  = 3;
constexpr UBaseType_t PRIO_SAFETY  = 2;

// ============================================================================
// Task periods (milliseconds)
// ============================================================================
constexpr uint32_t INTERVAL_CONTROL_MS = 100;
constexpr uint32_t INTERVAL_UI_MS      = 100;
constexpr uint32_t INTERVAL_BUTTON_MS  = 20;
constexpr uint32_t INTERVAL_SAFETY_MS  = 1000;

// ============================================================================
// Button debounce parameters
// ============================================================================
constexpr uint8_t BTN_DEBOUNCE_COUNT = 3; // 3 * 20ms = 60ms debounce window

// ============================================================================
// Bluetooth protocol buffer sizes
// ============================================================================
constexpr size_t RX_BUF_SIZE = 128;
constexpr size_t TX_BUF_SIZE = 80;
constexpr size_t TX_QUEUE_SIZE = 8;

// ============================================================================
// Bluetooth security
// ============================================================================
constexpr const char* BT_PIN_CODE = "1234";  // P0-3: Bluetooth pairing PIN code
#define FIRMWARE_VERSION "v3.0"  // P2-8: single source of truth (must be #define for F() macro)
constexpr uint8_t MAX_FRAMES_PER_SEC = 10;  // P2-2: rate-limit incoming frames

// ============================================================================
// Safety and watchdog
// ============================================================================
constexpr uint32_t WATCHDOG_TIMEOUT_S   = 5;
constexpr uint32_t HEARTBEAT_TIMEOUT_MS = 3000; // Task heartbeat timeout threshold
constexpr uint32_t DATA_TIMEOUT_MS      = 5000; // Temperature data timeout (BT disconnect guard)

// ============================================================================
// Safety limits
// ============================================================================
constexpr uint8_t MIN_SAFE_DUTY_PERCENT = 20;  // P0-4/5: Minimum safe fan duty (prevents stall)
constexpr uint32_t SAFETY_RECOVERY_DELAY_MS = 30000;  // P0-6: Auto-recover from safety override after 30s heartbeat

// ============================================================================
// Run modes
// ============================================================================
enum class OpMode : uint8_t {
    QUIET  = 1, // gamma=1.6, quieter at low load
    NORMAL = 2, // gamma=1.2, balanced
    TURBO  = 3, // gamma=0.85, early ramp-up
    MANUAL = 4  // direct button/remote duty control
};

#endif // CONFIG_H
