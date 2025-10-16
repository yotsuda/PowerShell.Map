using System.Collections;
using System.Management.Automation;
using PowerShell.Map.Helpers;
using PowerShell.Map.Server;

namespace PowerShell.Map.Cmdlets;

[Cmdlet(VerbsCommon.Show, "OpenStreetMap")]
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
                        Color = Color
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
                        if (!LocationHelper.TryParseLocation(loc, out double markerLat, out double markerLon,
                            msg => WriteVerbose(msg), msg => WriteWarning(msg)))
                        {
                            WriteWarning($"Could not parse location: {loc}, skipping");
                            continue;
                        }

                        markerList.Add(new MapMarker
                        {
                            Latitude = markerLat,
                            Longitude = markerLon,
                            Label = null,
                            Color = null
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

                    ExecuteWithRetry(server, () => server.UpdateMapWithMarkers(markerList.ToArray(), Zoom, DebugMode));
                    WriteVerbose($"Map updated with {markerList.Count} markers");
                    return;
                }

                // 単一の場所が指定された場合
                if (!LocationHelper.TryParseLocation(Location[0], out double lat, out double lon,
                    msg => WriteVerbose(msg), msg => WriteWarning(msg)))
                {
                    WriteError(new ErrorRecord(
                        new ArgumentException($"Invalid location format: {Location[0]}. Use 'latitude,longitude' format or a place name."),
                        "InvalidLocation",
                        ErrorCategory.InvalidArgument,
                        Location[0]));
                    return;
                }

                int zoom = Zoom ?? 13;
                ExecuteWithRetry(server, () => server.UpdateMap(lat, lon, zoom, Marker, DebugMode));
                WriteVerbose($"Map updated: {lat}, {lon} @ zoom {zoom}");
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
                ExecuteWithRetry(server, () => server.UpdateMap(lat, lon, zoom, marker, DebugMode));
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
                ExecuteWithRetry(server, () => server.UpdateMapWithMarkers(_pipelineMarkers.ToArray(), Zoom, DebugMode));
                WriteVerbose($"Map updated with {_pipelineMarkers.Count} markers from pipeline");
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
            Color = color
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
            Color = color
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
                Color = color
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
                    Color = color
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
}
