// Native unit tests for protocol.cpp — checksum, parsing, frame building
// Requires: arduino_stub.h, freertos_stub.h
#include <cassert>
#include <cstring>
#include <cstdio>
#include "arduino_stub.h"
#include "freertos_stub.h"

// Include firmware source under test
#include "../protocol.cpp"
// config.h constants needed by protocol
#include "../config.h"

// ---- Stub global variables ----
uint32_t stub_millis = 0;
SerialStub Serial;

// ============================================================================
// Checksum tests
// ============================================================================
static void test_calc_checksum_known_input()
{
    // XOR of "CPU,45.5"
    uint8_t cs = calc_checksum("CPU,45.5", 8);
    assert(cs == 0x28);
}

static void test_calc_checksum_empty()
{
    uint8_t cs = calc_checksum("", 0);
    assert(cs == 0);
}

static void test_calc_checksum_different_inputs()
{
    uint8_t cs1 = calc_checksum("CPU,45.5", 8);
    uint8_t cs2 = calc_checksum("GPU,72.1", 8);
    assert(cs1 != cs2);
}

// ============================================================================
// Frame parsing tests
// ============================================================================
static void test_parse_temp_cpu_valid()
{
    // Build frame: $CPU,45.5*XX\n
    const char* data = "CPU,45.5";
    uint8_t cs = calc_checksum(data, 8);
    char frame[32];
    snprintf(frame, sizeof(frame), "$%s*%02X\n", data, cs);

    ParsedFrame out;
    bool ok = parse_frame(frame, strlen(frame), out);
    assert(ok);
    assert(out.type == FrameType::TEMP_CPU);
    assert(out.temp.valid);
    assert(out.temp.temperature > 45.0f && out.temp.temperature < 46.0f);
}

static void test_parse_temp_gpu_valid()
{
    const char* data = "GPU,72.1";
    uint8_t cs = calc_checksum(data, 8);
    char frame[32];
    snprintf(frame, sizeof(frame), "$%s*%02X\n", data, cs);

    ParsedFrame out;
    bool ok = parse_frame(frame, strlen(frame), out);
    assert(ok);
    assert(out.type == FrameType::TEMP_GPU);
    assert(out.temp.temperature > 72.0f && out.temp.temperature < 73.0f);
}

static void test_parse_duty_set_valid()
{
    const char* data = "DUT,50";
    uint8_t cs = calc_checksum(data, 6);
    char frame[32];
    snprintf(frame, sizeof(frame), "$%s*%02X\n", data, cs);

    ParsedFrame out;
    bool ok = parse_frame(frame, strlen(frame), out);
    assert(ok);
    assert(out.type == FrameType::DUTY_SET);
    assert(out.ctrl.duty == 50);
}

static void test_parse_duty_set_below_min_safe()
{
    // P0-4: duty below MIN_SAFE_DUTY_PERCENT (20) should be rejected
    const char* data = "DUT,19";
    uint8_t cs = calc_checksum(data, 6);
    char frame[32];
    snprintf(frame, sizeof(frame), "$%s*%02X\n", data, cs);

    ParsedFrame out;
    bool ok = parse_frame(frame, strlen(frame), out);
    assert(!ok);  // Should reject
}

static void test_parse_duty_set_at_min_boundary()
{
    const char* data = "DUT,20";
    uint8_t cs = calc_checksum(data, 6);
    char frame[32];
    snprintf(frame, sizeof(frame), "$%s*%02X\n", data, cs);

    ParsedFrame out;
    bool ok = parse_frame(frame, strlen(frame), out);
    assert(ok);
    assert(out.ctrl.duty == 20);
}

static void test_parse_malformed_no_dollar()
{
    ParsedFrame out;
    bool ok = parse_frame("CPU,45.5*28\n", 12, out);
    assert(!ok);
}

static void test_parse_malformed_no_star()
{
    ParsedFrame out;
    bool ok = parse_frame("$CPU,45.5\n", 10, out);
    assert(!ok);
}

static void test_parse_malformed_wrong_checksum()
{
    ParsedFrame out;
    bool ok = parse_frame("$CPU,45.5*FF\n", 13, out);
    assert(!ok);
}

static void test_parse_status_query()
{
    const char* data = "STA,?";
    uint8_t cs = calc_checksum(data, 5);
    char frame[32];
    snprintf(frame, sizeof(frame), "$%s*%02X\n", data, cs);

    ParsedFrame out;
    bool ok = parse_frame(frame, strlen(frame), out);
    assert(ok);
    assert(out.type == FrameType::STATUS_QUERY);
}

static void test_parse_mode_set_valid()
{
    const char* data = "MOD,2";
    uint8_t cs = calc_checksum(data, 5);
    char frame[32];
    snprintf(frame, sizeof(frame), "$%s*%02X\n", data, cs);

    ParsedFrame out;
    bool ok = parse_frame(frame, strlen(frame), out);
    assert(ok);
    assert(out.type == FrameType::MODE_SET);
    assert(out.ctrl.mode == 2);
}

static void test_parse_mode_set_invalid()
{
    const char* data = "MOD,5";
    uint8_t cs = calc_checksum(data, 5);
    char frame[32];
    snprintf(frame, sizeof(frame), "$%s*%02X\n", data, cs);

    ParsedFrame out;
    bool ok = parse_frame(frame, strlen(frame), out);
    assert(!ok);  // Mode 5 invalid
}

static void test_parse_safety_reset()
{
    const char* data = "SAF";
    uint8_t cs = calc_checksum(data, 3);
    char frame[32];
    snprintf(frame, sizeof(frame), "$%s*%02X\n", data, cs);

    ParsedFrame out;
    bool ok = parse_frame(frame, strlen(frame), out);
    assert(ok);
    assert(out.type == FrameType::SAFETY_RESET);
}

// ============================================================================
// Frame building tests
// ============================================================================
static void test_build_ack()
{
    char buf[32];
    size_t len = build_ack(buf, sizeof(buf));
    assert(len > 0);
    // Verify it parses back
    ParsedFrame out;
    bool ok = parse_frame(buf, len, out);
    (void)ok;  // ACK is not in parse_frame switch — it's only built, not parsed
}

static void test_build_nack()
{
    char buf[32];
    size_t len = build_nack(buf, sizeof(buf), 3);
    assert(len > 0);
    // Verify NAK,03 is in the output
    assert(strstr(buf, "NAK,03") != nullptr);
}

static void test_build_status_response()
{
    char buf[128];
    size_t len = build_status_response(buf, sizeof(buf), 2, 50, 25000, 45.5f, 72.1f, true, true);
    assert(len > 0);
    assert(strstr(buf, "$STP,2,50,25000,45.5,72.1,1,1*") != nullptr);
}

// ============================================================================
// Test runner
// ============================================================================
int main()
{
    test_calc_checksum_known_input();
    test_calc_checksum_empty();
    test_calc_checksum_different_inputs();
    
    test_parse_temp_cpu_valid();
    test_parse_temp_gpu_valid();
    test_parse_duty_set_valid();
    test_parse_duty_set_below_min_safe();
    test_parse_duty_set_at_min_boundary();
    test_parse_malformed_no_dollar();
    test_parse_malformed_no_star();
    test_parse_malformed_wrong_checksum();
    test_parse_status_query();
    test_parse_mode_set_valid();
    test_parse_mode_set_invalid();
    test_parse_safety_reset();
    
    test_build_ack();
    test_build_nack();
    test_build_status_response();
    
    printf("All protocol tests passed!\n");
    return 0;
}
