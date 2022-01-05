/////////////////////////////////////////////////////////////////
/*
  Interactive Particles Responsive Made With ESP32 + INMP441 & Unity
  For More Information: https://youtu.be/lRj01J-cxew
  Created by Eric N. (ThatProject)
*/
/////////////////////////////////////////////////////////////////

#include <driver/i2s.h>

#define I2S_WS_RX 32
#define I2S_SCK_RX 33
#define I2S_SD_RX 15
#define I2S_PORT_RX I2S_NUM_0

#define SAMPLE_RATE 16000
#define SAMPLE_BITS 32
#define DMA_BANKS 4
#define DMA_BANK_SIZE 128

TaskHandle_t i2sReadTaskHandler = NULL;

void setup() {
  Serial.begin(115200);
  i2s_RX_init();
  xTaskCreatePinnedToCore(
    i2s_reader_task,
    "i2s_reader_task",
    10000,
    NULL,
    1,
    &i2sReadTaskHandler,
    0);
}

void loop() {}

void i2s_reader_task(void* parameter) {
  while (1) {
    int32_t samples[DMA_BANK_SIZE];
    size_t num_bytes_read = 0;
    i2s_read(I2S_PORT_RX, &samples, DMA_BANK_SIZE, &num_bytes_read, portMAX_DELAY);

    int samples_read = num_bytes_read / 8;
    if (samples_read > 0) {
      float mean = 0;
      for (int i = 0; i < samples_read; ++i) {
        mean += (samples[i] >> 14);
      }
      mean /= samples_read;

      float maxsample = -1e8, minsample = 1e8;
      for (int i = 0; i < samples_read; ++i) {
        minsample = min(minsample, samples[i] - mean);
        maxsample = max(maxsample, samples[i] - mean);
      }

      // To narrow the range of the value
      float result = (maxsample - minsample) / 100000000;
      Serial.println(result);
      vTaskDelay(1);
    }
  }
}

const i2s_config_t i2s_config_rx = {
  .mode = i2s_mode_t(I2S_MODE_MASTER | I2S_MODE_RX),
  .sample_rate = SAMPLE_RATE,
  .bits_per_sample = I2S_BITS_PER_SAMPLE_32BIT,
  .channel_format = I2S_CHANNEL_FMT_ONLY_RIGHT,
  .communication_format = i2s_comm_format_t(I2S_COMM_FORMAT_STAND_I2S),
  .intr_alloc_flags = ESP_INTR_FLAG_LEVEL1,
  .dma_buf_count = DMA_BANKS,
  .dma_buf_len = DMA_BANK_SIZE
};

const i2s_pin_config_t pin_config_rx = {
  .bck_io_num = I2S_SCK_RX,
  .ws_io_num = I2S_WS_RX,
  .data_out_num = I2S_PIN_NO_CHANGE,
  .data_in_num = I2S_SD_RX
};

void i2s_RX_init() {
  i2s_driver_install(I2S_PORT_RX, &i2s_config_rx, 0, NULL);
  i2s_set_pin(I2S_PORT_RX, &pin_config_rx);
}
