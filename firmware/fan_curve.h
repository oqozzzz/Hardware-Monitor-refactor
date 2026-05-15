#ifndef FAN_CURVE_H
#define FAN_CURVE_H

#include <Arduino.h>

// ============================================================================
// 风扇曲线运行时模块
// 取代编译期 constexpr 数组，支持通过蓝牙协议在线更新
// ============================================================================

#define MAX_CURVE_POINTS 10

struct FanCurvePoint {
    float   temperature;
    uint8_t duty_percent;
};

class FanCurve
{
public:
    // 重置为 config.h 中的默认曲线
    void reset_to_default();

    // 运行时设置曲线点 (count ∈ [2, MAX_CURVE_POINTS])
    // 返回 false 如果验证失败: 温度非严格递增 / duty 超出 0-100
    bool set_points(const FanCurvePoint *points, uint8_t count);

    // 线性插值查询温度对应的占空比(%)
    uint8_t lookup(float temp) const;

    // 访问器
    uint8_t               get_count()  const { return _count; }
    const FanCurvePoint * get_points() const { return _points; }

private:
    FanCurvePoint _points[MAX_CURVE_POINTS];
    uint8_t       _count;
};

#endif // FAN_CURVE_H
