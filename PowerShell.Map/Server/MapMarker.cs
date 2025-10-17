namespace PowerShell.Map.Server;

public class MapMarker
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Label { get; set; }
    public string? Color { get; set; }
    
    // 新規追加: PassThru用プロパティ
    public string? Location { get; set; }          // 元の入力文字列
    public string Status { get; set; } = "Success";  // Success/Failed/Warning/Outlier
    public string? GeocodingSource { get; set; }    // Nominatim/Coordinates/etc
    
    // ツアー用プロパティ
    public int? Step { get; set; }                  // 現在のステップ番号（ツアー用）
    public int? TotalSteps { get; set; }            // 総ステップ数（ツアー用）
}