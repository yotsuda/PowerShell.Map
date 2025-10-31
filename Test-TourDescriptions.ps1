# Test Start-OpenStreetMapTour with location descriptions

Write-Host "Testing Start-OpenStreetMapTour with descriptions..." -ForegroundColor Cyan

# Test 1: Simple tour without descriptions
Write-Host "`n=== Test 1: Simple tour (without descriptions) ===" -ForegroundColor Yellow
Start-OpenStreetMapTour -Location "Tokyo", "Osaka", "Kyoto" -Zoom 12 -PauseTime 1.5 -Duration 1.0

Start-Sleep -Seconds 2

# Test 2: Tour with descriptions using hashtables
Write-Host "`n=== Test 2: Tour with descriptions (hashtables) ===" -ForegroundColor Yellow
$locations = @(
    @{ Location = "Tokyo"; Description = "🗼 Capital of Japan - A vibrant metropolis blending tradition and technology" }
    @{ Location = "Mount Fuji"; Description = "🗻 Japan''s highest mountain at 3,776m - An iconic symbol of Japan" }
    @{ Location = "Kyoto"; Description = "⛩️ Ancient capital with 2,000+ temples and shrines" }
)

Start-OpenStreetMapTour -Locations $locations -Zoom 13 -PauseTime 2.0 -Duration 1.2

Start-Sleep -Seconds 2

# Test 3: Tour with mixed content (some with descriptions, some without)
Write-Host "`n=== Test 3: Mixed tour (some with descriptions) ===" -ForegroundColor Yellow
$mixedLocations = @(
    @{ Location = "Paris"; Description = "🗼 The City of Light - Home to the Eiffel Tower and Louvre Museum" }
    @{ Location = "London" }  # No description
    @{ Location = "Berlin"; Description = "🏛️ Capital of Germany - Rich history and vibrant culture" }
)

Start-OpenStreetMapTour -Locations $mixedLocations -Zoom 11 -PauseTime 1.8 -Duration 1.0

Write-Host "`nAll tests completed!" -ForegroundColor Green
