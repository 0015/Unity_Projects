/////////////////////////////////////////////////////////////////
/*
  ESP32 + UWB + IMU | Indoor Position & Rotation + Unity Visualization
  For More Information: https://youtu.be/fPuxcjHsfpc
  Created by Eric N. (ThatProject)
*/
/////////////////////////////////////////////////////////////////
///////////////////
//ESP Version 1.0.6
///////////////////
#include <Wire.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_BNO055.h>
#include <SPI.h>
#include <DW1000Ranging.h>
#include <WiFi.h>
#include <WiFiUdp.h>
#include "MyDisplay.h"

#define I2C_SDA 17
#define I2C_SCL 16

const uint8_t MY_PIN_SCK = 18;
const uint8_t MY_PIN_MOSI = 23;
const uint8_t MY_PIN_MISO = 19;
const uint8_t MY_PIN_SS = 15;
const uint8_t MY_PIN_RST = 2;
const uint8_t MY_PIN_IRQ = 22;

const char *ssid = "ThatProject";
const char *password = "California";
const char *udpAddress = "192.168.0.2";  //My UDP Server IP
const int udpPort = 8080;
unsigned long udpTimer = 0;
bool isWiFiConnected = false;

WiFiUDP udp;

TwoWire I2CBNO = TwoWire(0);
Adafruit_BNO055 bno = Adafruit_BNO055(55, 0x29, &I2CBNO);

MyDisplay *display;

static String uwbDevice[2];
static portMUX_TYPE mux = portMUX_INITIALIZER_UNLOCKED;
static const uint16_t timer_divider = 80;
static const uint64_t timer_max_count = 1000;
static hw_timer_t *timer = NULL;
static SemaphoreHandle_t bin_sem = NULL;
uint32_t cp0_regs[18];
void IRAM_ATTR onTimer() {
  uint32_t cp_state = xthal_get_cpenable();
  if (cp_state) {
    xthal_save_cp0(cp0_regs);
  } else {
    xthal_set_cpenable(1);
  }

  portENTER_CRITICAL_ISR(&mux);
  DW1000Ranging.loop();
  portEXIT_CRITICAL_ISR(&mux);

  BaseType_t task_woken = pdFALSE;
  xSemaphoreGiveFromISR(bin_sem, &task_woken);
  if (task_woken == pdTRUE) {
    portYIELD_FROM_ISR();
  }

  if (cp_state) {
    xthal_restore_cp0(cp0_regs);
  } else {
    xthal_set_cpenable(0);
  }
}

void setup() {
  Serial.begin(115200);
  initDisplay();
  
  bin_sem = xSemaphoreCreateBinary();
  if (bin_sem == NULL) {
    Serial.println("Could not create semaphore");
    ESP.restart();
  }

  udpTimer = 0;
  
  xTaskCreatePinnedToCore(system_task,
                          "Task System",
                          2048,
                          NULL,
                          1,
                          NULL,
                          1);

  connectToWiFi(ssid, password);
}

void setTimerInterrupt() {
  timer = timerBegin(0, timer_divider, true);
  timerAttachInterrupt(timer, &onTimer, true);
  timerAlarmWrite(timer, timer_max_count, true);
  timerAlarmEnable(timer);
}

void loop() {}

void system_task(void *parameters) {

  I2CBNO.begin(I2C_SDA, I2C_SCL);

  if (!bno.begin()) {
    Serial.println("No BNO055 detected");
    while (1) {
      vTaskDelay(10);
    }
  }

  vTaskDelay(100);

  uint8_t system, gyro, accel, mag = 0;
  display->imuLog("Calibration!", false);
  while (system != 3) {
    bno.getCalibration(&system, &gyro, &accel, &mag);
    vTaskDelay(100);
  }

  display->imuLog("All Set!", true);
  display->setIsAllReady(true);

  while (1) {
    if (xSemaphoreTake(bin_sem, portMAX_DELAY) == pdTRUE) {
      portENTER_CRITICAL(&mux);
      display->loop();
      display->uwbLog(uwbDevice[0] + " m", 0);
      display->uwbLog(uwbDevice[1] + " m", 1);
      portEXIT_CRITICAL(&mux);
      if (millis() - udpTimer >= 100) {
          udpTimer = millis();
          imu::Quaternion quat = bno.getQuat();
          String strData = String(quat.w(), 4) + ", " + String(quat.x(), 4) + ", " + String(quat.y(), 4) + ", " + String(quat.z(), 4);
          sendData(strData);
      }else{
        if(uwbDevice[0].length() > 1 && uwbDevice[1].length() >1){
          String strData = uwbDevice[0];
          strData += ", ";
          strData += uwbDevice[1];
          sendData(strData);
        }
      }
    }
    vTaskDelay(30);
  }
}

void sendData(String strData){
  if (!isWiFiConnected) return;
  udp.beginPacket(udpAddress, udpPort);
  udp.write((uint8_t *)strData.c_str(), strlen(strData.c_str()));
  udp.endPacket();
}

void initDisplay() {
  display = new MyDisplay();
  display->initTFT();
  display->systemLog("[System Up]", true);
}

String getQuaternion() {
  imu::Quaternion quat = bno.getQuat();
  return String(quat.w(), 4) + ", " + String(quat.x(), 4) + ", " + String(quat.y(), 4) + ", " + String(quat.z(), 4);
}

void initUWB() {
  SPI.begin(MY_PIN_SCK, MY_PIN_MISO, MY_PIN_MOSI);

  DW1000Ranging.initCommunication(MY_PIN_RST, MY_PIN_SS, MY_PIN_IRQ);
  DW1000Ranging.attachNewRange(newRange);
  DW1000Ranging.attachNewDevice(newDevice);
  DW1000Ranging.attachInactiveDevice(inactiveDevice);

  DW1000.enableDebounceClock();
  DW1000.enableLedBlinking();
  DW1000.setGPIOMode(MSGP0, LED_MODE);
  DW1000.setGPIOMode(MSGP1, LED_MODE);
  DW1000.setGPIOMode(MSGP2, LED_MODE);
  DW1000.setGPIOMode(MSGP3, LED_MODE);

  DW1000Ranging.startAsTag("7D:00:22:EA:82:60:3B:9C", DW1000.MODE_LONGDATA_RANGE_LOWPOWER);
  setTimerInterrupt();
}

void newRange() {
  float projectedRange = DW1000Ranging.getDistantDevice()->getRange() * 2 / 5;
  String shortName = String(DW1000Ranging.getDistantDevice()->getShortAddress(), HEX);
  uwbDevice[shortName == "aabb" ? 0 : 1] = String(projectedRange);
}

void newDevice(DW1000Device *device) {
  String shortName = String(device->getShortAddress(), HEX);
  display->uwbStatus(shortName == "aabb", true);
}

void inactiveDevice(DW1000Device *device) {
  String shortName = String(device->getShortAddress(), HEX);
  display->uwbStatus(shortName == "aabb", false);
}

void WiFiEvent(WiFiEvent_t event) {
  switch (event) {
    case SYSTEM_EVENT_STA_GOT_IP:
      display->systemLog(WiFi.localIP().toString());
      display->systemLog("IP address:");
      display->systemLog("Connected!");
      display->systemLog("WiFi", true);

      Serial.print("WiFi connected! IP address: ");
      Serial.println(WiFi.localIP());
      udp.begin(WiFi.localIP(), udpPort);

      display->systemLog("UDP begins!", true);
      isWiFiConnected = true;

      initUWB();
      break;
    case SYSTEM_EVENT_STA_DISCONNECTED:

      isWiFiConnected = false;
      display->systemLog("Disconnected!");
      display->systemLog("WiFi", true);
      display->setIsAllReady(false);
      break;
  }
}

void connectToWiFi(const char *ssid, const char *pwd) {
  WiFi.disconnect(true);
  WiFi.onEvent(WiFiEvent);
  WiFi.begin(ssid, pwd);
  display->systemLog("Connecting!");
  display->systemLog("WIFI", true);
}
