/////////////////////////////////////////////////////////////////
/*
  ESP32 + UWB | Indoor Positioning + Unity Visualization
  For More Information: https://youtu.be/c8Pn7lS5Ppg
  Created by Eric N. (ThatProject)
*/
/////////////////////////////////////////////////////////////////

#include <SPI.h>
#include "DW1000Ranging.h"
#include "WiFi.h"
#include <WiFiUdp.h>

// connection pins
const uint8_t PIN_SCK = 18;
const uint8_t PIN_MOSI = 23;
const uint8_t PIN_MISO = 19;
const uint8_t PIN_SS = 15;
const uint8_t PIN_RST = 2;
const uint8_t PIN_IRQ = 22;

const char * ssid = "ThatProject";
const char * password = "California";
const char * udpAddress = "192.168.0.2"; //My UDP Server IP
const int udpPort = 8080;

boolean connected = false;
WiFiUDP udp;

void setup() {
  Serial.begin(115200);
  delay(1000);
  SPI.begin(PIN_SCK, PIN_MISO, PIN_MOSI);
  //init the configuration
  DW1000Ranging.initCommunication(PIN_RST, PIN_SS, PIN_IRQ); //Reset, CS, IRQ pin
  //define the sketch as anchor. It will be great to dynamically change the type of module
  DW1000Ranging.attachNewRange(newRange);
  DW1000Ranging.attachNewDevice(newDevice);
  DW1000Ranging.attachInactiveDevice(inactiveDevice);
  //Enable the filter to smooth the distance
  //DW1000Ranging.useRangeFilter(true);

  DW1000.enableDebounceClock();
  DW1000.enableLedBlinking();
  DW1000.setGPIOMode(MSGP3, LED_MODE);

  //we start the module as a tag
  DW1000Ranging.startAsTag("7D:00:22:EA:82:60:3B:9C", DW1000.MODE_LONGDATA_RANGE_LOWPOWER);

  connectToWiFi(ssid, password);
}

void loop() {
  DW1000Ranging.loop();
}

void newRange() {
  float projectedRange = DW1000Ranging.getDistantDevice()->getRange() * 2 / 5;
  String strData = String(DW1000Ranging.getDistantDevice()->getShortAddress(), HEX);
  strData += ", ";
  strData += String(projectedRange);
  Serial.println(strData);

  if(connected){
    udp.beginPacket(udpAddress, udpPort);
    udp.write((uint8_t *)strData.c_str(), strlen(strData.c_str()));
    udp.endPacket();
  }
}

void newDevice(DW1000Device* device) {
  Serial.print("ranging init; 1 device added ! -> ");
  Serial.print(" short:");
  Serial.println(device->getShortAddress(), HEX);
}

void inactiveDevice(DW1000Device* device) {
  Serial.print("delete inactive device: ");
  Serial.println(device->getShortAddress(), HEX);
}

void WiFiEvent(WiFiEvent_t event){
    switch(event) {
      case SYSTEM_EVENT_STA_GOT_IP:
          Serial.print("WiFi connected! IP address: ");
          Serial.println(WiFi.localIP());  
          udp.begin(WiFi.localIP(),udpPort);
          connected = true;
          break;
      case SYSTEM_EVENT_STA_DISCONNECTED:
          Serial.println("WiFi lost connection");
          connected = false;
          break;
    }
}

void connectToWiFi(const char * ssid, const char * pwd){
  Serial.println();
  Serial.println("Connecting to WiFi network: " + String(ssid));
  WiFi.disconnect(true);
  WiFi.onEvent(WiFiEvent);
  WiFi.begin(ssid, pwd);
  Serial.println("Waiting for WIFI connection...");
}