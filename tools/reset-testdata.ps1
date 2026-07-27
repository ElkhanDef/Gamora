<#
.SYNOPSIS
    stats/ altındaki tüm .jsonl dosyalarını siler ve PC-01..04 için sahte,
    popülerlik sıralamasını test etmeye uygun dağıtılmış olaylar üretir.
    catalog.json ve covers/ klasörüne dokunmaz.

.PARAMETER DataRoot
    Gamora veri kökü. Varsayılan: C:\GamoraData (Faz 1 geliştirme yolu,
    settings.json'daki dataRoot ile aynı varsayılan).
#>
param(
    [string]$DataRoot = "C:\GamoraData"
)

function Write-Utf8NoBom {
    param([string]$Path, [string]$Content)
    [System.IO.File]::WriteAllText($Path, $Content, [System.Text.UTF8Encoding]::new($false))
}

$statsDir = Join-Path $DataRoot "stats"

if (Test-Path $statsDir) {
    Get-ChildItem -Path $statsDir -Filter "*.jsonl" -File | Remove-Item -Force
} else {
    New-Item -ItemType Directory -Path $statsDir -Force | Out-Null
}

# Dağılım: test-game-50 en popüler (6), test-game-10 ikinci (4), test-game-77
# üçüncü (3) — hero seçimi ve "ilk 10 popüler" rozeti anlamlı şekilde test edilsin
# diye 12 farklı oyuna yayıldı (ilk 10'a girmeyen 2 tanesi de var).
# PC-03.jsonl'deki son satır kasıtlı olarak yarım bırakıldı — PopularityService'in
# bozuk satırları sessizce atladığını doğrulamak için.

$pc01 = @'
{"gameId":"test-game-50","event":"launch","time":"2026-07-20T10:00:00"}
{"gameId":"test-game-50","event":"launch","time":"2026-07-21T11:00:00"}
{"gameId":"test-game-10","event":"launch","time":"2026-07-20T12:00:00"}
{"gameId":"test-game-3","event":"launch","time":"2026-07-20T13:00:00"}
{"gameId":"test-game-5","event":"launch","time":"2026-07-20T14:00:00"}
'@
Write-Utf8NoBom (Join-Path $statsDir "PC-01.jsonl") $pc01

$pc02 = @'
{"gameId":"test-game-50","event":"launch","time":"2026-07-22T09:00:00"}
{"gameId":"test-game-50","event":"launch","time":"2026-07-22T09:30:00"}
{"gameId":"test-game-10","event":"launch","time":"2026-07-22T10:00:00"}
{"gameId":"test-game-77","event":"launch","time":"2026-07-22T10:30:00"}
{"gameId":"test-game-6","event":"launch","time":"2026-07-22T11:00:00"}
{"gameId":"test-game-7","event":"launch","time":"2026-07-22T11:30:00"}
'@
Write-Utf8NoBom (Join-Path $statsDir "PC-02.jsonl") $pc02

$pc03 = @'
{"gameId":"test-game-50","event":"launch","time":"2026-07-23T08:00:00"}
{"gameId":"test-game-50","event":"launch","time":"2026-07-23T08:15:00"}
{"gameId":"test-game-10","event":"launch","time":"2026-07-23T09:00:00"}
{"gameId":"test-game-77","event":"launch","time":"2026-07-23T09:15:00"}
{"gameId":"test-game-77","event":"launch","time":"2026-07-23T09:30:00"}
{"gameId":"test-game-8","event":"launch","time":"2026-07-23T10:00:00"}
{"gameId":"test-game-9","event":"launch","time":"2026-07-23T10:15:00"}
{"gameId":"test-game-11","event":"lau
'@
Write-Utf8NoBom (Join-Path $statsDir "PC-03.jsonl") $pc03

$pc04 = @'
{"gameId":"test-game-10","event":"launch","time":"2026-07-24T08:00:00"}
{"gameId":"test-game-3","event":"launch","time":"2026-07-24T08:15:00"}
{"gameId":"test-game-12","event":"launch","time":"2026-07-24T08:30:00"}
{"gameId":"test-game-13","event":"launch","time":"2026-07-24T08:45:00"}
{"gameId":"test-game-14","event":"launch","time":"2026-07-24T09:00:00"}
'@
Write-Utf8NoBom (Join-Path $statsDir "PC-04.jsonl") $pc04

Write-Host "Sahte istatistik verisi yeniden üretildi: $statsDir"
Write-Host "Beklenen sıralama: test-game-50 (6), test-game-10 (4), test-game-77 (3), ..."
