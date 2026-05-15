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

    // 验证：温度严格递增，duty 在 [0, 100]
    for (uint8_t i = 0; i < count; i++) {
        if (points[i].duty_percent > 100) return false;
        if (i > 0 && points[i].temperature <= points[i - 1].temperature) return false;
    }
    // 第一点温度必须为 0
    if (points[0].temperature != 0.0f) return false;

    _count = count;
    for (uint8_t i = 0; i < count; i++) {
        _points[i] = points[i];
    }
    return true;
}

uint8_t FanCurve::lookup(float temp) const
{
    if (_count == 0) return 100; // 安全回退

    if (temp <= _points[0].temperature)
        return _points[0].duty_percent;

    if (temp >= _points[_count - 1].temperature)
        return _points[_count - 1].duty_percent;

    for (uint8_t i = 0; i < _count - 1; i++) {
        if (temp >= _points[i].temperature && temp < _points[i + 1].temperature) {
            float t0 = _points[i].temperature;
            float t1 = _points[i + 1].temperature;
            float d0 = static_cast<float>(_points[i].duty_percent);
            float d1 = static_cast<float>(_points[i + 1].duty_percent);
            float duty = d0 + (d1 - d0) * (temp - t0) / (t1 - t0);
            return static_cast<uint8_t>(duty);
        }
    }

    return _points[_count - 1].duty_percent;
}
