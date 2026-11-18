# SensorAvoid — MPU6050 기울기 센서 게임 컨트롤러

MPU6050 + 아두이노 나노로 직접 만든 기울기 컨트롤러로 유니티 2D 피하기 게임을 조작한다.
센서를 좌우로 기울이면 플레이어(하늘색 사각형)가 움직이고, 위에서 떨어지는 빨간 장애물을 피한다.
닿으면 게임 오버, 점수는 생존 시간(초). **최고 점수 128점.**

## 기술 스택
| 분야 | 사용한 것 |
|---|---|
| 하드웨어 | Arduino Nano (CH340), MPU6050 가속도·자이로 센서, I2C |
| 펌웨어 | Arduino (C++), `Wire` 라이브러리, 115200bps 시리얼 |
| 게임 | Unity 6 (6000.6), C#, 2D 물리(Rigidbody2D·BoxCollider2D), `System.IO.Ports` |
| 자동화 | Unity 에디터 스크립트(씬 자동 생성), 배치모드 빌드, arduino-cli |
| 도구 | 생성형 AI(Claude)로 코드 작성·오류 분석 |

## 구조
```
MPU6050 ─I2C→ 아두이노 나노 ─USB 시리얼 115200bps, "ax,ay,az" 50Hz→ Unity
                                          SerialReceiver (별도 스레드): 문자열 → 기울기 -1~1
                                          PlayerMove: 기울기 → 좌우 이동 (데드존, 방향 반전)
                                          Spawner / Obstacle: 장애물 생성·낙하·충돌
                                          GameManager: 점수, 게임 오버, Restart
```

## 측정 결과 (약 45° 기울임, 60개 평균)
| 자세 | ax | ay | az |
|---|---|---|---|
| 평평 | -915 | 440 | 16660 |
| 오른쪽 | **-12477** | 1856 | 10206 |
| 왼쪽 | **+11049** | -143 | 12714 |

→ 좌우 축 = ax(`axisIndex = 0`), 오른쪽이 음수라 `invert = true`.
이론값 16384 × sin45° ≈ 11585와 비슷함.

## 폴더
```
arduino/mpu_read/mpu_read.ino      아두이노 펌웨어
unity/Assets/Scripts/              게임 스크립트 원본 (+ Editor/ 씬 자동 생성)
SensorAvoid/                       실제 유니티 프로젝트 (Unity 6)
docs/report.md                     탐구 보고서
docs/submission/                   학교 제출용 보고서 (docx, pdf)
```

## 실행
1. 배선: VCC→5V, GND→GND, SDA→A4, SCL→A5
2. 펌웨어 업로드
   ```
   arduino-cli compile --fqbn arduino:avr:nano arduino/mpu_read
   arduino-cli upload -p COM6 --fqbn arduino:avr:nano arduino/mpu_read
   ```
   (sync 에러 나면 `arduino:avr:nano:cpu=atmega328old`)
3. 유니티 Hub에서 `SensorAvoid/` 열기 → `Assets/Scenes/Game` → Play
   - 시작할 때 들고 있는 자세가 기준(0)이 됨. 처음 1초는 평평하게
   - 센서 없이도 ←→ / A D 키로 플레이 가능
4. 씬을 다시 만들려면 메뉴 **SensorAvoid > 게임 씬 자동 생성**

## 조정 (Inspector)
| 증상 | 할 것 |
|---|---|
| 안 움직임 | `Serial`의 Axis Index를 0 / 1 / 2로 바꿔보기 |
| 방향 반대 | `Player`의 Invert 체크 |
| 너무 둔함/예민함 | `Serial`의 Sensitivity (기본 1.5) |
| 포트 에러 | Console 확인 → Port Name에 `COM6`처럼 직접 입력, 시리얼 모니터 닫기 |
| 난이도 | 프리팹 `Obstacle`의 Speed(5), `GameManager`의 Spawner Interval(0.8) |
