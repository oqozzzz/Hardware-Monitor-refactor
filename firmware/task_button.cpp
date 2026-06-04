#include "task_button.h"
#include "system_state.h"
#include "config.h"

// ============================================================================
// Static debounce state
// ============================================================================
static bool     btn_history[BTN_COUNT]   = {false};
static uint8_t  btn_counters[BTN_COUNT]  = {0};
static bool     btn_fired[BTN_COUNT]     = {false};  // prevents repeat-firing while held

// ============================================================================
// Button event handler
// ============================================================================
static void on_button_pressed(int idx)
{
    state_lock();
    OpMode  mode = g_state.mode;
    uint8_t duty = g_state.target_duty;
    int     freq = g_state.pwm_freq_hz;
    state_unlock();

    switch (idx) {
        // ------------------------------------------------------------------
        // Button 0: cycle run mode (1->2->3->4->1)
        // ------------------------------------------------------------------
        case 0: {
            uint8_t next = (static_cast<uint8_t>(mode) % 4) + 1;
            state_lock();
            g_state.mode = static_cast<OpMode>(next);
            // When switching to auto mode, reset target_duty so control task takes over
            if (g_state.mode != OpMode::MANUAL) {
                g_state.target_duty = 0;
            }
            g_state.display_dirty = true;
            state_unlock();
            break;
        }

        // ------------------------------------------------------------------
        // Button 1: increase PWM frequency (+200Hz)
        // ------------------------------------------------------------------
        case 1: {
            freq += 200;
            if (freq > PWM_FREQ_MAX) freq = PWM_FREQ_MAX;
            // Defer ledcSetup to task_pwm to avoid concurrent LEDC access
            state_lock();
            g_state.pending_freq_hz = freq;
            g_state.freq_change_pending = true;
            g_state.display_dirty = true;
            state_unlock();
            break;
        }

        // ------------------------------------------------------------------
        // Button 2: decrease PWM frequency (-200Hz)
        // ------------------------------------------------------------------
        case 2: {
            freq -= 200;
            if (freq < PWM_FREQ_MIN) freq = PWM_FREQ_MIN;
            // Defer ledcSetup to task_pwm to avoid concurrent LEDC access
            state_lock();
            g_state.pending_freq_hz = freq;
            g_state.freq_change_pending = true;
            g_state.display_dirty = true;
            state_unlock();
            break;
        }

        // ------------------------------------------------------------------
        // Button 3: increase duty in manual mode (+10%)
        // ------------------------------------------------------------------
        case 3: {
            if (mode == OpMode::MANUAL) {
                int new_duty = duty + static_cast<int>(map(10, 0, 100, 0, 255));
                if (new_duty > PWM_MAX_DUTY) new_duty = PWM_MAX_DUTY;
                state_lock();
                g_state.target_duty = static_cast<uint8_t>(new_duty);
                g_state.display_dirty = true;
                state_unlock();
            }
            break;
        }

        // ------------------------------------------------------------------
        // Button 4: decrease duty in manual mode (-10%)
        // ------------------------------------------------------------------
        case 4: {
            if (mode == OpMode::MANUAL) {
                int new_duty = duty - static_cast<int>(map(10, 0, 100, 0, 255));
                if (new_duty < 0) new_duty = 0;
                state_lock();
                g_state.target_duty = static_cast<uint8_t>(new_duty);
                g_state.display_dirty = true;
                state_unlock();
            }
            break;
        }
    }
}

// ============================================================================
// Button sampling task
// Period: 20ms
// Debounce: 3 consecutive consistent samples to confirm state (60ms window)
// Fires once on confirmed press edge only
// ============================================================================
void task_button(void *pvParameters)
{
    TickType_t last_wake = xTaskGetTickCount();

    for (;;) {
        vTaskDelayUntil(&last_wake, pdMS_TO_TICKS(INTERVAL_BUTTON_MS));

        for (int i = 0; i < BTN_COUNT; i++) {
            bool raw = (digitalRead(BUTTON_PINS[i]) == HIGH);

            if (raw == btn_history[i]) {
                if (btn_counters[i] < BTN_DEBOUNCE_COUNT) {
                    btn_counters[i]++;
                }
            } else {
                btn_counters[i] = 0;
                btn_history[i]  = raw;
                btn_fired[i]    = false;  // reset on state change (release)
            }

            // Stable pressed state and just reached debounce threshold: fire once
            if (btn_counters[i] >= BTN_DEBOUNCE_COUNT && raw && !btn_fired[i]) {
                btn_fired[i] = true;
                on_button_pressed(i);
            }
        }

        state_lock();
        g_state.heartbeat_button = millis();
        state_unlock();
    }
}
