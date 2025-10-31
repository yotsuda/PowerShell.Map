# PowerShell.Map - Cmdlet Demo Script
# 一貫性のある設計に基づいた新しいデモ

Import-Module .\PowerShell.Map\bin\Release\net9.0\PowerShell.Map.dll -Force

Write-Host "`n=== PowerShell.Map - Cmdlet Demo ===" -ForegroundColor Cyan
Write-Host "すべてのCmdletが 'OpenStreetMap' プレフィックスを持ち、パラメータも統一されています`n" -ForegroundColor White

# ========================================
# Demo 1: Show-OpenStreetMap の基本機能
# ========================================
Write-Host "Demo 1: Show-OpenStreetMap - 基本表示" -ForegroundColor Yellow
Write-Host "  単一地点の表示" -ForegroundColor Gray
Show-OpenStreetMap "Tokyo Tower" -Zoom 15 -Animate -Duration 2.0
Start-Sleep -Seconds 3

# ========================================
# Demo 2: ズーム変更のみ
# ========================================
Write-Host "`nDemo 2: Show-OpenStreetMap - ズーム変更のみ" -ForegroundColor Yellow
Write-Host "  Locationを指定せずにズームのみ変更（現在位置を維持）" -ForegroundColor Gray
Show-OpenStreetMap -Zoom 10 -Animate -Duration 1.5
Start-Sleep -Seconds 2
Show-OpenStreetMap -Zoom 17 -Animate -Duration 2.0
Start-Sleep -Seconds 3

# ========================================
# Demo 3: 位置とズームの両方を変更
# ========================================
Write-Host "`nDemo 3: Show-OpenStreetMap - 位置 + ズーム変更" -ForegroundColor Yellow
Write-Host "  Shibuya → Shinjuku → Harajuku" -ForegroundColor Gray
Show-OpenStreetMap "Shibuya, Tokyo" -Marker "🏙️ Shibuya" -Zoom 14 -Animate -Duration 1.5
Start-Sleep -Seconds 2
Show-OpenStreetMap "Shinjuku, Tokyo" -Marker "🌃 Shinjuku" -Zoom 15 -Animate -Duration 1.5
Start-Sleep -Seconds 2
Show-OpenStreetMap "Harajuku, Tokyo" -Marker "🎌 Harajuku" -Zoom 16 -Animate -Duration 1.5
Start-Sleep -Seconds 3

# ========================================
# Demo 4: 複数マーカー表示
# ========================================
Write-Host "`nDemo 4: Show-OpenStreetMap - 複数マーカー" -ForegroundColor Yellow
Write-Host "  複数の地点を同時に表示" -ForegroundColor Gray
$markers = @(
    @{ Location = "Tokyo Tower"; Label = "🗼 Tokyo Tower"; Color = "red" }
    @{ Location = "Tokyo Skytree"; Label = "🏗️ Skytree"; Color = "blue" }
    @{ Location = "Sensoji Temple, Tokyo"; Label = "⛩️ Sensoji"; Color = "orange" }
    @{ Location = "Meiji Shrine, Tokyo"; Label = "🌳 Meiji Shrine"; Color = "green" }
)
Show-OpenStreetMap -Markers $markers -Zoom 11 -Animate -Duration 1.5
Start-Sleep -Seconds 3

# ========================================
# Demo 5: ルート表示
# ========================================
Write-Host "`nDemo 5: Show-OpenStreetMapRoute - ルート表示" -ForegroundColor Yellow
Write-Host "  Tokyo Station → Tokyo Skytree" -ForegroundColor Gray
Show-OpenStreetMapRoute "Tokyo Station" "Tokyo Skytree" `
    -Animate -Duration 2.0 -Color "#ff6600" -Width 6 -Zoom 12
Start-Sleep -Seconds 3

# ========================================
# Demo 6: ツアー機能
# ========================================
Write-Host "`nDemo 6: Invoke-OpenStreetMapTour - 地図ツアー" -ForegroundColor Yellow
Write-Host "  日本の主要都市を巡回（Alias: Start-MapTour）" -ForegroundColor Gray
$cities = @("Tokyo", "Osaka", "Kyoto", "Fukuoka", "Sapporo")
Invoke-OpenStreetMapTour $cities -Zoom 12 -PauseTime 2.0 -Duration 1.5

# または Alias を使用
# Start-MapTour $cities -Zoom 12 -PauseTime 2.0 -Duration 1.5

Write-Host "`n=== Demo 完了 ===" -ForegroundColor Green
Write-Host "すべてのCmdletが一貫した設計に従っています!`n" -ForegroundColor White

# ========================================
# 使用例のまとめ
# ========================================
Write-Host "=== 主な使用パターン ===" -ForegroundColor Cyan

Write-Host "`n1. Show-OpenStreetMap の柔軟な使い方:" -ForegroundColor Yellow
Write-Host '   Show-OpenStreetMap "Tokyo"                  # 地点表示' -ForegroundColor Gray
Write-Host '   Show-OpenStreetMap -Zoom 15                 # ズームのみ' -ForegroundColor Gray
Write-Host '   Show-OpenStreetMap "Tokyo" -Zoom 15         # 両方指定' -ForegroundColor Gray
Write-Host '   Show-OpenStreetMap "Tokyo" -Animate         # アニメーション付き' -ForegroundColor Gray

Write-Host "`n2. パイプライン対応:" -ForegroundColor Yellow
Write-Host '   Import-Csv data.csv | Show-OpenStreetMap' -ForegroundColor Gray
Write-Host '   "Tokyo", "Osaka" | Invoke-OpenStreetMapTour' -ForegroundColor Gray

Write-Host "`n3. 共通パラメータ:" -ForegroundColor Yellow
Write-Host '   -Zoom <int>       : ズームレベル [1-19]' -ForegroundColor Gray
Write-Host '   -Animate          : アニメーション有効化' -ForegroundColor Gray
Write-Host '   -Duration <double>: アニメーション時間 [0.1-10.0秒]' -ForegroundColor Gray
Write-Host '   -DebugMode        : デバッグ情報表示' -ForegroundColor Gray
Write-Host ""
