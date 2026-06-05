#include "task_bt_rx.h"
#include "protocol.h"
#include "system_state.h"
#include "config.h"
#include <BluetoothSerial.h>
#include <string.h>
#include <math.h>

extern BluetoothSerial SerialBT;

// ============================================================================
//       
//            '\n'             FrameType   
//   
//   -        128   rx_idx    size_t
//   - 100ms            
//   -    legacy     
//   -   /       tx_queue     
// ============================================================================
void task_bt_rx(void *pvParameters)
{
    static char    rx_buf[RX_BUF_SIZE];
    static size_t  rx_idx = 0;
    static uint32_t last_byte_ms = 0;
    ParsedFrame frame;

    for (;;) {
        //        100ms     
        uint32_t now = millis();
        if (rx_idx > 0 && now - last_byte_ms > 100) {
            rx_idx = 0;
        }

        //         
        while (SerialBT.available()) {
            char c = SerialBT.read();
            last_byte_ms = now;

            if (c == '\n' || c == '\r') {
                if (rx_idx > 0) {
                    rx_buf[rx_idx] = '\0';

                    if (parse_frame(rx_buf, rx_idx, frame)) {
                        switch (frame.type) {
                            case FrameType::TEMP_CPU:
                                state_set_temp(true, frame.temp.temperature);
                                break;

                            case FrameType::TEMP_GPU:
                                state_set_temp(false, frame.temp.temperature);
                                break;

                            case FrameType::STATUS_QUERY:
                                state_lock();
                                g_state.status_query_pending = true;
                                state_unlock();
                                break;

                            case FrameType::FCURVE_QUERY:
                                state_lock();
                                g_state.fcurve_query_pending = true;
                                state_unlock();
                                break;

                            case FrameType::FCURVE_SET: {
                                bool ok = true;
                                //       
                                for (uint8_t i = 0; i < frame.fcurve.count; i++) {
                                    if (frame.fcurve.points[i].duty_percent < MIN_SAFE_DUTY_PERCENT) ok = false;  // P0-5: enforce minimum safe duty
                                    if (frame.fcurve.points[i].duty_percent > 100) ok = false;
                                    if (i > 0 && frame.fcurve.points[i].temperature <=
                                        frame.fcurve.points[i - 1].temperature) ok = false;
                                }
                                if (fabsf(frame.fcurve.points[0].temperature) > 0.01f) ok = false;

                                if (ok && g_state.tx_queue) {
                                    state_set_fan_curve(frame.fcurve.points, frame.fcurve.count);
                                    //    ACK
                                    char ack_buf[TX_BUF_SIZE];
                                    size_t ack_len = build_ack(ack_buf, TX_BUF_SIZE);
                                    if (ack_len > 0) {
                                        ack_buf[ack_len] = '\0';
                                        xQueueSend(g_state.tx_queue, ack_buf, 0);
                                    }
                                } else if (g_state.tx_queue) {
                                    //    NACK
                                    char nack_buf[TX_BUF_SIZE];
                                    uint8_t code = ok ? 2 : 3;
                                    size_t nack_len = build_nack(nack_buf, TX_BUF_SIZE, code);
                                    if (nack_len > 0) {
                                        nack_buf[nack_len] = '\0';
                                        xQueueSend(g_state.tx_queue, nack_buf, 0);
                                    }
                                }
                                break;
                            }

                            case FrameType::MODE_SET:
                                state_set_mode(static_cast<OpMode>(frame.ctrl.mode));
                                if (g_state.tx_queue) {
                                    char ack_buf[TX_BUF_SIZE];
                                    size_t ack_len = build_ack(ack_buf, TX_BUF_SIZE);
                                    if (ack_len > 0) { ack_buf[ack_len] = '\0'; xQueueSend(g_state.tx_queue, ack_buf, 0); }
                                }
                                break;

                            case FrameType::FREQ_SET:
                                // Defer ledcSetup to task_pwm to avoid concurrent LEDC access
                                state_lock();
                                g_state.pending_freq_hz = frame.ctrl.freq;
                                g_state.freq_change_pending = true;
                                state_unlock();
                                if (g_state.tx_queue) {
                                    char ack_buf[TX_BUF_SIZE];
                                    size_t ack_len = build_ack(ack_buf, TX_BUF_SIZE);
                                    if (ack_len > 0) { ack_buf[ack_len] = '\0'; xQueueSend(g_state.tx_queue, ack_buf, 0); }
                                }
                                break;

                            case FrameType::DUTY_SET: {
                                uint8_t duty_raw = static_cast<uint8_t>(map(frame.ctrl.duty, 0, 100, 0, 255));
                                state_set_target_duty(duty_raw);
                                if (g_state.tx_queue) {
                                    char ack_buf[TX_BUF_SIZE];
                                    size_t ack_len = build_ack(ack_buf, TX_BUF_SIZE);
                                    if (ack_len > 0) { ack_buf[ack_len] = '\0'; xQueueSend(g_state.tx_queue, ack_buf, 0); }
                                }
                                break;
                            }

                            case FrameType::UNKNOWN:
                                if (g_state.tx_queue) {
                                    char nack_buf[TX_BUF_SIZE];
                                    size_t nack_len = build_nack(nack_buf, TX_BUF_SIZE, 1);
                                    if (nack_len > 0) {
                                        nack_buf[nack_len] = '\0';
                                        xQueueSend(g_state.tx_queue, nack_buf, 0);
                                    }
                                }
                                break;

                            default:
                                break;
                        }
                    } else {
                        //               NACK
                        if (g_state.tx_queue) {
                            char nack_buf[TX_BUF_SIZE];
                            size_t nack_len = build_nack(nack_buf, TX_BUF_SIZE, 1);
                            if (nack_len > 0) {
                                nack_buf[nack_len] = '\0';
                                xQueueSend(g_state.tx_queue, nack_buf, 0);
                            }
                        }
                    }
                    rx_idx = 0;
                }
            } else if (rx_idx < RX_BUF_SIZE - 1) {
                rx_buf[rx_idx++] = c;
            } else {
                //          
                rx_idx = 0;
            }
        }

        //     
        state_lock();
        g_state.heartbeat_bt_rx = millis();
        state_unlock();

        vTaskDelay(pdMS_TO_TICKS(10));
    }
}
