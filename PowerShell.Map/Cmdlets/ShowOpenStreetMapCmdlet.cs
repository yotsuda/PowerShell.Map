using System.Collections;
using System.Management.Automation;
using PowerShell.Map.Helpers;
using PowerShell.Map.Server;

namespace PowerShell.Map.Cmdlets;

[Cmdlet(VerbsCommon.Show, "OpenStreetMap")]
[OutputType(typeof(MapMarker))]
public class ShowOpenStreetMapCmdlet : MapCmdletBase
{
    // パラメータセット定義
    private const string LocationSet = "Location";
    private const string MarkersSet = "Markers";
    private const string PipelineSet = "Pipeline";

    // 位置指定パラメータ (地名または座標文字列、単数または複数)
    [Parameter(Position = 0, ParameterSetName = LocationSet)]
    public string[]? Location { get; set; }

    // 単一マーカーのラベル (Location が1つの場合のみ有効)
    [Parameter(ParameterSetName = LocationSet)]
    public string? Marker { get; set; }

    // 複数マーカー (Markersパラメータセット専用)
    // 文字列、Hashtable、MapMarkerオブジェクトすべてに対応
    [Parameter(ParameterSetName = MarkersSet, Mandatory = true, ValueFromPipeline = true)]
    [AllowEmptyCollection]
    public object[]? Markers { get; set; }

    // パイプラインからプロパティ名で受け取る（CSVなどから）
    [Parameter(ParameterSetName = PipelineSet, ValueFromPipelineByPropertyName = true, Mandatory = true)]
    public string? Latitude { get; set; }

    [Parameter(ParameterSetName = PipelineSet, ValueFromPipelineByPropertyName = true, Mandatory = true)]
    public string? Longitude { get; set; }

    [Parameter(ParameterSetName = PipelineSet, ValueFromPipelineByPropertyName = true)]
    public string? Label { get; set; }

    [Parameter(ParameterSetName = PipelineSet, ValueFromPipelineByPropertyName = true)]
    public string? Color { get; set; }

    // 共通パラメータ
    [Parameter]
    [ValidateRange(1, 19)]
    public int? Zoom { get; set; }

    /// <summary>
    /// Animation duration in seconds (0.0-10.0, default: 1.0)
    /// Set to 0 for instant movement without animation
    /// </summary>
    [Parameter]
    [ValidateRange(0.0, 10.0)]
    public double Duration { get; set; } = 1.0;

    /// <summary>
    /// Enable 3D display (buildings and terrain)
    /// </summary>
    [Parameter]
    public SwitchParameter Enable3D { get; set; }

    /// <summary>
    /// Camera bearing in degrees (0-360, 0=North, 90=East, 180=South, 270=West)
    /// </summary>
    [Parameter]
    [ValidateRange(0, 360)]
    public double Bearing { get; set; } = 0;

    /// <summary>
    /// Camera pitch in degrees (0-85, 0=top-down view, 60=default for 3D, 85=almost horizontal)
    /// </summary>
    [Parameter]
    [ValidateRange(0, 85)]
    public double Pitch { get; set; } = 0;

    /// <summary>
    /// Optional description to display for the location (only for single location display)
    /// </summary>
    [Parameter]
    public string? Description { get; set; }
    // [Parameter]
    private SwitchParameter DebugMode { get; set; }

    private readonly List<MapMarker> _pipelineMarkers = [];

    protected override void ProcessRecord()
    {
        try
        {
            var server = MapServer.Instance;

            // Automatically enable 3D mode if Bearing or Pitch is specified
            if ((Bearing != 0 || Pitch != 0) && !Enable3D)
            {
                Enable3D = true;
                WriteVerbose("3D mode automatically enabled due to Bearing/Pitch parameters");
            }

            // パイプラインからプロパティ経由で受け取った場合
            if (ParameterSetName == PipelineSet && !string.IsNullOrEmpty(Latitude) && !string.IsNullOrEmpty(Longitude))
            {
                if (double.TryParse(Latitude, out double lat) && double.TryParse(Longitude, out double lon))
                {
                    _pipelineMarkers.Add(new MapMarker
                    {
                        Latitude = lat,
                        Longitude = lon,
                        Label = Label,
                        Color = GetMarkerColor(Color)
                    });
                    WriteVerbose($"Added marker from pipeline: {Label ?? $"{lat},{lon}"}");
                }
                return; // EndProcessingで一括処理
            }

            // Markersパラメータセットの処理
            if (ParameterSetName == MarkersSet && Markers != null)
            {
                var markerList = MarkerParser.ParseMarkers(Markers, GetMarkerColor, msg => WriteVerbose(msg), msg => WriteWarning(msg));

                if (markerList.Count == 0)
                {
                    WriteWarning("No valid markers provided in this batch");
                    return;
                }

                // パイプラインから来た場合は蓄積する (EndProcessingで一括処理)
                _pipelineMarkers.AddRange(markerList);
                WriteVerbose($"Added {markerList.Count} markers to pipeline batch (total: {_pipelineMarkers.Count})");
                return;
            }

            // 単一位置の処理 (Locationパラメータセット)
            if (Location != null && Location.Length > 0)
            {
                // 複数の場所が指定された場合 → マーカーとして表示
                if (Location.Length > 1)
                {
                    if (!string.IsNullOrEmpty(Marker))
                    {
                        WriteWarning("-Marker parameter is ignored when multiple locations are specified");
                    }

                    var markerList = new List<MapMarker>();
                    foreach (var loc in Location)
                    {
                        // 座標文字列かどうかチェック
                        bool isCoordStr = CoordinateValidator.IsCoordinateString(loc);

                        if (!LocationHelper.TryParseLocation(loc, out double markerLat, out double markerLon,
                            msg => WriteVerbose(msg), msg => WriteWarning(msg)))
                        {
                            WriteWarning($"Could not parse location: {loc}, skipping");
                            markerList.Add(new MapMarker
                            {
                                Location = loc,
                                Status = "Failed",
                                GeocodingSource = "Unknown"
                            });
                            continue;
                        }

                        // ラベルを決定: 座標の場合は逆ジオコーディング→座標、ロケーション名の場合はロケーション名
                        string markerLabel;
                        if (isCoordStr)
                        {
                            // 座標の場合、逆ジオコーディングを試みる
                            if (LocationHelper.TryReverseGeocode(markerLat, markerLon, out string? reversedName, msg => WriteVerbose(msg)))
                            {
                                markerLabel = reversedName!;
                                WriteVerbose($"Using reverse geocoded name: {markerLabel}");
                            }
                            else
                            {
                                markerLabel = $"{markerLat:F6},{markerLon:F6}";
                                WriteVerbose($"Reverse geocoding failed, using coordinates as label");
                            }
                        }
                        else
                        {
                            markerLabel = loc;
                        }

                        markerList.Add(new MapMarker
                        {
                            Latitude = markerLat,
                            Longitude = markerLon,
                            Label = markerLabel,
                            Color = GetMarkerColor(null),
                            Location = loc,
                            Status = "Success",
                            GeocodingSource = isCoordStr ? "Coordinates" : "Nominatim"
                        });
                        WriteVerbose($"Added marker: {loc} at {markerLat}, {markerLon}");
                    }

                    if (markerList.Count == 0)
                    {
                        WriteError(new ErrorRecord(
                            new ArgumentException("No valid locations provided"),
                            "NoValidLocations",
                            ErrorCategory.InvalidArgument,
                            Location));
                        return;
                    }

                    // 成功したマーカーのみを地図に表示対象とする
                    var successMarkers = markerList.Where(m => m.Status == "Success").ToArray();
                    
                    // 外れ値検出 (2個以上のマーカーがある場合のみ)
                    MapMarker[] validMarkers;
                    if (successMarkers.Length >= 2)
                    {
                        var (valid, outliers) = OutlierDetector.DetectOutliers(successMarkers, msg => WriteVerbose(msg), msg => WriteWarning(msg));
                        validMarkers = valid;
                        
                        // Mark outliers
                        foreach (var outlier in outliers)
                        {
                            outlier.Status = "Outlier";
                        }
                    }
                    else
                    {
                        validMarkers = successMarkers;
                    }
                    
                    if (validMarkers.Length > 0)
                    {
                        ExecuteWithRetry(server, () => server.UpdateMapWithMarkers(validMarkers, Zoom, DebugMode, Enable3D, Bearing, Pitch));
                        WriteVerbose($"Map updated with {validMarkers.Length} markers");
                    }
                    
                    // すべてのマーカー情報を出力
                    foreach (var m in markerList)
                    {
                        WriteObject(m);
                    }
                    return;
                }

                // 単一の場所が指定された場合
                bool isSingleCoord = CoordinateValidator.IsCoordinateString(Location[0]);

                if (!LocationHelper.TryParseLocation(Location[0], out double lat, out double lon,
                    msg => WriteVerbose(msg), msg => WriteWarning(msg)))
                {
                    var failedMarker = new MapMarker
                    {
                        Location = Location[0],
                        Label = Marker,
                        Status = "Failed",
                        GeocodingSource = "Unknown"
                    };
                    WriteObject(failedMarker);
                    
                    WriteError(new ErrorRecord(
                        new ArgumentException($"Invalid location format: {Location[0]}. Use 'latitude,longitude' format or a place name."),
                        "InvalidLocation",
                        ErrorCategory.InvalidArgument,
                        Location[0]));
                    return;
                }

                int zoom = Zoom ?? (Enable3D ? 17 : 13);
                
                // ラベルを決定: Markerパラメータがあればそれを使用、なければ座標の場合は逆ジオコーディング→座標、ロケーション名の場合はロケーション名
                string label;
                if (Marker != null)
                {
                    label = Marker;
                }
                else if (isSingleCoord)
                {
                    // 座標の場合、逆ジオコーディングを試みる
                    if (LocationHelper.TryReverseGeocode(lat, lon, out string? reversedName, msg => WriteVerbose(msg)))
                    {
                        label = reversedName!;
                        WriteVerbose($"Using reverse geocoded name: {label}");
                    }
                    else
                    {
                        label = $"{lat:F6},{lon:F6}";
                        WriteVerbose($"Reverse geocoding failed, using coordinates as label");
                    }
                }
                else
                {
                    label = Location[0];
                }

                ExecuteWithRetry(server, () => server.UpdateMap(lat, lon, zoom, label, DebugMode, Duration, Enable3D, Bearing, Pitch, Description));
                WriteVerbose($"Map updated: {lat}, {lon} @ zoom {zoom}");
                
                // マーカー情報を出力
                var resultMarker = new MapMarker
                {
                    Latitude = lat,
                    Longitude = lon,
                    Label = label,
                    Color = GetMarkerColor(null),
                    Location = Location[0],
                    Status = "Success",
                    GeocodingSource = isSingleCoord ? "Coordinates" : "Nominatim"
                };
                WriteObject(resultMarker);
                return;
            }

            // パラメータが何も指定されていない場合（現在の状態を使用）
            {
                var currentState = server.GetCurrentState();
                double lat = currentState.Latitude;
                double lon = currentState.Longitude;
                int zoom = Zoom ?? currentState.Zoom;
                string? marker = Marker ?? currentState.Marker;

                WriteVerbose($"Using current location: {lat}, {lon}");
                ExecuteWithRetry(server, () => server.UpdateMap(lat, lon, zoom, marker, DebugMode, Duration, Enable3D, Bearing, Pitch, Description));
                WriteVerbose($"Map updated: {lat}, {lon} @ zoom {zoom}");
            }
        }
        catch (Exception ex)
        {
            WriteError(new ErrorRecord(
                ex,
                "ShowMapFailed",
                ErrorCategory.NotSpecified,
                null));
        }
    }

    protected override void EndProcessing()
    {
        // パイプラインから受け取ったマーカーをまとめて処理
        if (_pipelineMarkers.Count > 0)
        {
            try
            {
                var server = MapServer.Instance;
                
                // 成功したマーカーのみを地図に表示対象とする
                var successMarkers = _pipelineMarkers.Where(m => m.Status == "Success").ToArray();
                
                // 外れ値検出 (2個以上のマーカーがある場合のみ)
                MapMarker[] validMarkers;
                if (successMarkers.Length >= 2)
                {
                    var (valid, outliers) = OutlierDetector.DetectOutliers(successMarkers, msg => WriteVerbose(msg), msg => WriteWarning(msg));
                    validMarkers = valid;
                    
                    // Mark outliers
                    foreach (var outlier in outliers)
                    {
                        outlier.Status = "Outlier";
                    }
                }
                else
                {
                    validMarkers = successMarkers;
                }
                
                if (validMarkers.Length > 0)
                {
                    ExecuteWithRetry(server, () => server.UpdateMapWithMarkers(validMarkers, Zoom, DebugMode, Enable3D, Bearing, Pitch));
                    WriteVerbose($"Map updated with {validMarkers.Length} markers from pipeline");
                }
                
                // 常にマーカー情報をパイプラインに出力
                foreach (var marker in _pipelineMarkers)
                {
                    WriteObject(marker);
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    ex,
                    "ShowMapFailed",
                    ErrorCategory.NotSpecified,
                    null));
            }
        }
    }



    /// <summary>
    /// 外れ値を検出する。マーカー群の中心から極端に離れているマーカーを除外する。
    /// </summary>
    /// <param name="markers">成功したマーカーの配列</param>
    /// <returns>有効なマーカーと外れ値のタプル</returns>
}
