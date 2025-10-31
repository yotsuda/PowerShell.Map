# Test script for updated Stop() method
Write-Host "=== MapServer.Stop() エラー抑止テスト ===" -ForegroundColor Cyan
Write-Host ""

# Staging フォルダからモジュールをインポート
$stagingPath = "C:\MyProj\PowerShell.Map\Staging"
Import-Module "$stagingPath\PowerShell.Map.psd1" -Force

Write-Host "1. モジュールをインポートしました" -ForegroundColor Green
Write-Host ""

# 地図を表示してサーバーを起動
Write-Host "2. サーバーを起動します..." -ForegroundColor Yellow
Show-OpenStreetMap -Location "Tokyo" | Out-Null
Write-Host "   ✓ サーバー起動完了" -ForegroundColor Green
Write-Host ""

# Remove-Module を実行
Write-Host "3. Remove-Module を実行します..." -ForegroundColor Yellow
Write-Host "   （エラーメッセージが表示されないことを確認）" -ForegroundColor Gray
Write-Host ""

Remove-Module PowerShell.Map -Verbose 2>&1 | Out-String | Write-Host -ForegroundColor Gray

Write-Host ""
Write-Host "4. 結果確認" -ForegroundColor Yellow

# ポートの状態確認
$listeners = [System.Net.NetworkInformation.IPGlobalProperties]::GetIPGlobalProperties().GetActiveTcpListeners()
$port8765 = $listeners | Where-Object { $_.Port -eq 8765 }

if ($port8765) {
    Write-Host "   ✗ ポート 8765 はまだリスニング中" -ForegroundColor Red
} else {
    Write-Host "   ✓ ポート 8765 は解放されました" -ForegroundColor Green
}

Write-Host ""
Write-Host "=== テスト完了 ===" -ForegroundColor Cyan
