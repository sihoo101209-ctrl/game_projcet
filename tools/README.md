# tools

## merge_logs — 로그 CSV 합치기

노트북마다 쌓인 `logs\summary.csv`, `logs\deaths.csv` 를 하나로 합친다. Windows 기본 PowerShell 만 있으면 됨 (설치 불필요).

1. USB 에서 회수한 `logs` 폴더들을 한 폴더 아래에 모은다. 폴더 이름이 곧 "출처" 열이 된다:
   ```
   수집\
   ├── 노트북1_A\logs\summary.csv, deaths.csv
   ├── 노트북2_B\logs\summary.csv, deaths.csv
   └── ...
   ```
2. `merge_logs.bat` 와 `merge_logs.ps1` 두 파일을 `수집\` 에 복사
3. `merge_logs.bat` 더블클릭 → `합본_summary.csv`, `합본_deaths.csv` 생성 (Excel 에서 바로 열림)

- 여러 번 실행해도 합본 파일은 덮어쓴다 (합본 파일 자체는 합치는 대상에서 제외)
- **기기번호 0 / 번호 0 행은 에디터 테스트 기록** — 분석 전에 제외
- 합본 CSV 도 `.gitignore` 로 막혀 있어 git 에 안 올라간다
