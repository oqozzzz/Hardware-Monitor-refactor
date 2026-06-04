#include "task_pwm.h"
#include "system_state.h"
#include "config.h"

// ============================================================================
// PWM output task
// Period: 50ms
// Responsibilities:
//   1. Read target_duty from global state
//   2. Ramp toward target with rate limiting (PWM_RAMP_STEP per cycle)
//   3. Apply pending frequency changes from bt_rx/button tasks
//   4. Write LEDC output and update current_duty
// ============================================================================
void task_pwm(void *pvParameters)
{
    uint8_t current = 0;
    TickType_t last_wake = xTaskGetTickCount();

    for (;;) {
        vTaskDelayUntil(&last_wake, pdMS_TO_TICKS(PWM_PERIOD_MS));

        state_lock();
        bool override = g_state.safety_override;
        uint8_t target = g_state.target_duty;
        bool freq_pending = g_state.freq_change_pending;
        int new_freq = g_state.pending_freq_hz;
        if (freq_pending) {
            g_state.freq_change_pending = false;
            g_state.pwm_freq_hz = new_freq;
        }
        state_unlock();

        // Apply frequency change in this task to avoid concurrent LEDC access
        if (freq_pending) {
            ledcSetup(PWM_CHANNEL, new_freq, PWM_RES_BITS);
        }

        if (override) {
            current = PWM_MAX_DUTY;
        } else {
            int diff = static_cast<int>(target) - static_cast<int>(current);
            if (diff > PWM_RAMP_STEP) {
                current += PWM_RAMP_STEP;
            } else if (diff < -PWM_RAMP_STEP) {
                current -= PWM_RAMP_STEP;
            } else {
                current = target;
            }
            if (current > PWM_MAX_DUTY) current = PWM_MAX_DUTY;
        }

        ledcWrite(PWM_CHANNEL, current);

        state_lock();
        g_state.current_duty = current;
        g_state.heartbeat_pwm = millis();
        state_unlock();
    }
}
