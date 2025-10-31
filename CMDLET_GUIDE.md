# PowerShell.Map - アニメーション機能と統一設計

## 📦 Cmdlet一覧

PowerShell.Mapは3つの主要Cmdletで構成されています。すべてに`OpenStreetMap`プレフィックスがあり、パラメータも統一されています。

### 1. `Show-OpenStreetMap`
地点やマーカーを地図に表示する万能Cmdlet

```powershell
# 単一地点
Show-OpenStreetMap "Tokyo"

# ズーム変更のみ（位置は維持）
Show-OpenStreetMap -Zoom 15 -Animate

# 位置 + ズーム
Show-OpenStreetMap "Osaka" -Zoom 12 -Animate -Duration 2.0

# 複数マーカー
Show-OpenStreetMap "Tokyo", "Osaka", "Kyoto"

# 詳細マーカー
$markers = @(
    @{ Location = "Tokyo Tower"; Label = "🗼 Tower"; Color = "red" }
    @{ Location = "Tokyo Skytree"; Label = "🏗️ Skytree"; Color = "blue" }
)
Show-OpenStreetMap -Markers $markers -Zoom 11

# パイプライン
Import-Csv locations.csv | Show-OpenStreetMap
```

### 2. `Show-OpenStreetMapRoute`
2地点間のルートを表示

```powershell
# 基本
Show-OpenStreetMapRoute "Tokyo Station" "Tokyo Skytree"

# カスタマイズ
Show-OpenStreetMapRoute "Tokyo" "Osaka" `
    -Color "#ff6600" -Width 6 -Zoom 8 -Animate -Duration 2.0
```

### 3. `Invoke-OpenStreetMapTour`
複数地点を自動巡回

```powershell
# 基本
Invoke-OpenStreetMapTour "Tokyo", "Osaka", "Kyoto"

# カスタマイズ
Invoke-OpenStreetMapTour "Paris", "London", "Berlin" `
    -Zoom 12 -PauseTime 3.0 -Duration 2.0
```

## 🔧 共通パラメータ

すべてのCmdletで以下のパラメータが使用可能:

| パラメータ | 型 | デフォルト | 説明 |
|-----------|-----|-----------|------|
| `-Zoom` | int | 自動/現在値 | ズームレベル [1-19] |
| `-Animate` | switch | なし | スムーズアニメーション有効化 |
| `-Duration` | double | 1.0または1.5 | アニメーション時間 [0.1-10.0秒] |
| `-DebugMode` | switch | なし | 詳細ログ表示 |

## 🎯 使用例

### ズーム変更のみ
```powershell
# 現在の位置を維持してズーム
Show-OpenStreetMap -Zoom 18 -Animate -Duration 2.0
```

### 位置移動のみ
```powershell
# ズームレベルを維持して移動（現在のズームを使用）
Show-OpenStreetMap "Shibuya" -Animate
```

### データ可視化
```powershell
# CSVから複数地点を表示
Import-Csv stores.csv | Show-OpenStreetMap -Animate

# 詳細マーカーで分類表示
$stores = Import-Csv stores.csv
$markers = $stores | ForEach-Object {
    @{
        Location = $_.Address
        Label = $_.StoreName
        Color = if ($_.Type -eq "Restaurant") { "red" } else { "blue" }
    }
}
Show-OpenStreetMap -Markers $markers -Zoom 12
```

### プレゼンテーション
```powershell
# 観光スポットツアー
Invoke-OpenStreetMapTour "Tokyo Tower", "Skytree", "Sensoji" `
    -Zoom 15 -PauseTime 3.0 -Duration 2.0

# ルート案内
Show-OpenStreetMapRoute "Start Point" "Goal" `
    -Animate -Duration 2.0 -Color "#ff6600" -Width 6
```

## ✨ 設計の特徴

### 一貫性
- ✅ すべてに`OpenStreetMap`プレフィックス
- ✅ パラメータ名が統一（`-Duration`, `-Zoom`など）
- ✅ 動詞も標準（Show, Invoke）

### 柔軟性
- ✅ `Show-OpenStreetMap`は位置・ズーム・両方の変更に対応
- ✅ パイプライン対応
- ✅ 複数の入力形式（文字列、配列、オブジェクト）

### シンプルさ
- ✅ 重複機能なし（各Cmdletの役割が明確）
- ✅ オプショナルパラメータで簡潔な記述が可能
- ✅ デフォルト値が適切

## 🚀 クイックスタート

```powershell
# モジュールをインポート
Import-Module .\PowerShell.Map\bin\Release\net9.0\PowerShell.Map.dll

# 地図を表示
Show-OpenStreetMap "Tokyo" -Animate

# ズームイン
Show-OpenStreetMap -Zoom 15 -Animate -Duration 2.0

# ツアーを実行
Invoke-OpenStreetMapTour "Tokyo", "Osaka", "Kyoto"
```

## 📝 デモ実行

```powershell
.\Demo-MapCmdlets.ps1
```

---

**Note**: PowerShell 7+ と .NET 9.0 が必要です
