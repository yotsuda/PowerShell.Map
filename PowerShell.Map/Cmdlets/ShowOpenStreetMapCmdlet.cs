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

    [Parameter]
    public SwitchParameter DebugMode { get; set; }

    private readonly List<MapMarker> _pipelineMarkers = [];

    protected override void ProcessRecord()
    {
        try
        {
            var server = MapServer.Instance;

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
                var markerList = ParseMarkers(Markers);

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
                        bool isCoordStr = loc.Contains(',') &&
                            loc.Split(',').Length == 2 &&
                            double.TryParse(loc.Split(',')[0].Trim(), out _) &&
                            double.TryParse(loc.Split(',')[1].Trim(), out _);

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
                        var (valid, outliers) = DetectOutliers(successMarkers);
                        validMarkers = valid;
                        
                        // 外れ値について警告
                        foreach (var outlier in outliers)
                        {
                            outlier.Status = "Outlier";
                            var locationInfo = outlier.Label ?? outlier.Location ?? "Unknown";
                            WriteWarning($"Marker '{locationInfo}' is too far from other markers (lat={outlier.Latitude:F4}, lon={outlier.Longitude:F4}) and will be excluded from the map.");
                        }
                    }
                    else
                    {
                        validMarkers = successMarkers;
                    }
                    
                    if (validMarkers.Length > 0)
                    {
                        ExecuteWithRetry(server, () => server.UpdateMapWithMarkers(validMarkers, Zoom, DebugMode));
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
                bool isSingleCoord = Location[0].Contains(',') && 
                    Location[0].Split(',').Length == 2 &&
                    double.TryParse(Location[0].Split(',')[0].Trim(), out _) &&
                    double.TryParse(Location[0].Split(',')[1].Trim(), out _);

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

                int zoom = Zoom ?? 13;
                
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
                
                ExecuteWithRetry(server, () => server.UpdateMap(lat, lon, zoom, label, DebugMode, Duration));
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
                ExecuteWithRetry(server, () => server.UpdateMap(lat, lon, zoom, marker, DebugMode, Duration));
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
                    var (valid, outliers) = DetectOutliers(successMarkers);
                    validMarkers = valid;
                    
                    // 外れ値について警告
                    foreach (var outlier in outliers)
                    {
                        outlier.Status = "Outlier";
                        var locationInfo = outlier.Label ?? outlier.Location ?? "Unknown";
                        WriteWarning($"Marker '{locationInfo}' is too far from other markers (lat={outlier.Latitude:F4}, lon={outlier.Longitude:F4}) and will be excluded from the map.");
                    }
                }
                else
                {
                    validMarkers = successMarkers;
                }
                
                if (validMarkers.Length > 0)
                {
                    ExecuteWithRetry(server, () => server.UpdateMapWithMarkers(validMarkers, Zoom, DebugMode));
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
    /// マーカー配列を解析してMapMarkerのリストに変換
    /// 文字列、Hashtable、MapMarkerオブジェクト、PSObjectに対応
    /// </summary>
    private List<MapMarker> ParseMarkers(object[] markers)
    {
        var markerList = new List<MapMarker>();

        foreach (var markerObj in markers)
        {
            MapMarker? parsedMarker = null;

            // 文字列の場合（最もシンプル）
            if (markerObj is string locationStr)
            {
                parsedMarker = ParseStringMarker(locationStr);
            }
            // MapMarkerオブジェクトの場合（型安全）
            else if (markerObj is MapMarker mapMarker)
            {
                parsedMarker = mapMarker;
            }
            // Hashtableの場合（詳細制御）
            else if (markerObj is Hashtable markerHash)
            {
                parsedMarker = ParseHashtableMarker(markerHash);
            }
            // PSObjectの場合 (パイプライン経由のカスタムオブジェクト)
            else if (markerObj is PSObject psObj)
            {
                parsedMarker = ParsePSObjectMarker(psObj);
            }
            else
            {
                WriteWarning($"Unsupported marker type: {markerObj?.GetType().Name}, skipping");
                continue;
            }

            if (parsedMarker != null)
            {
                markerList.Add(parsedMarker);
                WriteVerbose($"Added marker: {parsedMarker.Label ?? $"{parsedMarker.Latitude},{parsedMarker.Longitude}"} at {parsedMarker.Latitude}, {parsedMarker.Longitude}");
            }
        }

        return markerList;
    }

    /// <summary>
    /// 文字列からマーカーを解析
    /// フォーマット: "Location" または "Location|Label" または "Location|Label|Color"
    /// </summary>
    private MapMarker? ParseStringMarker(string locationStr)
    {
        var parts = locationStr.Split('|');
        var location = parts[0].Trim();
        var label = parts.Length > 1 ? parts[1].Trim() : null;
        var color = parts.Length > 2 ? parts[2].Trim() : null;

        if (string.IsNullOrEmpty(location))
        {
            WriteWarning("Empty location string, skipping");
            return null;
        }

        // 座標文字列かどうかチェック
        bool isCoordinates = location.Contains(',') && 
            location.Split(',').Length == 2 &&
            double.TryParse(location.Split(',')[0].Trim(), out _) &&
            double.TryParse(location.Split(',')[1].Trim(), out _);

        if (!LocationHelper.TryParseLocation(location, out double markerLat, out double markerLon,
            msg => WriteVerbose(msg), msg => WriteWarning(msg)))
        {
            WriteWarning($"Could not parse location: {location}, skipping");
            return new MapMarker
            {
                Location = location,
                Label = label,
                Status = "Failed",
                GeocodingSource = "Unknown"
            };
        }

        return new MapMarker
        {
            Latitude = markerLat,
            Longitude = markerLon,
            Label = label,
            Color = GetMarkerColor(color),
            Location = location,
            Status = "Success",
            GeocodingSource = isCoordinates ? "Coordinates" : "Nominatim"
        };
    }

    /// <summary>
    /// Hashtableからマーカーを解析
    /// </summary>
    private MapMarker? ParseHashtableMarker(Hashtable markerHash)
    {
        var location = markerHash["Location"]?.ToString();
        var label = markerHash["Label"]?.ToString();
        var color = markerHash["Color"]?.ToString();

        if (string.IsNullOrEmpty(location))
        {
            WriteWarning("Marker without Location property, skipping");
            return null;
        }

        // 座標文字列かどうかチェック
        bool isCoordinates = location.Contains(',') && 
            location.Split(',').Length == 2 &&
            double.TryParse(location.Split(',')[0].Trim(), out _) &&
            double.TryParse(location.Split(',')[1].Trim(), out _);

        if (!LocationHelper.TryParseLocation(location, out double markerLat, out double markerLon,
            msg => WriteVerbose(msg), msg => WriteWarning(msg)))
        {
            WriteWarning($"Could not parse location: {location}, skipping");
            return new MapMarker
            {
                Location = location,
                Label = label,
                Status = "Failed",
                GeocodingSource = "Unknown"
            };
        }

        return new MapMarker
        {
            Latitude = markerLat,
            Longitude = markerLon,
            Label = label,
            Color = GetMarkerColor(color),
            Location = location,
            Status = "Success",
            GeocodingSource = isCoordinates ? "Coordinates" : "Nominatim"
        };
    }

    /// <summary>
    /// PSObjectからマーカーを解析
    /// </summary>
    private MapMarker? ParsePSObjectMarker(PSObject psObj)
    {
        var location = psObj.Properties["Location"]?.Value?.ToString();
        var label = psObj.Properties["Label"]?.Value?.ToString();
        var color = psObj.Properties["Color"]?.Value?.ToString();

        // Locationプロパティがある場合
        if (!string.IsNullOrEmpty(location))
        {
            if (!LocationHelper.TryParseLocation(location, out double markerLat, out double markerLon,
                msg => WriteVerbose(msg), msg => WriteWarning(msg)))
            {
                WriteWarning($"Could not parse location: {location}, skipping");
                return null;
            }

            return new MapMarker
            {
                Latitude = markerLat,
                Longitude = markerLon,
                Label = label,
                Color = GetMarkerColor(color)
            };
        }

        // Latitude/Longitudeプロパティを直接使用（CSVパイプライン用）
        var latStr = psObj.Properties["Latitude"]?.Value?.ToString();
        var lonStr = psObj.Properties["Longitude"]?.Value?.ToString();

        if (!string.IsNullOrEmpty(latStr) && !string.IsNullOrEmpty(lonStr))
        {
            if (double.TryParse(latStr, out double lat) && double.TryParse(lonStr, out double lon))
            {
                return new MapMarker
                {
                    Latitude = lat,
                    Longitude = lon,
                    Label = label,
                    Color = GetMarkerColor(color)
                };
            }
            else
            {
                WriteWarning($"Could not parse Latitude/Longitude: {latStr}, {lonStr}, skipping");
                return null;
            }
        }

        WriteWarning("Marker without Location or Latitude/Longitude properties, skipping");
        return null;
    }

    /// <summary>
    /// 外れ値を検出する。マーカー群の中心から極端に離れているマーカーを除外する。
    /// </summary>
    /// <param name="markers">成功したマーカーの配列</param>
    /// <returns>有効なマーカーと外れ値のタプル</returns>
    private (MapMarker[] valid, MapMarker[] outliers) DetectOutliers(MapMarker[] markers)
    {
        if (markers.Length < 2)
        {
            return (markers, Array.Empty<MapMarker>());
        }

        // 最も近い2つのマーカー間の距離を計算（クラスタリングの密度チェック）
        double minDistance = double.MaxValue;
        for (int i = 0; i < markers.Length; i++)
        {
            for (int j = i + 1; j < markers.Length; j++)
            {
                var dist = CalculateDistance(
                    markers[i].Latitude!.Value, markers[i].Longitude!.Value,
                    markers[j].Latitude!.Value, markers[j].Longitude!.Value);
                minDistance = Math.Min(minDistance, dist);
            }
        }

        // マーカーが広範囲に散らばっている場合の特別処理
        // 最も近いマーカー同士が1000km以上離れている場合
        if (minDistance > 1000)
        {
            WriteWarning($"Markers are very far apart (minimum distance: {minDistance:F0}km). Only the first marker will be displayed on the map. Please verify your input data.");
            // 先頭の1つだけを有効として返し、残りを外れ値とする
            var firstMarker = new[] { markers[0] };
            var restMarkers = markers.Skip(1).ToArray();
            return (firstMarker, restMarkers);
        }

        // 中央値を計算
        // 中央値を計算 (all markers here have valid coordinates)
        var latitudes = markers.Select(m => m.Latitude!.Value).OrderBy(x => x).ToArray();
        var longitudes = markers.Select(m => m.Longitude!.Value).OrderBy(x => x).ToArray();
        
        double medianLat = latitudes.Length % 2 == 0
            ? (latitudes[latitudes.Length / 2 - 1] + latitudes[latitudes.Length / 2]) / 2
            : latitudes[latitudes.Length / 2];
            
        double medianLon = longitudes.Length % 2 == 0
            ? (longitudes[longitudes.Length / 2 - 1] + longitudes[longitudes.Length / 2]) / 2
            : longitudes[longitudes.Length / 2];

        // 各マーカーと中央値との距離を計算
        var markerDistances = markers.Select(m => new
        {
            Marker = m,
            Distance = CalculateDistance(medianLat, medianLon, m.Latitude!.Value, m.Longitude!.Value)
        }).ToArray();

        // 中央値からの距離の中央値を計算
        var distances = markerDistances.Select(md => md.Distance).OrderBy(d => d).ToArray();
        double medianDistance = distances.Length % 2 == 0
            ? (distances[distances.Length / 2 - 1] + distances[distances.Length / 2]) / 2
            : distances[distances.Length / 2];

        // しきい値: 中央値距離の5倍、または最低500km
        // より厳しい基準に変更（10倍 → 5倍）
        double threshold = Math.Max(medianDistance * 5, 500);

        // 外れ値を検出
        var validMarkers = new List<MapMarker>();
        var outlierMarkers = new List<MapMarker>();

        foreach (var md in markerDistances)
        {
            if (md.Distance > threshold)
            {
                outlierMarkers.Add(md.Marker);
                WriteVerbose($"Outlier detected: {md.Marker.Location} (distance: {md.Distance:F1}km from median, threshold: {threshold:F1}km)");
            }
            else
            {
                validMarkers.Add(md.Marker);
            }
        }

        return (validMarkers.ToArray(), outlierMarkers.ToArray());
    }

    /// <summary>
    /// 2点間の距離をHaversine公式で計算（単位: km）
    /// </summary>
    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // 地球の半径 (km)
        
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        return R * c;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}
