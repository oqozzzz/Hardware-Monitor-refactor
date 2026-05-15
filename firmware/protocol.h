#ifndef PROTOCOL_H
#define PROTOCOL_H

#include <Arduino.h>
#include "fan_curve.h"

// ============================================================================
// 帧类型枚举
// ============================================================================
enum class FrameType : uint8_t {
    TEMP_CPU,       // $CPU,65.4*XX
    TEMP_GPU,       // $GPU,72.1*XX
    STATUS_QUERY,   // $STA,?*XX
    FCURVE_SET,     // $FCV,N,t1,d1,...,tN,dN*XX
    FCURVE_QUERY,   // $FCQ,?*XX
    UNKNOWN
};

// ============================================================================
// 解析后的帧数据
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
            uint8_t      count;  // FCURVE_SET: 点数
            FanCurvePoint points[10];
        } fcurve;
    };
};

// ============================================================================
// 帧解析（仅支持新格式: $TYPE,PAYLOAD*XX）
// ============================================================================
bool parse_frame(const char *line, size_t len, ParsedFrame &out);

// ============================================================================
// 帧构建器（输出到调用者提供的缓冲区，返回写入字节数不含 '\0'）
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
