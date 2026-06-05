#include <Arduino.h>
#include "BluetoothSerial.h"
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>
#include <Wire.h>

#include "config.h"
#include "fan_curve.h"
#include "system_state.h"
#include "protocol.h"
#include "task_bt_rx.h"
#include "task_bt_tx.h"
#include "task_control.h"
#include "task_pwm.h"
#include "task_ui.h"
#include "task_button.h"
#include "safety.h"

// ============================================================================
// Global objects
// ============================================================================
BluetoothSerial SerialBT;

Adafruit_SSD1306 display(SCREEN_WIDTH, SCREEN_HEIGHT, &Wire, OLED_RESET);

const int BUTTON_PINS[BTN_COUNT] = {
    PIN_BTN_MODE,
    PIN_BTN_FREQ_UP,
    PIN_BTN_FREQ_DN,
    PIN_BTN_DUTY_UP,
    PIN_BTN_DUTY_DN
};

// ============================================================================
// Setup — initialize hardware and start FreeRTOS tasks
// ============================================================================
void setup()
{
    Serial.begin(115200);
    Serial.println(F("\n============================================"));
    Serial.println(F("  ESP32 Fan Controller - Production FW " FIRMWARE_VERSION));
    Serial.println(F("============================================"));

    // ------------------------------------------------------------------------
    // Bluetooth SPP setup
    // ------------------------------------------------------------------------
    SerialBT.begin("ESP32_FanController");
    SerialBT.setPin(BT_PIN_CODE);  // P0-3: Enable PIN authentication
    Serial.println(F("[INIT] Bluetooth started (PIN auth enabled)"));

    // ------------------------------------------------------------------------
    // PWM setup: 25kHz, 8-bit resolution, initial duty 0%
    // ------------------------------------------------------------------------
    ledcSetup(PWM_CHANNEL, PWM_FREQ_HZ, PWM_RES_BITS);
    ledcAttachPin(PIN_PWM_FAN, PWM_CHANNEL);
    ledcWrite(PWM_CHANNEL, 0);
    Serial.println(F("[INIT] PWM initialized @ 25kHz"));

    // ------------------------------------------------------------------------
    // Button pins: INPUT_PULLDOWN (active HIGH)
    // ------------------------------------------------------------------------
    for (int i = 0; i < BTN_COUNT; i++) {
        pinMode(BUTTON_PINS[i], INPUT_PULLDOWN);
    }
    Serial.println(F("[INIT] Buttons initialized"));

    // ------------------------------------------------------------------------
    // OLED display init (SSD1306 128x64 I2C)
    // ------------------------------------------------------------------------
    if (!display.begin(SSD1306_SWITCHCAPVCC, 0x3C)) {
        Serial.println(F("[WARN] SSD1306 init failed, running headless"));
    } else {
        display.clearDisplay();
        display.setTextSize(1);
        display.setTextColor(SSD1306_WHITE);
        display.setCursor(24, 28);
        display.print(F("Fan Ctrl " FIRMWARE_VERSION));
        display.display();
        Serial.println(F("[INIT] OLED initialized"));
    }

    // ------------------------------------------------------------------------
    // Initialize shared system state (mutex + defaults)
    // ------------------------------------------------------------------------
    if (!state_init()) {
        Serial.println(F("[ERR] State init failed!"));
    }

    // ------------------------------------------------------------------------
    // Create Bluetooth TX queue
    // ------------------------------------------------------------------------
    g_state.tx_queue = xQueueCreate(TX_QUEUE_SIZE, TX_BUF_SIZE);
    if (!g_state.tx_queue) {
        Serial.println(F("[ERR] TX queue creation failed!"));
    } else {
        Serial.println(F("[INIT] TX queue created"));
    }

    // ------------------------------------------------------------------------
    // Start FreeRTOS tasks (all pinned to Core 1 / APP CPU)
    // ------------------------------------------------------------------------
    xTaskCreatePinnedToCore(task_bt_rx,   "bt_rx",   STACK_BT_RX,   NULL, PRIO_BT_RX,   NULL, 1);
    xTaskCreatePinnedToCore(task_bt_tx,   "bt_tx",   STACK_BT_TX,   NULL, PRIO_BT_TX,   NULL, 1);
    xTaskCreatePinnedToCore(task_control, "control", STACK_CONTROL, NULL, PRIO_CONTROL, NULL, 1);
    xTaskCreatePinnedToCore(task_pwm,     "pwm",     STACK_PWM,     NULL, PRIO_PWM,     NULL, 1);
    xTaskCreatePinnedToCore(task_ui,      "ui",      STACK_UI,      NULL, PRIO_UI,      NULL, 1);
    xTaskCreatePinnedToCore(task_button,  "button",  STACK_BUTTON,  NULL, PRIO_BUTTON,  NULL, 1);
    xTaskCreatePinnedToCore(task_safety,  "safety",  STACK_SAFETY,  NULL, PRIO_SAFETY,  NULL, 1);

    Serial.println(F("[INIT] All tasks started successfully"));
}

// ============================================================================
// Main loop — periodic status print every 10s
// ============================================================================
void loop()
{
    static uint32_t last_print = 0;
    uint32_t now = millis();

    if (now - last_print >= 10000) {
        state_lock();
        Serial.print(F("[STATUS] Mode="));
        Serial.print(static_cast<int>(g_state.mode));
        Serial.print(F(" Duty="));
        Serial.print(map(g_state.current_duty, 0, 255, 0, 100));
        Serial.print(F("% Target="));
        Serial.print(map(g_state.target_duty, 0, 255, 0, 100));
        Serial.print(F("% CPU="));
        Serial.print(g_state.cpu_temp, 1);
        Serial.print(F(" GPU="));
        Serial.print(g_state.gpu_temp, 1);
        Serial.print(F(" Max="));
        Serial.print(g_state.max_temp, 1);
        Serial.print(F(" CurvePts="));
        Serial.print(g_state.fan_curve.get_count());
        Serial.println();
        state_unlock();

        last_print = now;
    }

    vTaskDelay(pdMS_TO_TICKS(1000));
}
