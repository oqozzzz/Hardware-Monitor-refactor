#ifndef FAN_CURVE_H
#define FAN_CURVE_H

#include <Arduino.h>

// ============================================================================
// Fan curve runtime module
// Replaces compile-time constexpr array, supports online updates via BT protocol
// ============================================================================

#define MAX_CURVE_POINTS 10

struct FanCurvePoint {
    float   temperature;
    uint8_t duty_percent;
};

class FanCurve
{
public:
    // Reset to default curve from config.h
    void reset_to_default();

    // Set curve points at runtime (count in [2, MAX_CURVE_POINTS])
    // Returns false if validation fails: temperature not strictly ascending / duty out of 0-100
    bool set_points(const FanCurvePoint *points, uint8_t count);

    // Catmull-Rom spline interpolation query for duty cycle (%) at given temperature
    uint8_t lookup(float temp) const;

    // Accessors
    uint8_t               get_count()  const { return _count; }
    const FanCurvePoint * get_points() const { return _points; }

private:
    FanCurvePoint _points[MAX_CURVE_POINTS];
    uint8_t       _count;
};

#endif // FAN_CURVE_H
