#include "fan_curve.h"
#include "config.h"

void FanCurve::reset_to_default()
{
    _count = DEFAULT_FAN_CURVE_POINTS;
    for (int i = 0; i < _count; i++) {
        _points[i].temperature  = DEFAULT_FAN_CURVE[i].temp;
        _points[i].duty_percent = DEFAULT_FAN_CURVE[i].duty_percent;
    }
}

bool FanCurve::set_points(const FanCurvePoint *points, uint8_t count)
{
    if (count < 2 || count > MAX_CURVE_POINTS) return false;

    //         duty   [0, 100]
    for (uint8_t i = 0; i < count; i++) {
        if (points[i].duty_percent > 100) return false;
        if (i > 0 && points[i].temperature <= points[i - 1].temperature) return false;
    }
    // First point temperature must be ~0 (tolerance for float parsing)
    if (fabsf(points[0].temperature) > 0.01f) return false;

    _count = count;
    for (uint8_t i = 0; i < count; i++) {
        _points[i] = points[i];
    }
    return true;
}

uint8_t FanCurve::lookup(float temp) const
{
    if (_count == 0) return 100; //     

    if (temp <= _points[0].temperature)
        return _points[0].duty_percent;

    if (temp >= _points[_count - 1].temperature)
        return _points[_count - 1].duty_percent;

    for (uint8_t i = 0; i < _count - 1; i++) {
        if (temp >= _points[i].temperature && temp < _points[i + 1].temperature) {
            // Catmull-Rom spline (=0.5): C1 continuous, monotonicity preserved via clamp
            uint8_t i0 = (i > 0) ? i - 1 : i;
            uint8_t i1 = i;
            uint8_t i2 = i + 1;
            uint8_t i3 = (i < _count - 2) ? i + 2 : i + 1;

            float d0 = static_cast<float>(_points[i0].duty_percent);
            float d1 = static_cast<float>(_points[i1].duty_percent);
            float d2 = static_cast<float>(_points[i2].duty_percent);
            float d3 = static_cast<float>(_points[i3].duty_percent);

            float t = (temp - _points[i1].temperature)
                    / (_points[i2].temperature - _points[i1].temperature);
            float tt = t * t;
            float ttt = tt * t;

            float c0 = -0.5f * ttt +        tt - 0.5f * t;
            float c1 =  1.5f * ttt - 2.5f * tt + 1.0f;
            float c2 = -1.5f * ttt + 2.0f * tt + 0.5f * t;
            float c3 =  0.5f * ttt - 0.5f * tt;

            float duty = c0 * d0 + c1 * d1 + c2 * d2 + c3 * d3;

            //           
            if (duty < d1) duty = d1;
            if (duty > d2) duty = d2;

            return static_cast<uint8_t>(duty);
        }
    }

    return _points[_count - 1].duty_percent;
}
