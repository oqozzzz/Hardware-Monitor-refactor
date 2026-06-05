// Native unit tests for fan_curve.cpp — set_points, lookup, interpolation
// Requires: arduino_stub.h
// Build: g++ -std=c++11 -I.. -o test_fan_curve test_fan_curve.cpp ../fan_curve.cpp
#include <cassert>
#include <cstdio>
#include "arduino_stub.h"

// Include firmware source (config.h constants needed by fan_curve)
#include "../config.h"
#include "../fan_curve.cpp"

uint32_t stub_millis = 0;
SerialStub Serial;

// ============================================================================
// set_points tests
// ============================================================================
static void test_set_points_valid()
{
    FanCurve curve;
    FanCurvePoint pts[3] = {
        {0.0f, 20},
        {50.0f, 50},
        {100.0f, 100},
    };
    bool ok = curve.set_points(pts, 3);
    assert(ok);
    assert(curve.get_count() == 3);
}

static void test_set_points_too_few()
{
    FanCurve curve;
    FanCurvePoint pts[1] = {{0.0f, 20}};
    bool ok = curve.set_points(pts, 1);
    assert(!ok);
}

static void test_set_points_below_min_duty()
{
    // P0-5: duty below MIN_SAFE_DUTY_PERCENT should be rejected
    FanCurve curve;
    FanCurvePoint pts[2] = {
        {0.0f, 19},
        {50.0f, 50},
    };
    bool ok = curve.set_points(pts, 2);
    assert(!ok);
}

static void test_set_points_at_min_duty_boundary()
{
    FanCurve curve;
    FanCurvePoint pts[2] = {
        {0.0f, 20},
        {50.0f, 50},
    };
    bool ok = curve.set_points(pts, 2);
    assert(ok);
}

static void test_set_points_non_monotonic_temp()
{
    FanCurve curve;
    FanCurvePoint pts[3] = {
        {0.0f, 20},
        {50.0f, 50},
        {30.0f, 60},  // Temperature goes down!
    };
    bool ok = curve.set_points(pts, 3);
    assert(!ok);
}

static void test_set_points_first_point_non_zero()
{
    FanCurve curve;
    FanCurvePoint pts[2] = {
        {10.0f, 20},  // First point must be ~0
        {50.0f, 50},
    };
    bool ok = curve.set_points(pts, 2);
    assert(!ok);
}

// ============================================================================
// lookup tests
// ============================================================================
static void test_lookup_below_range()
{
    FanCurve curve;
    curve.reset_to_default();
    // Below first point (0°C → 20%)
    uint8_t duty = curve.lookup(-10.0f);
    assert(duty == 20);
}

static void test_lookup_above_range()
{
    FanCurve curve;
    curve.reset_to_default();
    // Above last point (100°C → 100%)
    uint8_t duty = curve.lookup(120.0f);
    assert(duty == 100);
}

static void test_lookup_at_point()
{
    FanCurve curve;
    FanCurvePoint pts[3] = {
        {0.0f, 20},
        {50.0f, 50},
        {100.0f, 100},
    };
    curve.set_points(pts, 3);
    
    uint8_t d0 = curve.lookup(0.0f);
    assert(d0 == 20);
    
    uint8_t d50 = curve.lookup(50.0f);
    assert(d50 == 50);
    
    uint8_t d100 = curve.lookup(100.0f);
    assert(d100 == 100);
}

static void test_lookup_interpolation_between_points()
{
    FanCurve curve;
    FanCurvePoint pts[3] = {
        {0.0f, 20},
        {50.0f, 50},
        {100.0f, 100},
    };
    curve.set_points(pts, 3);
    
    // At 25°C (midpoint of 0-50), duty should be between 20 and 50
    uint8_t d25 = curve.lookup(25.0f);
    assert(d25 >= 20);
    assert(d25 <= 50);
    
    uint8_t d75 = curve.lookup(75.0f);
    assert(d75 >= 50);
    assert(d75 <= 100);
}

static void test_lookup_empty_curve_returns_full()
{
    // Edge case: no points should return 100 (full speed safety)
    FanCurve curve;
    // curve is empty (0 points)
    uint8_t duty = curve.lookup(50.0f);
    assert(duty == 100);
}

// ============================================================================
// reset_to_default tests
// ============================================================================
static void test_reset_to_default()
{
    FanCurve curve;
    curve.reset_to_default();
    assert(curve.get_count() == DEFAULT_FAN_CURVE_POINTS);
    
    // First point should be (0°C, 20%)
    uint8_t d0 = curve.lookup(0.0f);
    assert(d0 >= MIN_SAFE_DUTY_PERCENT);
    
    // Last point should be (100°C, 100%)
    uint8_t d100 = curve.lookup(100.0f);
    assert(d100 == 100);
}

// ============================================================================
// Test runner
// ============================================================================
int main()
{
    test_set_points_valid();
    test_set_points_too_few();
    test_set_points_below_min_duty();
    test_set_points_at_min_duty_boundary();
    test_set_points_non_monotonic_temp();
    test_set_points_first_point_non_zero();
    
    test_lookup_below_range();
    test_lookup_above_range();
    test_lookup_at_point();
    test_lookup_interpolation_between_points();
    test_lookup_empty_curve_returns_full();
    
    test_reset_to_default();
    
    printf("All fan_curve tests passed!\n");
    return 0;
}
