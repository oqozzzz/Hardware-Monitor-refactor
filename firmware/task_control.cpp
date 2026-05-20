#include "task_control.h"
#include "system_state.h"
#include "config.h"
#include "fan_curve.h"
#include <math.h>

// ============================================================================
// 根据温度和运行模式计算最终目标 PWM (0-255)
// 使用运行时可变风扇曲线 g_state.fan_curve
// ============================================================================
static uint8_t calculate_target_duty(float temp, OpMode mode, const FanCurve &curve)
{
    if (mode == OpMode::MANUAL) {
        return 0; // 手动模式下由按钮任务直接维护 target_duty
    }

    uint8_t base_percent = curve.lookup(temp);

    float gamma;
    switch (mode) {
        case OpMode::QUIET:  gamma = 1.6f;  break;
        case OpMode::NORMAL: gamma = 1.2f;  break;
        case OpMode::TURBO:  gamma = 0.85f; break;
        default:             gamma = 1.0f;  break;
    }

    float norm = static_cast<float>(base_percent) / 100.0f;
    float corrected = pow(norm, gamma);
    return static_cast<uint8_t>(corrected * 255.0f);
}

// ============================================================================
// 控制任务
// 周期：100ms
// 职责：
//   1. 读取温度数据，超时则标记失效
//   2. 取有效温度的最大值作为控制输入
//   3. 从运行时风扇曲线查表并应用模式系数，得到 target_duty
//   4. 仅在非手动模式下写入 target_duty
// ============================================================================
void task_control(void *pvParameters)
{
    TickType_t last_wake = xTaskGetTickCount();

    for (;;) {
        vTaskDelayUntil(&last_wake, pdMS_TO_TICKS(INTERVAL_CONTROL_MS));

        state_lock();
        float cpu_temp = g_state.cpu_temp;
        float gpu_temp = g_state.gpu_temp;
        bool  cpu_ok   = g_state.cpu_valid;
        bool  gpu_ok   = g_state.gpu_valid;
        OpMode mode    = g_state.mode;
        uint32_t now   = millis();

        if (now - g_state.last_cpu_ms > DATA_TIMEOUT_MS) {
            cpu_ok = false;
        }
        if (now - g_state.last_gpu_ms > DATA_TIMEOUT_MS) {
            gpu_ok = false;
        }

        // 拷贝风扇曲线快照
        FanCurve curve = g_state.fan_curve;
        state_unlock();

        // 计算有效最大温度
        float effective_temp = 0.0f;
        if (cpu_ok && gpu_ok) {
            effective_temp = (cpu_temp > gpu_temp) ? cpu_temp : gpu_temp;
        } else if (cpu_ok) {
            effective_temp = cpu_temp;
        } else if (gpu_ok) {
            effective_temp = gpu_temp;
        } else {
            effective_temp = 100.0f; // 双路失效：安全模式全速
        }

        state_lock();
        g_state.max_temp = effective_temp;
        state_unlock();

        if (mode != OpMode::MANUAL) {
            uint8_t target = calculate_target_duty(effective_temp, mode, curve);
            state_set_target_duty(target);
        }

        state_lock();
        g_state.heartbeat_control = millis();
        state_unlock();
    }
}
