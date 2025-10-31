# Test simple location specification

Write-Host "=== Test 1: Single location (positional) ===" -ForegroundColor Yellow
Show-OpenStreetMap 東京

Start-Sleep -Seconds 2

Write-Host "`n=== Test 2: Single location with description ===" -ForegroundColor Yellow
Show-OpenStreetMap 東京 -Description "日本の首都、人口1400万人の大都市"

Start-Sleep -Seconds 2

Write-Host "`n=== Test 3: Single location with marker label ===" -ForegroundColor Yellow
Show-OpenStreetMap "Tokyo" -Marker "🗼 東京タワー" -Description "333mの電波塔"

Start-Sleep -Seconds 2

Write-Host "`n=== Test 4: Multiple locations ===" -ForegroundColor Yellow
Show-OpenStreetMap "Tokyo", "Osaka", "Kyoto" -Zoom 8

Write-Host "`nAll tests completed!" -ForegroundColor Green
