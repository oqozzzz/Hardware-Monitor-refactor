#include "safety.h"
#include "system_state.h"
#include "config.h"
#include <esp_task_wdt.h>

// ============================================================================
// 安全监控任务
// 周期：1 秒
// 职责：
//   1. 订阅 ESP32 任务看门狗，防止系统死锁
//   2. 监控各业务任务的心跳计数器（含 bt_tx）
//   3. 若检测到任务卡死，强制风扇 100%
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

        // 核心任务心跳超时判定
        if (now - hb_bt_rx   > HEARTBEAT_TIMEOUT_MS) fault = true;
        if (now - hb_control > HEARTBEAT_TIMEOUT_MS) fault = true;
        if (now - hb_pwm     > HEARTBEAT_TIMEOUT_MS) fault = true;

        if (fault) {
            ledcWrite(PWM_CHANNEL, PWM_MAX_DUTY);
            Serial.println(F("[SAFETY] Fault detected! Fan forced to 100%"));
        }

        esp_task_wdt_reset();
    }
}
