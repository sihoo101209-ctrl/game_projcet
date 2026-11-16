# SensorAvoid — MPU6050 기울기 센서 게임 컨트롤러

MPU6050 + 아두이노 나노로 만든 컨트롤러로, 유니티 2D 피하기 게임을 조작한다.
센서를 좌우로 기울이면 플레이어가 움직이고, 떨어지는 장애물을 피한다.

## 폴더 구조
```
arduino/mpu_read/mpu_read.ino           아두이노 나노 업로드용
unity/Assets/Scripts/*.cs               게임 스크립트
unity/Assets/Scripts/Editor/*.cs        씬 자동 생성 메뉴
```

## A. 아두이노
1. 배선: VCC→5V, GND→GND, SDA→A4, SCL→A5
2. `mpu_read.ino` 업로드 (보드: Arduino Nano, 업로드 실패하면 프로세서를 ATmega328P (Old Bootloader)로)
3. 시리얼 모니터 **115200**으로 열고 `ax,ay,az` 숫자 3개 나오는지 확인
4. 좌우로 기울일 때 크게 변하는 축 확인 (ax=0, ay=1, az=2)
5. 시리얼 모니터 닫기 (열려 있으면 유니티가 포트 못 엶)

## B. 유니티 (자동)
1. Unity Hub에서 새 프로젝트: 2D, 이름 `SensorAvoid` (Unity 2021.2 이상 권장)
2. 이 레포의 `unity/Assets/Scripts` 폴더를 프로젝트의 `Assets` 안에 통째로 복사
   - 복사하면 Api Compatibility Level이 자동으로 .NET Framework로 바뀜
3. 위쪽 메뉴 **SensorAvoid > 게임 씬 자동 생성** 클릭
   - Player, Serial, GameManager, Obstacle 프리팹, Build Settings 등록까지 다 해줌
4. Play

## C. 테스트·조정 (전부 Inspector에서)
| 증상 | 할 것 |
|---|---|
| 안 움직임 | `Serial` 오브젝트의 Axis Index를 0 / 1 / 2로 바꿔보기 |
| 방향 반대 | `Player`의 Invert 체크 |
| 너무 둔함/예민함 | `Serial`의 Sensitivity 조절 (기본 1.5) |
| 포트 에러 | Console 메시지 확인 → Port Name에 `COM3`처럼 직접 입력 |
| 난이도 | 프리팹 `Obstacle`의 Speed, `GameManager`의 Spawner Interval |

- 센서 없이도 ←→ / A D 키로 플레이 가능 (테스트·시연용)
- 시작할 때 들고 있는 자세가 기준(0)이 됨. 게임 시작 1초 동안은 평평하게 들고 있기
