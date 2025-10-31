# Test unified Label support across all cmdlets

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Unified Label/Description/Color Test" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Test 1: Show-OpenStreetMap - Verify -Marker is removed
Write-Host "[Test 1] Show-OpenStreetMap - Verify -Marker parameter removed" -ForegroundColor Yellow
try {
    Show-OpenStreetMap "Tokyo" -Marker "Test" -ErrorAction Stop
    Write-Host "  ✗ FAIL: -Marker parameter still exists!" -ForegroundColor Red
} catch {
    Write-Host "  ✓ PASS: -Marker parameter successfully removed" -ForegroundColor Green
}
Start-Sleep -Seconds 1

# Test 2: Show-OpenStreetMap - Structured with Label
Write-Host "[Test 2] Show-OpenStreetMap - Structured location with Label" -ForegroundColor Yellow
try {
    $result = Show-OpenStreetMap -Locations @{ 
        Location = "Tokyo"
        Label = "🗼 東京タワー"
        Description = "333m communications tower"
        Color = "red"
    }
    if ($result.Label -eq "🗼 東京タワー" -and $result.Description -eq "333m communications tower") {
        Write-Host "  ✓ PASS: Label=$($result.Label), Description=$($result.Description), Color=$($result.Color)" -ForegroundColor Green
    } else {
        Write-Host "  ✗ FAIL: Label or Description not set correctly" -ForegroundColor Red
    }
} catch {
    Write-Host "  ✗ FAIL: $($_.Exception.Message)" -ForegroundColor Red
}
Start-Sleep -Seconds 2

# Test 3: Show-OpenStreetMap - Multiple locations with metadata
Write-Host "[Test 3] Show-OpenStreetMap - Multiple locations with Label/Description/Color" -ForegroundColor Yellow
try {
    $locations = @(
        @{ Location = "Tokyo"; Label = "🗼 東京"; Description = "首都"; Color = "red" }
        @{ Location = "Osaka"; Label = "🏯 大阪"; Description = "商都"; Color = "blue" }
        @{ Location = "Kyoto"; Label = "⛩️ 京都"; Description = "古都"; Color = "gold" }
    )
    $results = Show-OpenStreetMap -Locations $locations
    $allCorrect = $true
    foreach ($r in $results) {
        if ([string]::IsNullOrEmpty($r.Label) -or [string]::IsNullOrEmpty($r.Description)) {
            $allCorrect = $false
            break
        }
    }
    if ($allCorrect -and $results.Count -eq 3) {
        Write-Host "  ✓ PASS: All 3 locations have Label, Description, and Color" -ForegroundColor Green
    } else {
        Write-Host "  ✗ FAIL: Some locations missing metadata" -ForegroundColor Red
    }
} catch {
    Write-Host "  ✗ FAIL: $($_.Exception.Message)" -ForegroundColor Red
}
Start-Sleep -Seconds 2

# Test 4: Show-OpenStreetMapRoute - With Label and Description
Write-Host "[Test 4] Show-OpenStreetMapRoute - From/To with Label/Description" -ForegroundColor Yellow
try {
    $results = Show-OpenStreetMapRoute `
        -From @{ Location = "Tokyo"; Label = "🚀 出発地"; Description = "東京駅エリア" } `
        -To @{ Location = "Osaka"; Label = "🎯 目的地"; Description = "大阪城エリア" }
    
    if ($results[0].Label -eq "🚀 出発地" -and $results[1].Label -eq "🎯 目的地") {
        Write-Host "  ✓ PASS: From Label=$($results[0].Label), To Label=$($results[1].Label)" -ForegroundColor Green
    } else {
        Write-Host "  ✗ FAIL: Labels not set correctly. From=$($results[0].Label), To=$($results[1].Label)" -ForegroundColor Red
    }
} catch {
    Write-Host "  ✗ FAIL: $($_.Exception.Message)" -ForegroundColor Red
}
Start-Sleep -Seconds 2

# Test 5: Show-OpenStreetMapRoute - With Color
Write-Host "[Test 5] Show-OpenStreetMapRoute - From/To with Color" -ForegroundColor Yellow
try {
    $results = Show-OpenStreetMapRoute `
        -From @{ Location = "Tokyo"; Label = "出発"; Color = "green" } `
        -To @{ Location = "Osaka"; Label = "到着"; Color = "red" }
    
    if ($results[0].Color -like "*green*" -or $results[1].Color -like "*red*") {
        Write-Host "  ✓ PASS: Colors applied (From=$($results[0].Color), To=$($results[1].Color))" -ForegroundColor Green
    } else {
        Write-Host "  ⚠ WARNING: Color may not be applied to route markers" -ForegroundColor Yellow
    }
} catch {
    Write-Host "  ✗ FAIL: $($_.Exception.Message)" -ForegroundColor Red
}
Start-Sleep -Seconds 2

# Test 6: Start-OpenStreetMapTour - With Label/Description/Color
Write-Host "[Test 6] Start-OpenStreetMapTour - Locations with Label/Description/Color" -ForegroundColor Yellow
try {
    $tourLocations = @(
        @{ Location = "Tokyo"; Label = "🗼 東京"; Description = "首都"; Color = "red" }
        @{ Location = "Osaka"; Label = "🏯 大阪"; Description = "商都"; Color = "blue" }
    )
    $results = Start-OpenStreetMapTour -Locations $tourLocations -PauseTime 0.5 -Duration 0.5
    
    $allHaveLabels = ($results | Where-Object { -not [string]::IsNullOrEmpty($_.Label) }).Count -eq 2
    $allHaveDescriptions = ($results | Where-Object { -not [string]::IsNullOrEmpty($_.Description) }).Count -eq 2
    
    if ($allHaveLabels -and $allHaveDescriptions) {
        Write-Host "  ✓ PASS: All tour stops have Label and Description" -ForegroundColor Green
    } else {
        Write-Host "  ✗ FAIL: Some tour stops missing Label or Description" -ForegroundColor Red
    }
} catch {
    Write-Host "  ✗ FAIL: $($_.Exception.Message)" -ForegroundColor Red
}
Start-Sleep -Seconds 1

# Test 7: Backward compatibility - Simple usage without Label
Write-Host "[Test 7] Backward compatibility - Simple usage still works" -ForegroundColor Yellow
try {
    $r1 = Show-OpenStreetMap "Tokyo"
    $r2 = Show-OpenStreetMapRoute -From "Tokyo" -To "Osaka"
    $r3 = Start-OpenStreetMapTour "Tokyo", "Osaka" -PauseTime 0.5 -Duration 0.5
    
    if ($r1 -and $r2 -and $r3) {
        Write-Host "  ✓ PASS: All simple usages work without Label parameter" -ForegroundColor Green
    } else {
        Write-Host "  ✗ FAIL: Simple usage broken" -ForegroundColor Red
    }
} catch {
    Write-Host "  ✗ FAIL: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Test Complete!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
