#include "task_bt_tx.h"
#include "protocol.h"
#include "system_state.h"
#include "config.h"
#include <BluetoothSerial.h>
#include <string.h>

extern BluetoothSerial SerialBT;

// ============================================================================
// Bluetooth transmit task
// Period: 50ms
// Responsibilities:
//   1. Dequeue pending frames from tx_queue and send via SerialBT
//   2. Handle status_query_pending / fcurve_query_pending flags
//   3. Send STATUS_RSP telemetry every 2 seconds
// ============================================================================
void task_bt_tx(void *pvParameters)
{
    TickType_t last_wake = xTaskGetTickCount();
    uint32_t   last_telemetry_ms = 0;

    // Shared TX buffer (static allocation to avoid repeated 80-byte stack allocation)
    static char tx_buf[TX_BUF_SIZE];

    for (;;) {
        vTaskDelayUntil(&last_wake, pdMS_TO_TICKS(50));

        if (g_state.tx_queue == nullptr) continue;

        uint32_t now = millis();

        // ---- Read state snapshot ----
        state_lock();
        bool status_pending  = g_state.status_query_pending;
        bool fcurve_pending  = g_state.fcurve_query_pending;
        OpMode mode          = g_state.mode;
        uint8_t duty_raw     = g_state.current_duty;
        int     freq         = g_state.pwm_freq_hz;
        float   cpu_temp     = g_state.cpu_temp;
        float   gpu_temp     = g_state.gpu_temp;
        bool    cpu_valid    = g_state.cpu_valid;
        bool    gpu_valid    = g_state.gpu_valid;

        if (now - g_state.last_cpu_ms > DATA_TIMEOUT_MS) {
            cpu_valid = false;
        }
        if (now - g_state.last_gpu_ms > DATA_TIMEOUT_MS) {
            gpu_valid = false;
        }
        state_unlock();

        uint8_t duty_pct = map(duty_raw, 0, 255, 0, 100);

        // ---- Handle STATUS_QUERY ----
        if (status_pending) {
            size_t len = build_status_response(tx_buf, TX_BUF_SIZE,
                static_cast<int>(mode), duty_pct, freq,
                cpu_temp, gpu_temp, cpu_valid, gpu_valid);
            if (len > 0) {
                tx_buf[len] = '\0';
                SerialBT.println(tx_buf);
            }
            state_lock();
            g_state.status_query_pending = false;
            state_unlock();
        }

        // ---- Handle FCURVE_QUERY ----
        if (fcurve_pending) {
            state_lock();
            uint8_t count = g_state.fan_curve.get_count();
            const FanCurvePoint *src = g_state.fan_curve.get_points();
            state_unlock();

            size_t len = build_fcurve_response(tx_buf, TX_BUF_SIZE, src, count);
            if (len > 0) {
                tx_buf[len] = '\0';
                SerialBT.println(tx_buf);
            }
            state_lock();
            g_state.fcurve_query_pending = false;
            state_unlock();
        }

        // ---- Drain pending TX queue (ACK/NACK) ----
        while (xQueueReceive(g_state.tx_queue, tx_buf, 0) == pdTRUE) {
            SerialBT.println(tx_buf);
        }

        // ---- Periodic telemetry (every 2s) ----
        if (now - last_telemetry_ms >= 2000) {
            size_t len = build_status_response(tx_buf, TX_BUF_SIZE,
                static_cast<int>(mode), duty_pct, freq,
                cpu_temp, gpu_temp, cpu_valid, gpu_valid);
            if (len > 0) {
                tx_buf[len] = '\0';
                SerialBT.println(tx_buf);
            }
            last_telemetry_ms = now;
        }

        // Update heartbeat
        state_lock();
        g_state.heartbeat_bt_tx = millis();
        state_unlock();
    }
}
