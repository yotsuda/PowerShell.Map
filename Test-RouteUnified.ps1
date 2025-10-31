# Test Show-OpenStreetMapRoute with unified description specification

Write-Host "Testing unified description specification..." -ForegroundColor Cyan

# Test 1: Simple route without descriptions (backward compatible)
Write-Host "`n=== Test 1: Simple route (no descriptions) ===" -ForegroundColor Yellow
Show-OpenStreetMapRoute -From "Tokyo" -To "Osaka"

Start-Sleep -Seconds 2

# Test 2: Route with descriptions using hashtables
Write-Host "`n=== Test 2: Route with descriptions (hashtables) ===" -ForegroundColor Yellow
Show-OpenStreetMapRoute -From @{ Location = "Tokyo"; Description = "🗼 出発地：日本の首都、人口1400万人" } `
                        -To @{ Location = "Osaka"; Description = "🏯 目的地：西日本最大の都市、食の都" }

Start-Sleep -Seconds 2

# Test 3: Mixed - simple From, structured To
Write-Host "`n=== Test 3: Mixed specification ===" -ForegroundColor Yellow
Show-OpenStreetMapRoute -From "Kyoto" `
                        -To @{ Location = "Nara"; Description = "🦌 古都奈良、東大寺と鹿で有名" }

Write-Host "`nAll tests completed!" -ForegroundColor Green
