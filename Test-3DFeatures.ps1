# 3D機能テストスクリプト
Write-Host "`n=== PowerShell.Map - 3D機能テスト ===" -ForegroundColor Cyan

# Test 1: 基本的な3D表示
Write-Host "`nTest 1: 富士山を3D表示" -ForegroundColor Yellow
Show-OpenStreetMap "Mount Fuji" -Enable3D -Zoom 13 -Pitch 65 -Bearing 90
Start-Sleep -Seconds 3

# Test 2: 3Dトグル（3D→2D→3D）
Write-Host "`nTest 2: 3Dトグルテスト（3D→2D→3D）" -ForegroundColor Yellow
Show-OpenStreetMap "Tokyo Tower" -Enable3D -Zoom 16 -Pitch 60
Start-Sleep -Seconds 2
Show-OpenStreetMap "Tokyo Tower" -Zoom 16 -Pitch 0
Start-Sleep -Seconds 2
Show-OpenStreetMap "Tokyo Tower" -Enable3D -Zoom 16 -Pitch 60
Start-Sleep -Seconds 3

# Test 3: カメラアングル変更
Write-Host "`nTest 3: カメラアングル変更（4方向から富士山を表示）" -ForegroundColor Yellow
Show-OpenStreetMap "Mount Fuji" -Enable3D -Pitch 65 -Bearing 0 -Duration 1.5
Start-Sleep -Seconds 2
Show-OpenStreetMap "Mount Fuji" -Enable3D -Pitch 65 -Bearing 90 -Duration 1.5
Start-Sleep -Seconds 2
Show-OpenStreetMap "Mount Fuji" -Enable3D -Pitch 65 -Bearing 180 -Duration 1.5
Start-Sleep -Seconds 2
Show-OpenStreetMap "Mount Fuji" -Enable3D -Pitch 65 -Bearing 270 -Duration 1.5
Start-Sleep -Seconds 3

Write-Host "`n=== テスト完了 ===" -ForegroundColor Green
Write-Host "ブラウザで以下を確認してください:" -ForegroundColor White
Write-Host "  ✓ 3D地形（山の起伏）が表示されているか" -ForegroundColor Gray
Write-Host "  ✓ 3D建物が表示されているか（都市部）" -ForegroundColor Gray
Write-Host "  ✓ 3Dトグルボタンが機能しているか" -ForegroundColor Gray
Write-Host "  ✓ カメラアングル（Pitch/Bearing）が変更されているか" -ForegroundColor Gray
Write-Host "  ✓ デバッグログにエラーがないか（コンソールを確認）" -ForegroundColor Gray
