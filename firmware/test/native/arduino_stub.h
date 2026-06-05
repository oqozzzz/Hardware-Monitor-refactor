#ifndef ARDUINO_STUB_H
#define ARDUINO_STUB_H

// Minimal stub for Arduino APIs used in firmware logic
// Enables desktop (native) unit testing without real hardware

#include <cstdint>
#include <cstddef>
#include <cmath>
#include <cstring>
#include <cstdio>

// ---- Arduino type stubs ----
typedef uint8_t byte;

// ---- String stub ----
class String {
public:
    String() : _data(nullptr) {}
    String(const char* s) {
        if (s) { _data = strdup(s); }
    }
    ~String() { free(_data); }
    const char* c_str() const { return _data ? _data : ""; }
private:
    char* _data;
};

// ---- Serial stub ----
class SerialStub {
public:
    void begin(int) {}
    void print(const char*) {}
    void print(char) {}
    void print(int) {}
    void print(unsigned int) {}
    void print(float, int = 2) {}
    void println() {}
    void println(const char*) {}
};
extern SerialStub Serial;

// ---- Math stubs ----
inline float fabsf(float x) { return std::fabs(x); }
inline int isnan(float x) { return std::isnan(x); }
inline int isinf(float x) { return std::isinf(x); }

// ---- Time stubs ----
extern uint32_t stub_millis;
inline uint32_t millis() { return stub_millis; }
inline void advance_millis(uint32_t ms) { stub_millis += ms; }

// ---- LEDC stubs (PWM) ----
inline void ledcSetup(int, int, int) {}
inline void ledcAttachPin(int, int) {}
inline void ledcWrite(int, uint8_t) {}

// ---- Pin stubs ----
inline void pinMode(int, int) {}
constexpr int INPUT_PULLDOWN = 0;

// ---- Map function ----
inline long map(long x, long in_min, long in_max, long out_min, long out_max) {
    return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
}

// ---- F() macro stub ----
#define F(x) (x)

#endif // ARDUINO_STUB_H
