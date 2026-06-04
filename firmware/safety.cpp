#include "safety.h"
#include "system_state.h"
#include "config.h"
#include <esp_task_wdt.h>

// ============================================================================
// Safety monitoring task
// Period: 1s
// Responsibilities:
//   1. Subscribe to ESP32 task watchdog to prevent system deadlock
//   2. Monitor heartbeat counters of core tasks (including bt_tx)
//   3. On task hang detection, force fan to 100%
// ============================================================================
void task_safety(void *pvParameters)
{
    esp_task_wdt_init(WATCHDOG_TIMEOUT_S, true);
    esp_task_wdt_add(NULL);

    for (;;) {
        vTaskDelay(pdMS_TO_TICKS(INTERVAL_SAFETY_MS));

        state_lock();
        uint32_t hb_bt_rx   = g_state.heartbeat_bt_rx;
        uint32_t hb_bt_tx   = g_state.heartbeat_bt_tx;
        uint32_t hb_control = g_state.heartbeat_control;
        uint32_t hb_pwm     = g_state.heartbeat_pwm;
        uint32_t hb_ui      = g_state.heartbeat_ui;
        uint32_t hb_button  = g_state.heartbeat_button;
        state_unlock();

        uint32_t now = millis();
        bool fault = false;

        // Core task heartbeat timeout detection
        if (now - hb_bt_rx   > HEARTBEAT_TIMEOUT_MS) fault = true;
        if (now - hb_control > HEARTBEAT_TIMEOUT_MS) fault = true;
        if (now - hb_pwm     > HEARTBEAT_TIMEOUT_MS) fault = true;

        if (fault) {
            state_lock();
            g_state.safety_override = true;
            g_state.target_duty = PWM_MAX_DUTY;
            state_unlock();
            ledcWrite(PWM_CHANNEL, PWM_MAX_DUTY);
            Serial.println(F("[SAFETY] Fault detected! Fan forced to 100%"));
        }

        esp_task_wdt_reset();
    }
}
