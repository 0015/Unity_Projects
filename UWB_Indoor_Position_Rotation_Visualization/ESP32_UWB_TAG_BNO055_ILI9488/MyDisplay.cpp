/////////////////////////////////////////////////////////////////
/*
  ESP32 + UWB + IMU | Indoor Position & Rotation + Unity Visualization
  For More Information: https://youtu.be/fPuxcjHsfpc
  Created by Eric N. (ThatProject)
*/
/////////////////////////////////////////////////////////////////
#include "MyDisplay.h"
#include "bg_image.h"
#include "pyramid_image_1.h"
#include "pyramid_image_2.h"
#include "pyramid_image_3.h"

MyDisplay::MyDisplay() {
  tft = new TFT_eSPI();
  sLog = new TFT_eSprite(tft);
  TFT_timer = 0;
  isAllReady = false;
  pyramidImageIdx = 0;
}

MyDisplay::~MyDisplay() {
  delete tft;
}

void MyDisplay::initTFT() {
  tft->init();
  tft->setRotation(1);
  tft->setFreeFont(&FreeSansBold9pt7b);
  tft->fillScreen(TFT_WHITE);
  tft->setTextColor(TFT_BLACK, TFT_WHITE);  
  tft->setSwapBytes(true);
  tft->pushImage(0, 0, BGImageWidth, BGImageHeight, BGImage);

  sLog->setColorDepth(1);
  sLog->createSprite(160, 260);
  sLog->fillSprite(TFT_WHITE); 
  sLog->setScrollRect(0, 0, 160, 260, TFT_WHITE);
  sLog->setTextColor(TFT_BLACK);
  sLog->setTextDatum(TL_DATUM);
}

void MyDisplay::loop() {
 if (isAllReady && millis() - TFT_timer >= 200) {
    TFT_timer = millis();
    playPyramidImages();
  }
}

void MyDisplay::setIsAllReady(bool isOn){
  isAllReady = isOn;

  if(!isOn){
    tft->fillRect(320, 60, 160, 220, TFT_WHITE);
  }
}

void MyDisplay::playPyramidImages(){
  if(pyramidImageIdx > 2) pyramidImageIdx = 0;
  switch(pyramidImageIdx){
    case 0:
      tft->pushImage(340, 130, 120, 120, PyramidImage1);
    break;
    case 1:
      tft->pushImage(340, 130, 120, 120, PyramidImage2);
    break;
    case 2:
      tft->pushImage(340, 130, 120, 120, PyramidImage3);
    break;
    default:
    break;
  }
  ++pyramidImageIdx;
}

void MyDisplay::systemLog(String str, bool moreSpace){
  sLog->drawString(str, 2, 2, 4); 
  sLog->pushSprite(0, 60);
  sLog->scroll(0, 20);

  if(moreSpace){
    sLog->scroll(0, 18);
  }
}

void MyDisplay::imuLog(String str, bool isSuccess){
  tft->setTextColor(TFT_BLACK, TFT_WHITE);
  tft->fillRect(330, 70, 10, 10, isSuccess? TFT_GREEN : TFT_RED);
  tft->setTextPadding(160);
  tft->drawString(str, 346, 70, 4);
}

void MyDisplay::imuResetCountDown(String str){
  tft->setTextColor(TFT_RED, TFT_WHITE);
  tft->setTextPadding(20);
  tft->drawString(str, 374, 140, 8);
}

void MyDisplay::uwbStatus(bool isRight, bool isOn){
  tft->fillRect(170, isRight ? 70 : 160, 10, 10, isOn ? TFT_GREEN : TFT_RED);
  tft->setTextColor(TFT_WHITE, TFT_BLACK);
  tft->setTextPadding(120);
  tft->drawString(isRight? "Right Anc" : "Left Anc", 190, isRight? 70 : 160, 4);
}

void MyDisplay::uwbLog(String str, int anchorNum){
  tft->setTextColor(TFT_WHITE, TFT_BLACK);
  tft->setTextPadding(120);
  tft->drawString(str, 200, anchorNum==0 ? 100 : 190, 4);
}