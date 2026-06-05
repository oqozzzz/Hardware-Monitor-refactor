#ifndef PROTOCOL_H
#define PROTOCOL_H

#include <Arduino.h>
#include "fan_curve.h"

// ============================================================================
// Frame type enumeration
// ============================================================================
enum class FrameType : uint8_t {
    TEMP_CPU,       // $CPU,65.4*XX
    TEMP_GPU,       // $GPU,72.1*XX
    STATUS_QUERY,   // $STA,?*XX
    FCURVE_SET,     // $FCV,N,t1,d1,...,tN,dN*XX
    FCURVE_QUERY,   // $FCQ,?*XX
    MODE_SET,       // $MOD,<1-4>*XX
    FREQ_SET,       // $FRQ,<hz>*XX
    DUTY_SET,       // $DUT,<0-100>*XX
    SAFETY_RESET,   // $SAF*XX  P0-6: remote safety override reset
    UNKNOWN
};

// ============================================================================
// Parsed frame data
// ============================================================================
struct TempData {
    float temperature;
    bool  valid;
};

struct ParsedFrame {
    FrameType type;
    union {
        TempData temp;          // TEMP_CPU / TEMP_GPU
        struct {
            uint8_t      count;  // FCURVE_SET: point count
            FanCurvePoint points[10];
        } fcurve;
        struct {
            uint8_t mode;       // MODE_SET: 1=QUIET,2=NORMAL,3=TURBO,4=MANUAL
            int     freq;       // FREQ_SET: PWM frequency Hz
            uint8_t duty;       // DUTY_SET: duty cycle 0-100%
        } ctrl;
    };
};

// ============================================================================
// Frame parser (new format only: $TYPE,PAYLOAD*XX)
// ============================================================================
bool parse_frame(const char *line, size_t len, ParsedFrame &out);

// ============================================================================
// Frame builders (output to caller-provided buffer, return bytes written excluding '\0')
// ============================================================================
size_t build_status_response(char *buf, size_t buf_size,
                             int mode, uint8_t duty_pct, int freq_hz,
                             float cpu_temp, float gpu_temp,
                             bool cpu_valid, bool gpu_valid);

size_t build_fcurve_response(char *buf, size_t buf_size,
                             const FanCurvePoint *points, uint8_t count);

size_t build_ack(char *buf, size_t buf_size);
size_t build_nack(char *buf, size_t buf_size, uint8_t error_code);

#endif // PROTOCOL_H
