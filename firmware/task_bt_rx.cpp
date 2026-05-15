#include "task_bt_rx.h"
#include "protocol.h"
#include "system_state.h"
#include "config.h"
#include <BluetoothSerial.h>
#include <string.h>

extern BluetoothSerial SerialBT;

// ============================================================================
// 蓝牙接收任务
// 职责：轮询串口缓冲区，以 '\n' 为界提取完整帧，解析后按 FrameType 路由
// 改进：
//   - 缓冲区扩大至 128 字节，rx_idx 使用 size_t
//   - 100ms 无新数据自动丢弃残缺帧
//   - 移除 legacy 格式支持
//   - 查询/设置类帧通过 tx_queue 触发应答
// ============================================================================
void task_bt_rx(void *pvParameters)
{
    static char    rx_buf[RX_BUF_SIZE];
    static size_t  rx_idx = 0;
    static uint32_t last_byte_ms = 0;
    ParsedFrame frame;

    for (;;) {
        // 超时丢弃残缺帧（100ms 无新数据）
        uint32_t now = millis();
        if (rx_idx > 0 && now - last_byte_ms > 100) {
            rx_idx = 0;
        }

        // 读取所有可用字节
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
                                // 验证曲线数据
                                for (uint8_t i = 0; i < frame.fcurve.count; i++) {
                                    if (frame.fcurve.points[i].duty_percent > 100) ok = false;
                                    if (i > 0 && frame.fcurve.points[i].temperature <=
                                        frame.fcurve.points[i - 1].temperature) ok = false;
                                }
                                if (frame.fcurve.points[0].temperature != 0.0f) ok = false;

                                if (ok && g_state.tx_queue) {
                                    state_set_fan_curve(frame.fcurve.points, frame.fcurve.count);
                                    // 发送 ACK
                                    char ack_buf[TX_BUF_SIZE];
                                    size_t ack_len = build_ack(ack_buf, TX_BUF_SIZE);
                                    if (ack_len > 0) {
                                        ack_buf[ack_len] = '\0';
                                        xQueueSend(g_state.tx_queue, ack_buf, 0);
                                    }
                                } else if (g_state.tx_queue) {
                                    // 发送 NACK
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
                        // 解析失败（校验和不匹配等），发送 NACK
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
                // 缓冲区溢出，丢弃整帧
                rx_idx = 0;
            }
        }

        // 更新心跳
        state_lock();
        g_state.heartbeat_bt_rx = millis();
        state_unlock();

        vTaskDelay(pdMS_TO_TICKS(10));
    }
}
