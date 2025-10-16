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
                
                StartServerAndOpenBrowser(server);
                server.UpdateMapWithMarkers(markerList.ToArray(), Zoom, DebugMode);
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
                if (!server.IsRunning)
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException("Location must be specified when starting the map for the first time."),
                        "NoLocation",
                        ErrorCategory.InvalidOperation,
                        null));
                    return;
                }
                
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

            StartServerAndOpenBrowser(server);
            server.UpdateMap(lat, lon, zoom, marker, DebugMode);
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

    private void StartServerAndOpenBrowser(MapServer server)
    {
        if (!server.IsRunning)
        {
            server.Start();
            WriteVerbose("Map server started");
            
            server.NotifyBrowserOpened();
            LocationHelper.OpenBrowser(server.Url, msg => WriteWarning(msg));
            WriteVerbose($"Browser opened at {server.Url}");
            
            System.Threading.Thread.Sleep(1000);
        }
        else if (!server.HasConnectedClients)
        {
            server.NotifyBrowserOpened();
            LocationHelper.OpenBrowser(server.Url, msg => WriteWarning(msg));
            WriteVerbose("No SSE clients connected, opened new browser tab");
            
            System.Threading.Thread.Sleep(1000);
        }
        else
        {
            WriteVerbose("SSE clients already connected, skipping browser open");
        }
    }
}