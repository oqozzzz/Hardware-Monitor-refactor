#include "task_pwm.h"
#include "system_state.h"
#include "config.h"

// ============================================================================
// PWM 输出任务
// 周期：50ms
// 职责：
//   1. 从全局状态读取 target_duty
//   2. 以斜坡限制逐步逼近目标值，避免风扇转速突变
//   3. 写入 LEDC 并回写 current_duty
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
        state_unlock();

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
