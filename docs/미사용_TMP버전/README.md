# 미사용 — TMP 버전 타이틀 스크립트

`SessionStartUI.cs`(제공 원본)와 `VersionLabel.cs`는 TextMeshPro 에 의존한다.
TMP Essential Resources 를 Import 하지 않으면 텍스트가 안 보이거나 분홍색으로 깨지므로,
현재 프로젝트는 TMP 없이 동작하는 `Assets/Scripts/TitleScreen.cs`(같은 검증 규칙, 코드 생성 UI)를 쓴다.

이 폴더는 `Assets/` 밖이라 Unity 가 컴파일하지 않는다. 나중에 TMP 로 타이틀을 직접 꾸미고 싶을 때만 옮겨서 쓸 것.
