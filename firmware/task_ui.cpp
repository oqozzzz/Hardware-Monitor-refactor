#include "task_ui.h"
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>
#include "system_state.h"
#include "config.h"

// 由 firmware.ino 实例化的全局显示对象
extern Adafruit_SSD1306 display;

// ============================================================================
// 模式枚举转显示字符串
// ============================================================================
static const char* mode_to_string(OpMode mode)
{
    switch (mode) {
        case OpMode::QUIET:  return "Quiet";
        case OpMode::NORMAL: return "Normal";
        case OpMode::TURBO:  return "Turbo";
        case OpMode::MANUAL: return "Manual";
        default:             return "???";
    }
}

// ============================================================================
// 模式系数提示文字
// ============================================================================
static const char* mode_hint(OpMode mode)
{
    switch (mode) {
        case OpMode::QUIET:  return "50%";
        case OpMode::NORMAL: return "75%";
        case OpMode::TURBO:  return "100%";
        case OpMode::MANUAL: return "btn";
        default:             return "";
    }
}

// ============================================================================
// 绘制左上象限：运行模式 (0,0) - (62,30)
// ============================================================================
static void draw_mode_quadrant(OpMode mode)
{
    // 标签行：MODE + 系数提示
    display.setTextSize(1);
    display.setCursor(3, 3);
    display.print(F("MODE"));
    if (mode != OpMode::MANUAL) {
        display.print(F(" "));
        display.print(mode_hint(mode));
    }

    // 模式名称
    display.setCursor(4, 14);
    display.print(mode_to_string(mode));
}

// ============================================================================
// 绘制右上象限：风扇状态 (66,0) - (126,30)
// ============================================================================
static void draw_fan_quadrant(uint8_t duty, int freq)
{
    // 标签行
    display.setTextSize(1);
    display.setCursor(67, 3);
    display.print(F("FAN "));
    display.print(freq / 1000);
    display.print(F("k"));

    // 占空比
    uint8_t duty_pct = map(duty, 0, 255, 0, 100);
    display.setCursor(68, 14);
    display.print(duty_pct);
    display.print(F("%"));
}

// ============================================================================
// 绘制左下象限：CPU 温度 (0,34) - (62,62)
// ============================================================================
static void draw_cpu_quadrant(float temp, bool valid)
{
    // 标签
    display.setTextSize(1);
    display.setCursor(3, 35);
    display.print(F("CPU"));
    if (!valid) {
        display.print(F(" !"));
    }

    // 温度值
    display.setCursor(4, 46);
    if (valid) {
        display.print(temp, 1);
    } else {
        display.print(F("--.-"));
    }
    display.print(F("c"));
}

// ============================================================================
// 绘制右下象限：GPU 温度 (66,34) - (126,62)
// ============================================================================
static void draw_gpu_quadrant(float temp, bool valid)
{
    // 标签
    display.setTextSize(1);
    display.setCursor(67, 35);
    display.print(F("GPU"));
    if (!valid) {
        display.print(F(" !"));
    }

    // 温度值
    display.setCursor(68, 46);
    if (valid) {
        display.print(temp, 1);
    } else {
        display.print(F("--.-"));
    }
    display.print(F("c"));
}

// ============================================================================
// 绘制分隔线（留出 2px 间隙，避免文字贴边）
// ============================================================================
static void draw_divider(void)
{
    // 垂直中线: x = 64，间隙 y: 2-62
    display.drawFastVLine(64, 2, 60, SSD1306_WHITE);
    // 水平中线: y = 32，间隙 x: 2-126
    display.drawFastHLine(2, 32, 124, SSD1306_WHITE);
}

// ============================================================================
// UI 刷新任务
// 周期：100ms（10Hz）
// 布局（全部 1x 字号，白字黑底）：
//   +------------+------------+
//   | MODE 75%   | FAN 25k    |  y: 3
//   |  Normal    |  65%       |  y: 14
//   +------------+------------+
//   | CPU        | GPU        |  y: 35
//   |  55.2c     |  48.0c     |  y: 46
//   +------------+------------+
// ============================================================================
void task_ui(void *pvParameters)
{
    TickType_t last_wake = xTaskGetTickCount();

    // 开机首帧
    display.clearDisplay();
    display.setTextSize(1);
    display.setTextColor(SSD1306_WHITE);
    display.setCursor(24, 28);
    display.print(F("Fan Ctrl v2"));
    display.display();

    for (;;) {
        vTaskDelayUntil(&last_wake, pdMS_TO_TICKS(INTERVAL_UI_MS));

        state_lock();
        bool dirty = g_state.display_dirty;
        if (!dirty) {
            state_unlock();
            continue;
        }
        g_state.display_dirty = false;

        OpMode  mode    = g_state.mode;
        uint8_t duty    = g_state.current_duty;
        int     freq    = g_state.pwm_freq_hz;
        float   cpu     = g_state.cpu_temp;
        float   gpu     = g_state.gpu_temp;
        bool    cpu_ok  = g_state.cpu_valid;
        bool    gpu_ok  = g_state.gpu_valid;
        uint32_t now    = millis();

        // 数据超时检测
        if (now - g_state.last_data_ms > DATA_TIMEOUT_MS) {
            cpu_ok = false;
            gpu_ok = false;
        }

        g_state.heartbeat_ui = now;
        state_unlock();

        display.clearDisplay();

        // 绘制分隔线
        draw_divider();

        // 绘制四个象限
        draw_mode_quadrant(mode);
        draw_fan_quadrant(duty, freq);
        draw_cpu_quadrant(cpu, cpu_ok);
        draw_gpu_quadrant(gpu, gpu_ok);

        display.display();
    }
}
