# SensorAvoid — MPU6050 기울기 센서 게임 컨트롤러

MPU6050 + 아두이노 나노로 만든 컨트롤러로, 유니티 2D 피하기 게임을 조작한다.
센서를 좌우로 기울이면 플레이어가 움직이고, 떨어지는 장애물을 피한다.

## 폴더 구조
```
arduino/mpu_read/mpu_read.ino   아두이노 나노 업로드용
unity/Assets/Scripts/*.cs       유니티 프로젝트 Assets/Scripts에 복사
```

## A. 아두이노
1. 배선: VCC→5V, GND→GND, SDA→A4, SCL→A5
2. `mpu_read.ino` 업로드
3. 시리얼 모니터 **115200** 으로 열고 `ax,ay,az` 숫자 3개 나오는지 확인
4. 좌우로 기울일 때 크게 변하는 축 확인 (ax=0, ay=1, az=2)
5. 시리얼 모니터 닫기 (열려 있으면 유니티가 포트 못 엶)

## B. 유니티 세팅
1. 새 프로젝트: 2D, 이름 `SensorAvoid`
2. Project Settings > Player > Other Settings > Api Compatibility Level → **.NET Framework**
3. File > Build Settings > **Add Open Scenes** (안 하면 Restart 안 됨)
4. `Player`: Square, Position (0, -4, 0), Rigidbody 2D (Kinematic, **Use Full Kinematic Contacts 체크**), Box Collider 2D, Tag `Player`, `PlayerMove.cs`
   - 체크 항목이 없으면: Body Type Dynamic, Gravity Scale 0, Freeze Rotation Z
5. `Serial`: Create Empty, `SerialReceiver.cs`, Port Name을 실제 포트로
6. `Obstacle` 프리팹: Square(빨강), Rigidbody 2D (Kinematic), Box Collider 2D (Is Trigger), `Obstacle.cs` → 프리팹화 후 원본 삭제
7. `GameManager`: Create Empty, `GameManager.cs` + `Spawner.cs`, Obstacle Prefab 연결

## C. 테스트
- 안 움직이면 `SerialReceiver`의 Axis Index 0/1/2 바꿔보기
- 방향 반대면 `PlayerMove.cs`의 `t * speed` → `-t * speed`
- 난이도: `Obstacle.speed`, `Spawner.interval`
- 포트 에러는 Console에 `포트 열기 실패` 로 표시됨
