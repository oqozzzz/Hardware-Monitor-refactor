#include "task_button.h"
#include "system_state.h"
#include "config.h"

// ============================================================================
// 静态消抖状态
// ============================================================================
static bool     btn_history[BTN_COUNT]   = {false};
static uint8_t  btn_counters[BTN_COUNT]  = {0};

// ============================================================================
// 按钮事件处理
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
        // 按钮 0：切换运行模式 (1->2->3->4->1)
        // ------------------------------------------------------------------
        case 0: {
            uint8_t next = (static_cast<uint8_t>(mode) % 4) + 1;
            state_lock();
            g_state.mode = static_cast<OpMode>(next);
            // 切换到自动模式时，重置 target_duty 让控制任务接管
            if (g_state.mode != OpMode::MANUAL) {
                g_state.target_duty = 0; // 控制任务会在下个周期重新计算
            }
            g_state.display_dirty = true;
            state_unlock();
            break;
        }

        // ------------------------------------------------------------------
        // 按钮 1：增加 PWM 频率 (+200Hz)
        // ------------------------------------------------------------------
        case 1: {
            freq += 200;
            if (freq > PWM_FREQ_MAX) freq = PWM_FREQ_MAX;
            ledcSetup(PWM_CHANNEL, freq, PWM_RES_BITS);
            state_lock();
            g_state.pwm_freq_hz = freq;
            g_state.display_dirty = true;
            state_unlock();
            break;
        }

        // ------------------------------------------------------------------
        // 按钮 2：减少 PWM 频率 (-200Hz)
        // ------------------------------------------------------------------
        case 2: {
            freq -= 200;
            if (freq < PWM_FREQ_MIN) freq = PWM_FREQ_MIN;
            ledcSetup(PWM_CHANNEL, freq, PWM_RES_BITS);
            state_lock();
            g_state.pwm_freq_hz = freq;
            g_state.display_dirty = true;
            state_unlock();
            break;
        }

        // ------------------------------------------------------------------
        // 按钮 3：手动模式下增加占空比 (+10%)
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
        // 按钮 4：手动模式下减少占空比 (-10%)
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
// 按钮采样任务
// 周期：20ms
// 消抖策略：连续 3 次采样一致才确认状态（60ms 消抖窗口）
// 仅在确认按下沿时触发一次事件
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
            }

            // 稳定为按下状态且刚达到消抖阈值：触发一次短按
            if (btn_counters[i] == BTN_DEBOUNCE_COUNT && raw) {
                on_button_pressed(i);
            }
        }

        state_lock();
        g_state.heartbeat_button = millis();
        state_unlock();
    }
}
