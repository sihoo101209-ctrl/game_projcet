# CLAUDE.md — SensorAvoid 프로젝트 인수인계

이 파일은 이전 Claude Code 클라우드 세션이 정리한 것. 그 세션은 사용자 PC에 접근할 수 없어서
아두이노 업로드·유니티 실행·실물 테스트를 못 했음. **이 세션(사용자 PC에서 실행)이 그 나머지를 끝까지 하는 게 목표.**

---

## 0. 가장 먼저 알아둘 것

- **마감: 2026-11-18(수) 오늘.** 학교 제출용이자 포트폴리오용. 속도가 최우선.
- 사용자 요청: "네가 다 해줘". 사용자는 직접 손대는 걸 최소로 하고 싶어함.
  → 명령 실행·파일 수정·빌드는 전부 Claude가 직접 하고, 사용자에겐 **물리적으로 사람만 할 수 있는 것**
  (센서 들고 기울이기, USB 꽂기, 사진·영상 찍기, 느낀 점 쓰기)만 요청할 것.
- 사용자 말투 선호: **반말, 짧고 직접적으로.** 코드 보여줄 땐 파일별로 짧게 나누고 줄별 설명.
  사용자는 영어 자판 상태로 한글을 칠 때가 있음 (예: `rmsid sprk gownj` = "그냥 네가 해줘"). 두벌식으로 풀어 읽을 것.
- **보고서에 결과를 지어내지 말 것.** 실제 테스트 값(축, 감도, 최고 점수 등)은 실제로 측정한 것만 쓴다.
- git: 작업물은 브랜치 `ccr-7dba1805-em8opj`에 있음 (main에는 아직 없음).
  이 파일이 보인다면 이미 그 브랜치를 체크아웃한 상태. 커밋은 이 브랜치에 하고, 푸시 전엔 사용자 확인.

---

## 1. 프로젝트 개요

- MPU6050 가속도/자이로 센서 + 아두이노 나노로 만든 **기울기 게임 컨트롤러**
- 유니티 2D **피하기 게임** (비행 슈팅에서 슈팅을 뺀 버전): 센서를 좌우로 기울이면 플레이어(하늘색 사각형)가
  좌우로 움직이고, 위에서 떨어지는 빨간 사각형 장애물을 피함. 닿으면 게임 오버, Restart 버튼으로 재시작.
  점수 = 생존 시간(초).
- 7월 대구한의대 '생성형 AI를 활용한 게임개발자 체험하기' 캠퍼스 탐구 보고서의 주제로 쓰임.
- 진행 상태(이 파일 작성 시점):
  - [x] 부품 도착, 조립(배선) 완료 — 사용자 말 기준
  - [x] 코드 전부 작성 (아두이노 1개, 유니티 6개 + 에디터 자동화 1개)
  - [x] C# 컴파일 검증 (Unity 2021.1 DLL 기준, 아래 7번 참고)
  - [x] 보고서 초안 `docs/report.md` (실측 결과 칸은 `[ ]`로 비어 있음)
  - [ ] 아두이노 업로드 / 시리얼 값 확인
  - [ ] 유니티 프로젝트 생성 / 씬 생성 / 실제 플레이 테스트
  - [ ] 축·방향·감도·난이도 조정
  - [ ] 보고서 실측 결과 채우기, 사용자 기존 초안과 합치기
  - [ ] 포트폴리오용 결과물(영상, 사진, 코드) 정리
  - [ ] 학교 제출

---

## 2. 레포 구조

```
CLAUDE.md                                   이 파일
README.md                                   사람용 짧은 설명서 (A~C 단계)
arduino/mpu_read/mpu_read.ino               아두이노 나노 펌웨어 (폴더명=파일명이어야 IDE에서 열림)
unity/Assets/Scripts/SerialReceiver.cs      시리얼 수신 → 기울기(-1~1)
unity/Assets/Scripts/PlayerMove.cs          플레이어 이동 (센서 + 키보드)
unity/Assets/Scripts/Obstacle.cs            장애물 낙하, 충돌 시 게임 오버
unity/Assets/Scripts/Spawner.cs             장애물 생성
unity/Assets/Scripts/GameManager.cs         점수, 게임 오버, Restart (OnGUI)
unity/Assets/Scripts/Editor/SensorAvoidSetup.cs   씬 자동 생성 (메뉴 + 명령줄)
docs/report.md                              탐구 보고서 초안
```

`unity/` 폴더는 **유니티 프로젝트 전체가 아님.** `unity/Assets/Scripts`를 실제 유니티 프로젝트의 `Assets/` 안에
복사해서 쓰는 구조. (실제 유니티 프로젝트는 레포 밖에 만들어도 되고, 포트폴리오용으로 레포 안 `SensorAvoid/`에
만들어도 됨 — 레포에 넣을 거면 `Library/ Temp/ Logs/ obj/ UserSettings/ *.csproj *.sln`을 .gitignore에 추가할 것.)

---

## 3. 하드웨어

| MPU6050 | 아두이노 나노 |
|---|---|
| VCC | 5V |
| GND | GND |
| SDA | A4 |
| SCL | A5 |

- I2C 주소 0x68 (AD0 핀이 GND/미연결일 때). 0x69면 AD0이 HIGH인 것.
- 가속도 범위 기본 ±2g → **1g = 16384**.
- 나노 클론은 USB 칩이 CH340인 경우가 많음 → 윈도우에서 포트가 안 보이면 CH340 드라이버 필요.
- 나노 클론은 부트로더가 구형인 경우가 많음 → 업로드가 `stk500_getsync()`/`not in sync`로 실패하면
  프로세서를 **ATmega328P (Old Bootloader)**로 (FQBN `arduino:avr:nano:cpu=atmega328old`).

---

## 4. 코드 설명과 설계 결정

사용자가 처음 준 코드에서 아래를 바꿨음. 이유까지 알고 있어야 다시 망가뜨리지 않음.

### 4-1. `mpu_read.ino`
- 0x6B(PWR_MGMT_1)에 0 써서 슬립 해제 → 0x3B부터 6바이트 읽어 ax, ay, az.
- `"ax,ay,az\n"` 형식으로 20ms마다(약 50Hz) 전송.
- **변경: 9600 → 115200bps.** 한 줄 최대 약 22바이트 × 50Hz = 1100B/s로 9600bps(약 960B/s) 한계를 넘어
  반응이 밀림. 유니티 `SerialReceiver.baudRate`도 115200. 시리얼 모니터도 115200으로 열어야 함.

### 4-2. `SerialReceiver.cs` (Serial 오브젝트)
Inspector 필드: `portName="auto"`, `baudRate=115200`, `axisIndex=1`, `sensitivity=1.5`. 정적 `tilt`(-1~1)를 다른 스크립트가 읽음.
- **별도 스레드에서 `ReadLine`.** 원래는 `Update`에서 `ReadLine`(timeout 20ms)을 불러서 데이터 없는 프레임마다
  최대 20ms씩 멈춰 끊겼음. 스레드가 `latest`를 쓰고 `Update`가 `tilt = latest`로 옮김.
- **싱글톤 + `DontDestroyOnLoad`.** 원래 코드는 Restart(씬 재로드) 때 포트를 안 닫아서 새 오브젝트가 포트를 못 열고
  ("Access denied"), 다시 열 수 있다 해도 DTR 때문에 나노가 리셋돼 1~2초 입력이 끊김. 지금은 처음 오브젝트만 살아남고
  포트도 한 번만 엶. `OnDestroy`(앱 종료 시)에서 닫음.
- **포트 자동 선택 (`"auto"`)**: `usb` 들어간 이름 우선(맥), 아니면 번호가 가장 큰 COM(윈도우, 숫자 기준 정렬).
  COM1 같은 내장 포트만 있거나 틀리면 Inspector에서 `COM5`처럼 직접 입력.
- **시작 자세 보정**: 처음 받은 20줄(약 0.4초) 평균을 offset으로 빼서 쥔 자세를 0으로. 시작할 때 평평하게 들고 있어야 함.
- `tilt = Clamp((raw/16384 - offset) * sensitivity, -1, 1)`. sensitivity 1.5면 약 42° 기울이면 최대 속도.
- 깨진 줄(`p.Length < 3`)·타임아웃·파싱 오류는 무시.
- **`#if` 가드**: Api Compatibility Level이 .NET Standard면 `System.IO.Ports`가 없어 컴파일 에러 → 그러면 에디터
  스크립트까지 같이 컴파일이 안 돼서 자동 설정 메뉴도 안 뜨는 닭과 달걀 문제가 생김. 그래서
  `NET_STANDARD / NET_STANDARD_2_0 / NET_STANDARD_2_1`이 정의돼 있으면 시리얼 코드를 빼고 경고만 띄우게 함.
  에디터 스크립트가 Api Level을 .NET Framework로 바꾸면 재컴파일되면서 시리얼 코드가 켜짐.

### 4-3. `PlayerMove.cs` (Player 오브젝트)
필드: `speed=8`, `limitX=8`, `deadZone=0.1`, `invert=false`.
- 데드존 아래는 0 (손 떨림 무시). `invert` 체크하면 방향 반전 (원래 가이드의 `-t * speed` 수정 대신).
- **키보드 대체 입력**: ←→ / A D. 키 입력이 있으면 센서보다 우선. 센서 없이 테스트·시연 가능.
  `ENABLE_LEGACY_INPUT_MANAGER`면 `Input.GetAxisRaw`, `ENABLE_INPUT_SYSTEM`만 있으면 `Keyboard.current`.
  (Unity 6 신규 프로젝트는 Input System만 켜져 있을 수 있어서 `Input.GetAxis`를 바로 쓰면 예외남.)
- 게임 오버면 멈춤.

### 4-4. `Obstacle.cs` (Obstacle 프리팹)
`speed=5`, 아래로 이동, y < -7이면 삭제, `OnTriggerEnter2D`에서 Player 태그면 `GameManager.instance.GameOver()`.

### 4-5. `Spawner.cs` (GameManager 오브젝트)
`interval=0.8`, `rangeX=8`, y=6 위치에 랜덤 x로 프리팹 생성. 게임 오버면 멈춤.

### 4-6. `GameManager.cs` (GameManager 오브젝트)
정적 `instance`, 정적 `isOver`(Awake에서 false로 초기화). OnGUI로 점수 표시, 게임 오버 시 Restart 버튼.
- **변경: Restart를 `LoadScene(name)`으로.** 씬이 Build Settings에 없으면 재로드 실패 → 자동 설정이 등록해줌.

### 4-7. `Editor/SensorAvoidSetup.cs` — 씬 자동 생성
- `[InitializeOnLoad]`: 스크립트가 들어가자마자 Standalone의 Api Compatibility Level을 .NET Framework로 바꿈
  (2021.2+: `NamedBuildTarget.Standalone` + `ApiCompatibilityLevel.NET_Unity_4_8`, 이전 버전: `BuildTargetGroup.Standalone` + `NET_4_6`).
- 메뉴 **SensorAvoid > 게임 씬 자동 생성** (`Build()`), 명령줄용 **`SensorAvoidSetup.BuildBatch`**.
  - `BuildBatch`는 `Assets/Scenes/SampleScene.unity`가 있으면 열고, 없으면 빈 씬을 만듦.
  - 카메라가 없으면 직교 카메라(size 5, z=-10, 어두운 배경) 생성.
  - `Assets/Sprites/Square.png` (4x4 흰색, PPU 4 → 1유닛) 생성
  - `Assets/Prefabs/Obstacle.prefab`: 빨강, BoxCollider2D(Is Trigger), Rigidbody2D **Kinematic**, NeverSleep, Obstacle
  - `Player`: (0,-4,0), 하늘색, BoxCollider2D, Rigidbody2D **Dynamic + Gravity 0 + Freeze Rotation**, NeverSleep, Tag Player, PlayerMove
    - **왜 Dynamic?** Box2D 기반 2D 물리는 기본 설정에서 Kinematic끼리 접촉(트리거 포함)을 처리 안 할 수 있음
      (`Use Full Kinematic Contacts`가 꺼져 있으면). 원래 가이드는 둘 다 Kinematic이라 게임 오버가 안 날 위험이 있었음.
  - `Serial`: SerialReceiver / `GameManager`: GameManager + Spawner(obstaclePrefab 연결)
  - 다시 실행하면 Player/Serial/GameManager를 지우고 새로 만듦 (멱등)
  - 씬 저장 (경로 없으면 `Assets/Scenes/Game.unity`) + Build Settings 맨 앞에 등록

---

## 5. 이 세션이 할 일 (순서대로)

먼저 OS 확인 (`uname`이나 `$env:OS`). 사용자는 아마 윈도우. 아래 명령은 윈도우(PowerShell) 기준, 맥이면 경로만 바꿀 것.

### 5-1. 아두이노 업로드
1. 사용자에게 나노 USB 꽂았는지 확인. 아두이노 IDE의 시리얼 모니터가 열려 있으면 닫으라고 함 (포트 점유).
2. `arduino-cli`가 있는지 확인. 없으면 설치 (`winget install ArduinoSA.CLI` 또는 공식 zip).
   아두이노 IDE 2.x를 깔았으면 IDE에 arduino-cli가 내장돼 있지만 PATH에 없으니 따로 깔아도 됨.
3. ```
   arduino-cli core update-index
   arduino-cli core install arduino:avr
   arduino-cli board list                       # 포트 확인 (예: COM5)
   arduino-cli compile --fqbn arduino:avr:nano arduino/mpu_read
   arduino-cli upload  -p COM5 --fqbn arduino:avr:nano arduino/mpu_read
   ```
   업로드 실패(sync 에러)하면 `--fqbn arduino:avr:nano:cpu=atmega328old`로 재시도.
   포트가 아예 안 보이면 CH340 드라이버 문제일 가능성 → 사용자에게 알림.

### 5-2. 시리얼 값 확인 + 좌우 축 찾기
- Python 있으면 `pip install pyserial` 후 짧은 스크립트로 115200에서 몇 초 읽기. 없으면 PowerShell:
  ```powershell
  $p = New-Object System.IO.Ports.SerialPort COM5,115200; $p.ReadTimeout=1000; $p.Open()
  1..100 | % { $p.ReadLine() }; $p.Close()
  ```
- 정상이면 `숫자,숫자,숫자` 줄이 나오고, 평평할 때 한 축(보통 az)이 약 ±16384.
- 전부 0이거나 -1이거나 안 나오면: 배선(SDA/SCL 바뀜, VCC/GND) 의심 → 사용자에게 배선 확인 요청.
- **사용자에게 "센서를 오른쪽으로 쭉 기울이고 있어" → 읽기 → "왼쪽으로" → 읽기**를 요청해서
  가장 크게 (부호 반대로) 변하는 축을 찾음. 그 인덱스가 `axisIndex` (ax=0, ay=1, az=2).
  오른쪽으로 기울였을 때 값이 **음수**가 되는 축이면 `invert = true`.
- 확인 끝나면 **반드시 포트 닫기.**

### 5-3. 유니티 프로젝트 만들기
- 설치된 에디터 찾기: `C:\Program Files\Unity\Hub\Editor\<버전>\Editor\Unity.exe` (맥: `/Applications/Unity/Hub/Editor/<버전>/Unity.app/Contents/MacOS/Unity`).
  없으면 사용자에게 Unity Hub로 에디터 설치 요청 (2021.2 이상, 가능하면 2022 LTS나 Unity 6).
- 방법 A (사용자가 Hub에서 2D 템플릿으로 만들기, 가장 안전): 프로젝트명 `SensorAvoid`.
- 방법 B (명령줄): `Unity.exe -batchmode -createProject <경로>\SensorAvoid -quit -logFile -`
  → 템플릿 선택이 안 돼서 3D Built-in 프로젝트가 됨. 그래도 스프라이트 + 직교 카메라로 2D 게임은 문제없이 돌아감.
- `unity/Assets/Scripts` 폴더를 프로젝트의 `Assets/Scripts`로 복사.
- **유니티 에디터가 그 프로젝트를 열고 있으면 배치모드 실행 불가** → 에디터 닫고 할 것.
- 씬 생성:
  ```
  Unity.exe -batchmode -projectPath <경로>\SensorAvoid -executeMethod SensorAvoidSetup.BuildBatch -quit -logFile build.log
  ```
  로그에서 `[SensorAvoid] 씬 생성 완료`와 `error CS` 유무 확인. 에러 있으면 고치고 다시.
  (또는 에디터에서 메뉴 **SensorAvoid > 게임 씬 자동 생성** 클릭.)
- 5-2에서 찾은 `axisIndex`/`invert`는 씬의 컴포넌트 값으로 넣거나, 간단히 스크립트 기본값
  (`SerialReceiver.axisIndex`, `PlayerMove.invert`)을 바꾸고 BuildBatch를 다시 돌리면 됨
  (BuildBatch가 오브젝트를 새로 만들기 때문에 기본값이 들어감). 레포 쪽 스크립트에도 같이 반영.

### 5-4. 플레이 테스트 (사용자가 함)
- 사용자에게 에디터로 프로젝트 열고 Play 눌러달라고 함. 시작 1초는 센서를 평평하게.
- 체크리스트: 좌우로 움직이나 / 방향 맞나 / 너무 둔하거나 예민하지 않나 / 장애물에 닿으면 게임 오버 되나 /
  Restart 되나 / Restart 후에도 센서가 계속 되나 / 점수가 올라가나.
- Console 에러는 사용자에게 복사해서 붙여달라고 함. 또는 `%LOCALAPPDATA%\Unity\Editor\Editor.log`를 직접 읽어도 됨.
- 조정 기준: 너무 어려우면 Obstacle speed↓(5→4) 또는 interval↑(0.8→1.0), 쉬우면 반대.
- 선택(시간 남으면): 빌드해서 exe 만들기 (File > Build Settings > Build). 시연·포트폴리오에 좋음.
  배치모드 빌드는 `-buildWindows64Player <경로>\SensorAvoid.exe`.

### 5-5. 보고서 마무리 (`docs/report.md`)
- 사용자가 **이미 써둔 초안이 따로 있음** (7월 대구한의대 캠프 탐구 보고서). 그걸 달라고 해서 이 초안과 합칠 것.
  파일 형식(hwp/docx/구글독스)도 물어볼 것. 학교 제출 양식이 있으면 그 양식에 맞춤.
- `[ ]` 칸 채우기: 좌우 축, 방향 반전 여부, 최종 감도/속도/간격, 최고 점수, 사진·영상, 실제 겪은 문제, 느낀 점,
  제작 계획서와 비교 (계획서도 사용자에게 받아야 함).
- 7번 "제작 과정에서 고려한 점"은 설계 때 반영한 내용이고, 실제 테스트에서 겪은 문제가 있으면 그걸 우선해서 추가.
- 지어내지 말 것. 측정 안 한 값은 사용자에게 묻거나 빈칸으로 둠. 느낀 점은 사용자 말을 다듬기만.

### 5-6. 포트폴리오 정리
- 사용자에게 받을 것: 회로 사진, 컨트롤러 실물 사진, 플레이 영상(센서 기울이는 손 + 화면이 같이 나오게, 30초~1분).
- 레포에 `docs/images/` 만들고 사진 넣고 README 상단에 데모 이미지·영상 링크, 기술 스택, 구조도 추가.
- 코드는 이미 정리돼 있음.

### 5-7. 제출
- 사용자가 학교에 제출. Claude는 제출 파일 형식 변환(docx/pdf 등)만 도와줌.

---

## 6. 문제 해결 빠른 표

| 증상 | 원인 / 해결 |
|---|---|
| `System.IO.Ports` 없음 컴파일 에러 | Api Level이 .NET Standard. 원래 `#if`로 막혀 있어야 함. Project Settings > Player > Other Settings > Api Compatibility Level → .NET Framework |
| Console `포트 열기 실패` / Access denied | 시리얼 모니터나 다른 프로그램이 포트 점유, 또는 포트 이름 틀림 → `Serial.portName`에 직접 입력 |
| Console `시리얼 포트가 없어` | USB 안 꽂힘 / 드라이버(CH340) |
| 센서 꺼짐 경고 (`Api Compatibility Level이 .NET Framework가 아니라`) | Api Level 변경 후 재컴파일 안 됨 → 에디터 재시작 |
| 아예 안 움직임 | axisIndex 틀림(0/1/2 바꿔보기), 또는 시리얼 값이 안 들어옴(5-2로 확인) |
| 반대로 움직임 | `Player.invert` 체크 |
| 가만히 있어도 흘러감 | 시작할 때 기울인 상태로 보정됨 → 평평하게 들고 Play 다시, 또는 deadZone↑ |
| 장애물에 닿아도 게임 오버 안 됨 | Player Rigidbody2D가 Dynamic인지, Obstacle 콜라이더 Is Trigger인지, Player 태그인지 확인 |
| Restart 버튼 눌러도 안 됨 | 씬이 Build Settings에 없음 → 자동 설정 다시 실행 |
| 사각형이 안 보임 (URP 2D 템플릿) | 씬에 Global Light 2D가 없으면 Lit 스프라이트가 어둡게 나올 수 있음 → 2D 템플릿 SampleScene에서 실행했는지 확인, 아니면 Light 2D(Global) 추가 |
| 업로드 `not in sync` | `cpu=atmega328old` |

---

## 7. 검증 상태 (중요)

- 실행 검증: **없음.** 유니티 에디터·아두이노 실물로 돌려본 적 없음.
- 컴파일 검증: 클라우드에서 NuGet `Unity3D.SDK 2021.1.14.1`의 UnityEngine.dll/UnityEditor.dll + .NET Framework 4.8 참조로
  전체 스크립트를 컴파일 → 통과. 두 경우 다 확인함:
  `NET_4_6 + ENABLE_LEGACY_INPUT_MANAGER` / `NET_STANDARD_2_1` (시리얼 꺼진 분기).
- **검증 안 된 부분:**
  - `UNITY_2021_2_OR_NEWER` 분기 (`NamedBuildTarget`, `ApiCompatibilityLevel.NET_Unity_4_8`) — 2021.1 DLL엔 없음.
    공식 API로 알고 있지만 실제 에디터에서 첫 컴파일 때 확인할 것.
  - `ENABLE_INPUT_SYSTEM` 분기 (`UnityEngine.InputSystem.Keyboard`) — Input System 패키지 DLL이 없어서 미확인.
  - `SerialPort.GetPortNames()` 자동 선택 결과, 실제 시리얼 수신, 2D 트리거 판정, URP 2D에서의 스프라이트 표시.
  - 아두이노 스케치는 원래 사용자 코드에서 baud만 바꾼 것, 컴파일 안 해봄 (단순해서 위험 낮음).

---

## 8. 사용자가 처음 준 계획 (원본 체크리스트, 참고용)

A. 아두이노: 배선 → `mpu_read.ino` 업로드 → 시리얼 모니터(원래 9600, 지금은 115200)에서 `ax,ay,az` 확인 →
좌우로 기울일 때 크게 변하는 축 확인 → 시리얼 모니터 닫기.

B. 유니티: 2D 프로젝트 `SensorAvoid` → Api Compatibility Level .NET Framework → Player(Square, (0,-4,0),
Rigidbody2D Kinematic, BoxCollider2D, Tag Player, PlayerMove) → Serial(SerialReceiver, 포트 설정) →
Obstacle 프리팹(빨강, Kinematic, Trigger, Obstacle) → GameManager(GameManager + Spawner, 프리팹 연결).
**지금은 이 B 전체를 `SensorAvoidSetup`이 자동으로 함** (Player만 Kinematic → Dynamic으로 바뀜, 이유는 4-7).

C. 테스트: 움직임 확인 → 축 바꾸기 → 방향 반전 → 난이도(Obstacle speed, Spawner interval) → 게임 오버/Restart → 점수.

D. 보고서·제출: 보고서 마무리 → 계획서와 결과 비교 → 11/18 제출 → 포트폴리오(영상, 사진, 코드) 정리.
