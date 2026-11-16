#include <Wire.h>                       // I2C 통신 라이브러리

const int MPU = 0x68;                   // MPU6050 I2C 주소 (AD0 핀이 GND면 0x68)
int16_t ax, ay, az;                     // 가속도 x, y, z (원시값, ±2g 기준 1g = 16384)

void setup() {
  Serial.begin(115200);                 // 시리얼 속도. 유니티 SerialReceiver의 baudRate와 같아야 함
  Wire.begin();                         // I2C 시작 (나노: SDA=A4, SCL=A5)
  Wire.beginTransmission(MPU);
  Wire.write(0x6B);                     // PWR_MGMT_1 레지스터
  Wire.write(0);                        // 0 쓰면 슬립 해제 → 센서 동작 시작
  Wire.endTransmission(true);
}

void loop() {
  Wire.beginTransmission(MPU);
  Wire.write(0x3B);                     // 가속도 데이터 시작 레지스터 (ACCEL_XOUT_H)
  Wire.endTransmission(false);          // 연결 유지한 채로 읽기 준비
  Wire.requestFrom(MPU, 6, true);       // 6바이트 요청 (x, y, z 각각 상위+하위)

  ax = Wire.read() << 8 | Wire.read();  // 상위 바이트 << 8 | 하위 바이트 → 16비트 값
  ay = Wire.read() << 8 | Wire.read();
  az = Wire.read() << 8 | Wire.read();

  Serial.print(ax); Serial.print(",");  // "ax,ay,az" 한 줄로 전송
  Serial.print(ay); Serial.print(",");
  Serial.println(az);

  delay(20);                            // 약 50Hz로 전송
}
