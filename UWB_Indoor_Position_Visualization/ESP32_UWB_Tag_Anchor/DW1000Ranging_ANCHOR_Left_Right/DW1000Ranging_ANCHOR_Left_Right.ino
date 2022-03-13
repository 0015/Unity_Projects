/////////////////////////////////////////////////////////////////
/*
  ESP32 + UWB | Indoor Positioning + Unity Visualization
  For More Information: https://youtu.be/c8Pn7lS5Ppg
  Created by Eric N. (ThatProject)
*/
/////////////////////////////////////////////////////////////////

#include <SPI.h>
#include "DW1000Ranging.h"

// connection pins
const uint8_t PIN_SCK = 18;
const uint8_t PIN_MOSI = 23;
const uint8_t PIN_MISO = 19;
const uint8_t PIN_SS = 15;
const uint8_t PIN_RST = 2;
const uint8_t PIN_IRQ = 22;

void setup() {
  Serial.begin(115200);
  delay(1000);
  //init the configuration
  SPI.begin(PIN_SCK, PIN_MISO, PIN_MOSI);
  DW1000Ranging.initCommunication(PIN_RST, PIN_SS, PIN_IRQ);  //Reset, CS, IRQ pin
  //define the sketch as anchor. It will be great to dynamically change the type of module
  DW1000Ranging.attachNewRange(newRange);
  DW1000Ranging.attachBlinkDevice(newBlink);
  DW1000Ranging.attachInactiveDevice(inactiveDevice);
  //Enable the filter to smooth the distance
  //DW1000Ranging.useRangeFilter(true);

  DW1000.enableDebounceClock();
  DW1000.enableLedBlinking();
  DW1000.setGPIOMode(MSGP3, LED_MODE);
  
  //Left Anchor - short name of this anchor: aabb
  //DW1000Ranging.startAsAnchor("BB:AA:5B:D5:A9:9A:E2:9C", DW1000.MODE_LONGDATA_RANGE_LOWPOWER, false); 

  //Right Anchor - short name of this anchor: ccdd
  DW1000Ranging.startAsAnchor("DD:CC:5B:D5:A9:9A:E2:9C", DW1000.MODE_LONGDATA_RANGE_LOWPOWER, false);
}

void loop() {
  DW1000Ranging.loop();
}

void newRange() {
  updateRange(DW1000Ranging.getDistantDevice()->getRange());
}

void newBlink(DW1000Device* device) {
}

void inactiveDevice(DW1000Device* device) {
}

void updateRange(float range) {
}
