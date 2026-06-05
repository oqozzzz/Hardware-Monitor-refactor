#ifndef FREERTOS_STUB_H
#define FREERTOS_STUB_H

// Minimal stub for FreeRTOS APIs used in firmware logic
// Enables desktop (native) unit testing

#include <cstdint>
#include <cstddef>

// ---- Semaphore stub ----
typedef void* SemaphoreHandle_t;
typedef void* QueueHandle_t;

inline SemaphoreHandle_t xSemaphoreCreateMutex() { return (SemaphoreHandle_t)1; }
inline int xSemaphoreTake(SemaphoreHandle_t, uint32_t) { return 1; }
inline int xSemaphoreGive(SemaphoreHandle_t) { return 1; }
inline QueueHandle_t xQueueCreate(int, int) { return (QueueHandle_t)1; }
inline int xQueueSend(QueueHandle_t, const void*, uint32_t) { return 1; }

// ---- Task delay stub ----
inline void vTaskDelay(int) {}

// ---- Macros ----
#define portMAX_DELAY 0xFFFFFFFF
#define pdMS_TO_TICKS(ms) (ms)

#endif // FREERTOS_STUB_H
