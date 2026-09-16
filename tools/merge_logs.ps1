# 여러 노트북에서 회수한 logs 폴더의 CSV 를 하나로 합친다.
#
# 사용법:
#   1. USB 에서 회수한 logs 폴더들을 한 폴더 아래에 모은다 (폴더 이름은 자유):
#        수집\노트북1_A\logs\summary.csv, deaths.csv
#        수집\노트북2_B\logs\summary.csv, deaths.csv
#   2. merge_logs.bat 와 이 파일을 "수집" 폴더에 복사하고 merge_logs.bat 를 더블클릭
#   3. 수집\합본_summary.csv, 수집\합본_deaths.csv 가 생긴다
#      - 첫 열 "출처" = 어느 폴더에서 온 행인지 (예: 노트북1_A)
#      - 기기번호 0 / 번호 0 행은 에디터 테스트 기록이므로 분석에서 제외할 것
#
# PowerShell 에서 직접: powershell -ExecutionPolicy Bypass -File merge_logs.ps1 -Root "C:\경로\수집"

param([string]$Root = (Get-Location).Path)
$ErrorActionPreference = 'Stop'
$Root = (Resolve-Path $Root).Path

function Merge-Csv([string]$name, [string]$outName) {
    $files = Get-ChildItem -Path $Root -Recurse -File | Where-Object { $_.Name -ieq $name }
    if (-not $files) { Write-Host "  $name 파일 없음"; return }

    $out = New-Object System.Collections.Generic.List[string]
    $header = $null
    $rows = 0
    foreach ($f in $files) {
        $lines = [System.IO.File]::ReadAllLines($f.FullName, [System.Text.Encoding]::UTF8)
        if ($lines.Count -eq 0) { continue }
        if ($null -eq $header) {
            $header = $lines[0]
            $out.Add("출처," + $header)
        } elseif ($lines[0] -ne $header) {
            Write-Warning "헤더가 달라 건너뜀: $($f.FullName)"
            continue
        }
        $rel = $f.DirectoryName.Substring($Root.Length).TrimStart('\')
        $src = if ($rel -eq '') { '.' } else { $rel.Split('\')[0] }
        $n = 0
        for ($i = 1; $i -lt $lines.Count; $i++) {
            if ($lines[$i].Trim() -ne '') { $out.Add("$src," + $lines[$i]); $n++ }
        }
        $rows += $n
        Write-Host ("  + {0}  ({1}행)" -f $f.FullName.Substring($Root.Length).TrimStart('\'), $n)
    }
    $outPath = Join-Path $Root $outName
    [System.IO.File]::WriteAllLines($outPath, $out, (New-Object System.Text.UTF8Encoding $true))
    Write-Host ("→ {0}  (총 {1}행)" -f $outName, $rows)
}

Write-Host "루트: $Root"
Merge-Csv 'summary.csv' '합본_summary.csv'
Merge-Csv 'deaths.csv'  '합본_deaths.csv'
Write-Host "완료"
