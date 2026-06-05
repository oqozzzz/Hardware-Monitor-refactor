#include "task_bt_rx.h"
#include "protocol.h"
#include "system_state.h"
#include "config.h"
#include <BluetoothSerial.h>
#include <string.h>
#include <math.h>

extern BluetoothSerial SerialBT;

// ============================================================================
// P1-10: Helper functions to eliminate repeated ACK/NACK inline code (6+ occurrences)
// ============================================================================
static void send_ack()
{
    if (!g_state.tx_queue) return;
    char buf[TX_BUF_SIZE];
    size_t len = build_ack(buf, TX_BUF_SIZE);
    if (len > 0) { buf[len] = '\0'; xQueueSend(g_state.tx_queue, buf, 0); }
}

static void send_nack(uint8_t code)
{
    if (!g_state.tx_queue) return;
    char buf[TX_BUF_SIZE];
    size_t len = build_nack(buf, TX_BUF_SIZE, code);
    if (len > 0) { buf[len] = '\0'; xQueueSend(g_state.tx_queue, buf, 0); }
}

// ============================================================================
// Bluetooth RX task: reads frames from Bluetooth Serial, parses and dispatches
// ============================================================================
void task_bt_rx(void *pvParameters)
{
    static char    rx_buf[RX_BUF_SIZE];
    static size_t  rx_idx = 0;
    static uint32_t last_byte_ms = 0;
    // P2-2: frame rate limiter
    static uint32_t frame_count = 0;
    static uint32_t frame_count_start_ms = 0;
    ParsedFrame frame;

    for (;;) {
        // Reset partial frame if idle >100ms
        uint32_t now = millis();
        if (rx_idx > 0 && now - last_byte_ms > 100) {
            rx_idx = 0;
        }

        // P2-2: reset frame rate counter every second
        if (now - frame_count_start_ms >= 1000) {
            frame_count = 0;
            frame_count_start_ms = now;
        }

        // Read available bytes
        while (SerialBT.available()) {
            char c = SerialBT.read();
            last_byte_ms = now;

            if (c == '\n' || c == '\r') {
                if (rx_idx > 0) {
                    rx_buf[rx_idx] = '\0';

                    // P2-2: silently drop frames exceeding rate limit
                    if (++frame_count > MAX_FRAMES_PER_SEC) {
                        rx_idx = 0;
                        continue;
                    }

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
                                // CR #7: validation dedup — FanCurve::set_points is the single source of truth
                                if (state_set_fan_curve(frame.fcurve.points, frame.fcurve.count)) {
                                    send_ack();  // P1-10
                                } else if (g_state.tx_queue) {
                                    send_nack(3);  // P1-10: validation failed
                                }
                                break;
                            }

                            case FrameType::MODE_SET:
                                state_set_mode(static_cast<OpMode>(frame.ctrl.mode));
                                send_ack();  // P1-10
                                break;

                            case FrameType::FREQ_SET:
                                // Defer ledcSetup to task_pwm to avoid concurrent LEDC access
                                state_lock();
                                g_state.pending_freq_hz = frame.ctrl.freq;
                                g_state.freq_change_pending = true;
                                state_unlock();
                                send_ack();  // P1-10
                                break;

                            case FrameType::DUTY_SET: {
                                uint8_t duty_raw = static_cast<uint8_t>(map(frame.ctrl.duty, 0, 100, 0, 255));
                                state_set_target_duty(duty_raw);
                                send_ack();  // P1-10
                                break;
                            }

                            case FrameType::SAFETY_RESET:  // P0-6: remote safety override reset
                                state_lock();
                                g_state.safety_override = false;
                                g_state.display_dirty = true;
                                state_unlock();
                                send_ack();  // P1-10
                                break;

                            case FrameType::UNKNOWN:
                                send_nack(1);  // P1-10
                                break;

                            default:
                                break;
                        }
                    } else {
                        send_nack(1);  // P1-10: parse failure NACK
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
