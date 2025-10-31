# Test Show-OpenStreetMapRoute with unified description specification

Write-Host "Testing Show-OpenStreetMapRoute with descriptions..." -ForegroundColor Cyan

# Test 1: Simple route without descriptions
Write-Host "`n=== Test 1: Simple route (string locations) ===" -ForegroundColor Yellow
Show-OpenStreetMapRoute -From "Tokyo" -To "Osaka"

Start-Sleep -Seconds 2

# Test 2: Route with descriptions using hashtables
Write-Host "`n=== Test 2: Route with descriptions (hashtables) ===" -ForegroundColor Yellow
Show-OpenStreetMapRoute -From @{ Location = "Tokyo"; Description = "🗼 出発地: 日本の首都" } `
                        -To @{ Location = "Osaka"; Description = "🏯 目的地: 西日本最大の都市" }

Start-Sleep -Seconds 2

# Test 3: Mixed - From with description, To without
Write-Host "`n=== Test 3: Mixed (From with description, To simple) ===" -ForegroundColor Yellow
Show-OpenStreetMapRoute -From @{ Location = "Paris"; Description = "🇫🇷 City of Light" } `
                        -To "London"

Start-Sleep -Seconds 2

# Test 4: Coordinates with descriptions
Write-Host "`n=== Test 4: Coordinates with descriptions ===" -ForegroundColor Yellow
Show-OpenStreetMapRoute -From @{ Location = "35.6762,139.6503"; Description = "東京駅" } `
                        -To @{ Location = "34.6937,135.5023"; Description = "大阪駅" }

Write-Host "`nAll tests completed!" -ForegroundColor Green
