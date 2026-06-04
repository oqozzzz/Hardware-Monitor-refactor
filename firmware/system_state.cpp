#include "system_state.h"
#include <string.h>

// ============================================================================
//     
// ============================================================================
SystemState       g_state;
SemaphoreHandle_t g_state_mutex = nullptr;

// ============================================================================
//    
// ============================================================================
bool state_init(void)
{
    memset(&g_state, 0, sizeof(g_state));

    g_state.mode           = OpMode::NORMAL;
    g_state.pwm_freq_hz    = PWM_FREQ_HZ;
    g_state.target_duty    = static_cast<uint8_t>(map(20, 0, 100, 0, 255)); //    20%
    g_state.current_duty   = 0;
    g_state.freq_change_pending = false;
    g_state.pending_freq_hz    = PWM_FREQ_HZ;
    g_state.display_dirty       = true;

    g_state.cpu_valid      = false;
    g_state.gpu_valid      = false;


    g_state.status_query_pending = false;
    g_state.fcurve_query_pending = false;

    //               
    g_state.fan_curve.reset_to_default();

    // tx_queue   firmware.ino         
    g_state.tx_queue = nullptr;

    g_state_mutex = xSemaphoreCreateMutex();
    return (g_state_mutex != nullptr);
}

// ============================================================================
//      
// ============================================================================
void state_lock(void)
{
    if (g_state_mutex) {
        xSemaphoreTake(g_state_mutex, portMAX_DELAY);
    }
}

void state_unlock(void)
{
    if (g_state_mutex) {
        xSemaphoreGive(g_state_mutex);
    }
}

// ============================================================================
//          
// ============================================================================
void state_set_temp(bool is_cpu, float temp)
{
    state_lock();
    uint32_t now = millis();
    if (is_cpu) {
        g_state.cpu_temp    = temp;
        g_state.cpu_valid   = true;
        g_state.last_cpu_ms = now;
    } else {
        g_state.gpu_temp    = temp;
        g_state.gpu_valid   = true;
        g_state.last_gpu_ms = now;
    }

    g_state.display_dirty = true;
    state_unlock();
}

void state_set_target_duty(uint8_t duty)
{
    state_lock();
    g_state.target_duty = duty;
    state_unlock();
}

void state_set_current_duty(uint8_t duty)
{
    state_lock();
    g_state.current_duty = duty;
    state_unlock();
}

void state_set_mode(OpMode mode)
{
    state_lock();
    g_state.mode = mode;
    state_unlock();
}

void state_set_fan_curve(const FanCurvePoint *points, uint8_t count)
{
    state_lock();
    g_state.fan_curve.set_points(points, count);
    g_state.display_dirty = true;
    state_unlock();
}

void state_mark_dirty(void)
{
    state_lock();
    g_state.display_dirty = true;
    state_unlock();
}
