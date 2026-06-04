#include "task_control.h"
#include "system_state.h"
#include "config.h"
#include "fan_curve.h"
#include <math.h>

// ============================================================================
// Calculate final target PWM (0-255) from temperature and run mode
// Uses runtime-mutable fan curve g_state.fan_curve
// ============================================================================
static uint8_t calculate_target_duty(float temp, OpMode mode, const FanCurve &curve)
{
    if (mode == OpMode::MANUAL) {
        return 0; // In manual mode, button task directly maintains target_duty
    }

    uint8_t base_percent = curve.lookup(temp);

    float gamma;
    switch (mode) {
        case OpMode::QUIET:  gamma = 1.6f;  break;
        case OpMode::NORMAL: gamma = 1.2f;  break;
        case OpMode::TURBO:  gamma = 0.85f; break;
        default:             gamma = 1.0f;  break;
    }

    float norm = static_cast<float>(base_percent) / 100.0f;
    float corrected = pow(norm, gamma);
    return static_cast<uint8_t>(corrected * 255.0f);
}

// ============================================================================
// Control task
// Period: 100ms
// Responsibilities:
//   1. Read temperature data, mark as expired if timed out
//   2. Take the max of valid temperatures as control input
//   3. Lookup runtime fan curve and apply mode coefficient to get target_duty
//   4. Only write target_duty in non-manual modes
// ============================================================================
void task_control(void *pvParameters)
{
    TickType_t last_wake = xTaskGetTickCount();

    for (;;) {
        vTaskDelayUntil(&last_wake, pdMS_TO_TICKS(INTERVAL_CONTROL_MS));

        state_lock();
        float cpu_temp = g_state.cpu_temp;
        float gpu_temp = g_state.gpu_temp;
        bool  cpu_ok   = g_state.cpu_valid;
        bool  gpu_ok   = g_state.gpu_valid;
        OpMode mode    = g_state.mode;
        uint32_t now   = millis();

        if (now - g_state.last_cpu_ms > DATA_TIMEOUT_MS) {
            cpu_ok = false;
        }
        if (now - g_state.last_gpu_ms > DATA_TIMEOUT_MS) {
            gpu_ok = false;
        }

        // Copy fan curve snapshot
        FanCurve curve = g_state.fan_curve;
        state_unlock();

        // Calculate effective max temperature
        float effective_temp = 0.0f;
        if (cpu_ok && gpu_ok) {
            effective_temp = (cpu_temp > gpu_temp) ? cpu_temp : gpu_temp;
        } else if (cpu_ok) {
            effective_temp = cpu_temp;
        } else if (gpu_ok) {
            effective_temp = gpu_temp;
        } else {
            effective_temp = 100.0f; // Both sources lost: safety mode full speed
        }

        state_lock();
        g_state.max_temp = effective_temp;
        state_unlock();

        if (mode != OpMode::MANUAL) {
            uint8_t target = calculate_target_duty(effective_temp, mode, curve);
            state_set_target_duty(target);
        }

        state_lock();
        g_state.heartbeat_control = millis();
        state_unlock();
    }
}
