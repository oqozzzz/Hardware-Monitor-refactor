#ifndef SYSTEM_STATE_H
#define SYSTEM_STATE_H

#include <Arduino.h>
#include <freertos/FreeRTOS.h>
#include <freertos/semphr.h>
#include <freertos/queue.h>
#include "config.h"
#include "fan_curve.h"

// ============================================================================
// Global system state structure
// All fields accessed via g_state_mutex to prevent cross-task data races
// ============================================================================
struct SystemState {
    // ---- Temperature data ---------------------------------------------------
    float     cpu_temp;
    float     gpu_temp;
    float     max_temp;       // effective max temperature for control algorithm
    bool      cpu_valid;      // CPU data within validity window
    bool      gpu_valid;      // GPU data within validity window
    uint32_t  last_cpu_ms;    // timestamp of last CPU data
    uint32_t  last_gpu_ms;    // timestamp of last GPU data

    // ---- Control output -----------------------------------------------------
    OpMode    mode;
    uint8_t   target_duty;    // control-task computed target duty (0-255)
    uint8_t   current_duty;   // PWM-task actual output duty (0-255)
    int       pwm_freq_hz;    // current PWM frequency

    // ---- Pending frequency change (set by bt_rx/button, applied by pwm) ----
    bool      freq_change_pending;
    int       pending_freq_hz;

    // ---- Fan curve (mutable at runtime) -------------------------------------
    FanCurve  fan_curve;

    // ---- Bluetooth TX queue -------------------------------------------------
    QueueHandle_t tx_queue;   // other tasks push frame strings, bt_tx sends them

    // ---- Query pending flags (set by bt_rx, consumed by bt_tx) -------------
    bool      status_query_pending;
    bool      fcurve_query_pending;

    // ---- UI dirty flag ------------------------------------------------------
    bool      display_dirty;  // when true, UI task refreshes OLED on next cycle

    // ---- Safety state -------------------------------------------------------
    bool      safety_override; // set by safety task, forces PWM 100%
    uint32_t  fault_timestamp; // P0-6: timestamp when safety_override was asserted

    // ---- Task heartbeats (monitored by safety task, protected by mutex) ----
    uint32_t heartbeat_bt_rx;     // P3-6: volatile removed — mutex-protected, volatile redundant
    uint32_t heartbeat_bt_tx;
    uint32_t heartbeat_control;
    uint32_t heartbeat_pwm;
    uint32_t heartbeat_ui;
    uint32_t heartbeat_button;
};

// ============================================================================
// Global instance and mutex declaration
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
bool state_set_fan_curve(const FanCurvePoint *points, uint8_t count);  // returns false if validation fails
void state_mark_dirty(void);

#endif // SYSTEM_STATE_H
