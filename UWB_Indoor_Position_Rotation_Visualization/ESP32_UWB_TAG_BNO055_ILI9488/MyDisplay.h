/////////////////////////////////////////////////////////////////
/*
  ESP32 + UWB + IMU | Indoor Position & Rotation + Unity Visualization
  For More Information: https://youtu.be/fPuxcjHsfpc
  Created by Eric N. (ThatProject)
*/
/////////////////////////////////////////////////////////////////
#ifndef MyDisplay_H_
#define MyDisplay_H_

#include <TFT_eSPI.h>
class MyDisplay {
private:
  TFT_eSPI* tft;
  TFT_eSprite* sLog;
  unsigned long TFT_timer;
  bool isAllReady;
  int pyramidImageIdx;
  void playPyramidImages();
public:
  MyDisplay();
  ~MyDisplay();
  void initTFT();
  void loop();
  void setIsAllReady(bool isOn);
  void systemLog(String str, bool moreSpace=false);
  void imuLog(String str, bool isSuccess);
  void imuResetCountDown(String str);
  void uwbStatus(bool isRight, bool isOn);
  void uwbLog(String str, int anchorNum);
};
#endif