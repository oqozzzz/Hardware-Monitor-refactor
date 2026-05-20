#ifndef SYSTEM_STATE_H
#define SYSTEM_STATE_H

#include <Arduino.h>
#include <freertos/FreeRTOS.h>
#include <freertos/semphr.h>
#include <freertos/queue.h>
#include "config.h"
#include "fan_curve.h"

// ============================================================================
// 系统全局状态结构
// 所有字段通过 g_state_mutex 互斥访问，禁止直接跨任务裸读裸写
// ============================================================================
struct SystemState {
    // ---- 温度数据 ----------------------------------------------------------
    float     cpu_temp;
    float     gpu_temp;
    float     max_temp;       // 当前有效最大温度（用于控制算法）
    bool      cpu_valid;      // CPU 数据是否在有效期内
    bool      gpu_valid;      // GPU 数据是否在有效期内
    uint32_t  last_cpu_ms;    // 上次收到 CPU 数据的时间戳
    uint32_t  last_gpu_ms;    // 上次收到 GPU 数据的时间戳

    // ---- 控制输出 ----------------------------------------------------------
    OpMode    mode;
    uint8_t   target_duty;    // 控制任务计算出的目标占空比 (0-255)
    uint8_t   current_duty;   // PWM 任务实际输出占空比 (0-255)
    int       pwm_freq_hz;    // 当前 PWM 频率

    // ---- 风扇曲线（运行时可变）-----------------------------------------------
    FanCurve  fan_curve;

    // ---- 蓝牙发送队列 -------------------------------------------------------
    QueueHandle_t tx_queue;   // 其他任务 push 帧字符串到队列，bt_tx 任务负责发送

    // ---- 查询待处理标志（由 bt_rx 设置，bt_tx 处理）--------------------------
    bool      status_query_pending;
    bool      fcurve_query_pending;

    // ---- UI 脏标记 ---------------------------------------------------------
    bool      display_dirty;  // 为 true 时 UI 任务将在下次周期刷新 OLED

    // ---- 安全状态 -----------------------------------------------------------
    bool      safety_override; // 安全任务触发后置位，PWM 任务强制 100%，仅复位可清除

    // ---- 任务心跳（由安全任务监控）------------------------------------------
    volatile uint32_t heartbeat_bt_rx;
    volatile uint32_t heartbeat_bt_tx;
    volatile uint32_t heartbeat_control;
    volatile uint32_t heartbeat_pwm;
    volatile uint32_t heartbeat_ui;
    volatile uint32_t heartbeat_button;
};

// ============================================================================
// 全局实例与互斥量声明
// ============================================================================
extern SystemState        g_state;
extern SemaphoreHandle_t  g_state_mutex;

// ============================================================================
// API
// ============================================================================
bool state_init(void);
void state_lock(void);
void state_unlock(void);

void state_set_temp(bool is_cpu, float temp);
void state_set_target_duty(uint8_t duty);
void state_set_current_duty(uint8_t duty);
void state_set_mode(OpMode mode);
void state_set_fan_curve(const FanCurvePoint *points, uint8_t count);
void state_mark_dirty(void);

#endif // SYSTEM_STATE_H
