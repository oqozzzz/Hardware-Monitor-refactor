#include "protocol.h"
#include "config.h"
#include <string.h>
#include <stdlib.h>
#include <stdio.h>
#include <math.h>

// ============================================================================
// XOR checksum calculation
// ============================================================================
static uint8_t calc_checksum(const char *data, size_t len)
{
    uint8_t cs = 0;
    for (size_t i = 0; i < len; i++) {
        cs ^= static_cast<uint8_t>(data[i]);
    }
    return cs;
}

// ============================================================================
// Frame parser (format: $TYPE,PAYLOAD*XX)
// ============================================================================
bool parse_frame(const char *line, size_t len, ParsedFrame &out)
{
    out.type = FrameType::UNKNOWN;

    if (len < 6 || line[0] != '$') return false;

    // Find '*' checksum separator
    const char *star = static_cast<const char *>(memchr(line, '*', len));
    if (!star) return false;

    // Data section: between $ and *
    const char *data_start = line + 1;
    size_t data_len = star - data_start;
    if (data_len < 3) return false;

    // Verify checksum
    if (star + 3 > line + len) return false;
    char cs_buf[3] = {star[1], star[2], '\0'};
    uint8_t rx_cs = static_cast<uint8_t>(strtol(cs_buf, nullptr, 16));
    if (rx_cs != calc_checksum(data_start, data_len)) return false;

    // ---- TEMP_CPU / TEMP_GPU ----
    if (data_len >= 6 && (strncmp(data_start, "CPU,", 4) == 0 ||
                          strncmp(data_start, "GPU,", 4) == 0))
    {
        bool is_cpu = (data_start[0] == 'C');
        out.type = is_cpu ? FrameType::TEMP_CPU : FrameType::TEMP_GPU;
        float temp = strtof(data_start + 4, nullptr);
        // P1-3: reject NaN, Inf, and out-of-range temperatures
        if (isnan(temp) || isinf(temp)) return false;
        if (temp < -50.0f || temp > 150.0f) return false;
        out.temp.temperature = temp;
        out.temp.valid = true;
        return true;
    }

    // ---- STATUS_QUERY: $STA,?*XX ----
    if (data_len == 5 && strncmp(data_start, "STA,", 4) == 0) {
        out.type = FrameType::STATUS_QUERY;
        return true;
    }

    // ---- FCURVE_QUERY: $FCQ,?*XX ----
    if (data_len == 5 && strncmp(data_start, "FCQ,", 4) == 0) {
        out.type = FrameType::FCURVE_QUERY;
        return true;
    }

    // ---- FCURVE_SET ----
    if (data_len >= 6 && strncmp(data_start, "FCV,", 4) == 0) {
        const char *p = data_start + 4;

        // Parse point count N
        uint8_t count = static_cast<uint8_t>(strtol(p, const_cast<char **>(&p), 10));
        if (count < 2 || count > 10) return false;
        if (*p != ',') return false;

        out.fcurve.count = count;
        for (uint8_t i = 0; i < count; i++) {
            p++; // skip comma
            float t = strtof(p, const_cast<char **>(&p));
            if (*p != ',') return false;
            p++; // skip comma
            uint8_t d = static_cast<uint8_t>(strtol(p, const_cast<char **>(&p), 10));
            out.fcurve.points[i].temperature = t;
            out.fcurve.points[i].duty_percent = d;
        }

        out.type = FrameType::FCURVE_SET;
        return true;
    }

    // ---- MODE_SET: $MOD,<1-4>*XX ----
    if (data_len >= 5 && strncmp(data_start, "MOD,", 4) == 0) {
        long mode = strtol(data_start + 4, nullptr, 10);
        if (mode < 1 || mode > 4) return false;
        out.ctrl.mode = static_cast<uint8_t>(mode);
        out.type = FrameType::MODE_SET;
        return true;
    }

    // ---- FREQ_SET: $FRQ,<hz>*XX ----
    if (data_len >= 6 && strncmp(data_start, "FRQ,", 4) == 0) {
        long freq = strtol(data_start + 4, nullptr, 10);
        if (freq < PWM_FREQ_MIN || freq > PWM_FREQ_MAX) return false;
        out.ctrl.freq = static_cast<int>(freq);
        out.type = FrameType::FREQ_SET;
        return true;
    }

    // ---- DUTY_SET: $DUT,<0-100>*XX ----
    if (data_len >= 5 && strncmp(data_start, "DUT,", 4) == 0) {
        long duty = strtol(data_start + 4, nullptr, 10);
        if (duty < MIN_SAFE_DUTY_PERCENT || duty > 100) return false;  // P0-4: enforce minimum safe duty
        out.ctrl.duty = static_cast<uint8_t>(duty);
        out.type = FrameType::DUTY_SET;
        return true;
    }

    // ---- SAFETY_RESET: $SAF*XX ----
    if (data_len == 3 && strncmp(data_start, "SAF", 3) == 0) {
        out.type = FrameType::SAFETY_RESET;
        return true;
    }

    return false;
}

// ============================================================================
// Frame finalization: append *XX checksum and \n
// ============================================================================

static size_t finalize_frame(char *buf, size_t buf_size, size_t data_len)
{
    // data_len is length of $TYPE,PAYLOAD in buf
    if (data_len + 4 > buf_size) return 0; // need room for *XX\n\0

    const char *data_start = buf + 1; // skip leading $
    uint8_t cs = calc_checksum(data_start, data_len - 1); // -1 to exclude $ from checksum

    // Append *XX\n to buf at position data_len
    size_t total = data_len + snprintf(buf + data_len, buf_size - data_len, "*%02X\n", cs);
    return total;
}

size_t build_status_response(char *buf, size_t buf_size,
                             int mode, uint8_t duty_pct, int freq_hz,
                             float cpu_temp, float gpu_temp,
                             bool cpu_valid, bool gpu_valid)
{
    int data_len = snprintf(buf, buf_size, "$STP,%d,%d,%d,%.1f,%.1f,%d,%d",
                            mode, duty_pct, freq_hz,
                            cpu_temp, gpu_temp,
                            cpu_valid ? 1 : 0, gpu_valid ? 1 : 0);
    if (data_len < 0 || static_cast<size_t>(data_len) >= buf_size) return 0;
    return finalize_frame(buf, buf_size, static_cast<size_t>(data_len));
}

size_t build_fcurve_response(char *buf, size_t buf_size,
                             const FanCurvePoint *points, uint8_t count)
{
    size_t pos = snprintf(buf, buf_size, "$FCP,%d", count);
    if (pos >= buf_size) return 0;

    for (uint8_t i = 0; i < count; i++) {
        int added = snprintf(buf + pos, buf_size - pos, ",%.1f,%d",
                             points[i].temperature, points[i].duty_percent);
        if (added < 0 || static_cast<size_t>(added) >= buf_size - pos) return 0;
        pos += added;
    }

    return finalize_frame(buf, buf_size, pos);
}

size_t build_ack(char *buf, size_t buf_size)
{
    size_t data_len = snprintf(buf, buf_size, "$ACK");
    if (data_len >= buf_size) return 0;
    return finalize_frame(buf, buf_size, data_len);
}

size_t build_nack(char *buf, size_t buf_size, uint8_t error_code)
{
    int data_len = snprintf(buf, buf_size, "$NAK,%02d", error_code);
    if (data_len < 0 || static_cast<size_t>(data_len) >= buf_size) return 0;
    return finalize_frame(buf, buf_size, static_cast<size_t>(data_len));
}
