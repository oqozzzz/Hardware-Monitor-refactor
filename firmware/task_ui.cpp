#include "task_ui.h"
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>
#include "system_state.h"
#include "config.h"

// Global display instance from firmware.ino
extern Adafruit_SSD1306 display;

// ============================================================================
// Mode name string for display
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
// Mode coefficient hint text
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
// Draw upper-left quadrant: run mode (0,0)-(62,30)
// ============================================================================
static void draw_mode_quadrant(OpMode mode)
{
    // Label row: MODE + coefficient hint
    display.setTextSize(1);
    display.setCursor(3, 3);
    display.print(F("MODE"));
    if (mode != OpMode::MANUAL) {
        display.print(F(" "));
        display.print(mode_hint(mode));
    }

    // Mode name
    display.setCursor(4, 14);
    display.print(mode_to_string(mode));
}

// ============================================================================
// Draw upper-right quadrant: fan status (66,0)-(126,30)
// ============================================================================
static void draw_fan_quadrant(uint8_t duty, int freq)
{
    // Label row
    display.setTextSize(1);
    display.setCursor(67, 3);
    display.print(F("FAN "));
    display.print(freq / 1000);
    display.print(F("k"));

    // Duty cycle
    uint8_t duty_pct = map(duty, 0, 255, 0, 100);
    display.setCursor(68, 14);
    display.print(duty_pct);
    display.print(F("%"));
}

// ============================================================================
// Draw lower-left quadrant: CPU temperature (0,34)-(62,62)
// ============================================================================
static void draw_cpu_quadrant(float temp, bool valid)
{
    // Label
    display.setTextSize(1);
    display.setCursor(3, 35);
    display.print(F("CPU"));
    if (!valid) {
        display.print(F(" !"));
    }

    // Temperature value
    display.setCursor(4, 46);
    if (valid) {
        display.print(temp, 1);
    } else {
        display.print(F("--.-"));
    }
    display.print(F("c"));
}

// ============================================================================
// Draw lower-right quadrant: GPU temperature (66,34)-(126,62)
// ============================================================================
static void draw_gpu_quadrant(float temp, bool valid)
{
    // Label
    display.setTextSize(1);
    display.setCursor(67, 35);
    display.print(F("GPU"));
    if (!valid) {
        display.print(F(" !"));
    }

    // Temperature value
    display.setCursor(68, 46);
    if (valid) {
        display.print(temp, 1);
    } else {
        display.print(F("--.-"));
    }
    display.print(F("c"));
}

// ============================================================================
// Draw divider lines (2px gap to avoid text clipping)
// ============================================================================
static void draw_divider(void)
{
    // Vertical center: x=64, y: 2-62
    display.drawFastVLine(64, 2, 60, SSD1306_WHITE);
    // Horizontal center: y=32, x: 2-126
    display.drawFastHLine(2, 32, 124, SSD1306_WHITE);
}

// ============================================================================
// UI refresh task
// Period: 100ms (10Hz)
// Layout (all 1x size, white-on-black):
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

    // Boot splash screen
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

        // Data timeout detection
        if (now - g_state.last_cpu_ms > DATA_TIMEOUT_MS) {
            cpu_ok = false;
        }
        if (now - g_state.last_gpu_ms > DATA_TIMEOUT_MS) {
            gpu_ok = false;
        }

        g_state.heartbeat_ui = now;
        state_unlock();

        display.clearDisplay();

        // Draw divider lines
        draw_divider();

        // Draw four quadrants
        draw_mode_quadrant(mode);
        draw_fan_quadrant(duty, freq);
        draw_cpu_quadrant(cpu, cpu_ok);
        draw_gpu_quadrant(gpu, gpu_ok);

        display.display();
    }
}
