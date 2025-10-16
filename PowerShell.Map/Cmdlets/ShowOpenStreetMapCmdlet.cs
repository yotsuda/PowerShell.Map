using System.Collections;
using System.Management.Automation;
using PowerShell.Map.Helpers;
using PowerShell.Map.Server;

namespace PowerShell.Map.Cmdlets;

[Cmdlet(VerbsCommon.Show, "OpenStreetMap")]
public class ShowOpenStreetMapCmdlet : PSCmdlet
{
    [Parameter(Position = 0)]
    public string? Location { get; set; }

    [Parameter]
    [ValidateRange(1, 19)]
    public int? Zoom { get; set; }

    [Parameter]
    public string? Marker { get; set; }

    [Parameter(ValueFromPipeline = true)]
    public Hashtable[]? Markers { get; set; }

    [Parameter]
    public SwitchParameter DebugMode { get; set; }

    private const int BrowserConnectionWaitMs = 2000;

    protected override void ProcessRecord()
    {
        try
        {
            var server = MapServer.Instance;

            // Handle multiple markers
            if (Markers != null && Markers.Length > 0)
            {
                var markerList = new List<MapMarker>();

                foreach (var markerHash in Markers)
                {
                    var location = markerHash["Location"]?.ToString();
                    var label = markerHash["Label"]?.ToString();
                    var color = markerHash["Color"]?.ToString();

                    if (string.IsNullOrEmpty(location))
                    {
                        WriteWarning("Marker without Location property, skipping");
                        continue;
                    }

                    if (!LocationHelper.TryParseLocation(location, out double markerLat, out double markerLon,
                        msg => WriteVerbose(msg), msg => WriteWarning(msg)))
                    {
                        WriteWarning($"Could not parse location: {location}, skipping");
                        continue;
                    }

                    markerList.Add(new MapMarker
                    {
                        Latitude = markerLat,
                        Longitude = markerLon,
                        Label = label,
                        Color = color
                    });

                    WriteVerbose($"Added marker: {label ?? location} at {markerLat}, {markerLon}");
                }

                if (markerList.Count == 0)
                {
                    WriteError(new ErrorRecord(
                        new ArgumentException("No valid markers provided"),
                        "NoValidMarkers",
                        ErrorCategory.InvalidArgument,
                        Markers));
                    return;
                }

                var markers = markerList.ToArray();
                ExecuteWithRetry(server, () => server.UpdateMapWithMarkers(markers, Zoom, DebugMode));
                WriteVerbose($"Map updated with {markerList.Count} markers");
                return;
            }

            // Handle single location
            double lat, lon;
            int zoom;
            string? marker = Marker;

            if (!string.IsNullOrEmpty(Location))
            {
                if (!LocationHelper.TryParseLocation(Location!, out lat, out lon,
                    msg => WriteVerbose(msg), msg => WriteWarning(msg)))
                {
                    WriteError(new ErrorRecord(
                        new ArgumentException($"Invalid location format: {Location}. Use 'latitude,longitude' format or a place name."),
                        "InvalidLocation",
                        ErrorCategory.InvalidArgument,
                        Location));
                    return;
                }

                zoom = Zoom ?? 13;
            }
            else
            {

                var currentState = server.GetCurrentState();
                lat = currentState.Latitude;
                lon = currentState.Longitude;
                zoom = Zoom ?? currentState.Zoom;

                if (marker == null)
                {
                    marker = currentState.Marker;
                }

                WriteVerbose($"Using current location: {lat}, {lon}");
            }
            ExecuteWithRetry(server, () => server.UpdateMap(lat, lon, zoom, marker, DebugMode));
            WriteVerbose($"Map updated: {lat}, {lon} @ zoom {zoom}");
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

    private void ExecuteWithRetry(MapServer server, Func<bool> updateAction)
    {
        // Check if browser window exists first
        bool windowExists = server.IsBrowserWindowOpen();
        WriteVerbose($"Browser window check: {(windowExists ? "exists" : "not found")}");
        
        // Try to update the map first
        bool success = updateAction();
        WriteVerbose($"First update attempt: {(success ? "succeeded" : "failed")}");

        // If window doesn't exist, open browser regardless of updateAction result
        if (!windowExists)
        {
            WriteVerbose("No browser window detected, opening browser");
            LocationHelper.OpenBrowser(server.Url, msg => WriteWarning(msg));
            System.Threading.Thread.Sleep(BrowserConnectionWaitMs);
            success = updateAction();
            WriteVerbose($"Update after opening browser: {(success ? "succeeded" : "failed")}");
            
            if (!success)
            {
                WriteVerbose("Still no connection after opening browser, retrying once more");
                System.Threading.Thread.Sleep(BrowserConnectionWaitMs);
                success = updateAction();
                WriteVerbose($"Final retry: {(success ? "succeeded" : "failed")}");
            }
        }
        // If window exists but update failed, wait and retry
        else if (!success)
        {
            WriteVerbose("Browser window exists but SSE not connected yet, waiting...");
            System.Threading.Thread.Sleep(BrowserConnectionWaitMs);
            success = updateAction();
            WriteVerbose($"Update after waiting: {(success ? "succeeded" : "failed")}");
            
            if (!success)
            {
                WriteVerbose("Still no connection, retrying once more");
                System.Threading.Thread.Sleep(BrowserConnectionWaitMs);
                success = updateAction();
                WriteVerbose($"Final retry: {(success ? "succeeded" : "failed")}");
            }
        }
        
        if (!success)
        {
            WriteWarning("Failed to update map - browser may not have opened correctly.");
        }
        else
        {
            WriteVerbose("Map updated successfully");
        }
    }
}
