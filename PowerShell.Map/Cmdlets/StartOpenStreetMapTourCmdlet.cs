using System.Management.Automation;
using PowerShell.Map.Helpers;
using PowerShell.Map.Server;

namespace PowerShell.Map.Cmdlets;

/// <summary>
/// Creates an animated tour that visits multiple locations sequentially
/// </summary>
[Cmdlet(VerbsLifecycle.Start, "OpenStreetMapTour")]
[OutputType(typeof(MapMarker))]
public class StartOpenStreetMapTourCmdlet : MapCmdletBase
{
    /// <summary>
    /// Array of locations to visit in the tour (place names or coordinates)
    /// </summary>
    [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
    public string[]? Location { get; set; }

    /// <summary>
    /// Zoom level for each location (1-19, default: 13)
    /// </summary>
    [Parameter]
    [ValidateRange(1, 19)]
    public int Zoom { get; set; } = 13;

    /// <summary>
    /// Time to pause at each location in seconds (0.5-30.0, default: 2.0)
    /// </summary>
    [Parameter]
    [ValidateRange(0.5, 30.0)]
    public double PauseTime { get; set; } = 2.0;

    /// <summary>
    /// Animation duration for transitions between locations in seconds (0.1-10.0, default: 1.5)
    /// </summary>
    [Parameter]
    [ValidateRange(0.1, 10.0)]
    public double Duration { get; set; } = 1.5;

    // /// <summary>
    // /// Enable debug mode to show detailed logging
    // /// </summary>
    // [Parameter]
    private SwitchParameter DebugMode { get; set; }

    private readonly List<string> _allLocations = new();

    protected override void ProcessRecord()
    {
        if (Location != null)
        {
            _allLocations.AddRange(Location);
        }
    }

    protected override void EndProcessing()
    {
        if (_allLocations.Count == 0)
        {
            WriteWarning("No locations provided for tour");
            return;
        }

        try
        {
            var server = MapServer.Instance;
            
            WriteVerbose($"Starting map tour with {_allLocations.Count} locations");
            
            for (int i = 0; i < _allLocations.Count; i++)
            {
                var location = _allLocations[i];
                WriteVerbose($"[{i + 1}/{_allLocations.Count}] Visiting: {location}");

                if (!LocationHelper.TryParseLocation(location, out double lat, out double lon,
                    msg => WriteVerbose(msg), msg => WriteWarning(msg)))
                {
                    WriteWarning($"Could not parse location: {location}, skipping");
                    
                    // Output failed marker
                    WriteObject(new MapMarker
                    {
                        Step = i + 1,
                        TotalSteps = _allLocations.Count,
                        Location = location,
                        Status = "Failed",
                        GeocodingSource = "Unknown"
                    });
                    continue;
                }

                string label = location;
                if (location.Contains(','))
                {
                    // Try reverse geocoding for coordinates
                    if (LocationHelper.TryReverseGeocode(lat, lon, out string? reversedName, 
                        msg => WriteVerbose(msg)))
                    {
                        label = reversedName!;
                    }
                }

                // Move to location with animation (always animated in tour mode)
                ExecuteWithRetry(server, () => server.UpdateMap(lat, lon, Zoom, label, DebugMode, Duration));
                
                // Output progress info
                WriteObject(new MapMarker
                {
                    Step = i + 1,
                    TotalSteps = _allLocations.Count,
                    Location = location,
                    Latitude = lat,
                    Longitude = lon,
                    Label = label,
                    Status = "Success",
                    GeocodingSource = location.Contains(',') ? "Coordinates" : "Nominatim"
                });
                
                // Wait at location (transition + pause time)
                var totalWaitMs = (int)((Duration + PauseTime) * 1000);
                System.Threading.Thread.Sleep(totalWaitMs);
            }

            WriteVerbose("Tour completed successfully");
        }
        catch (Exception ex)
        {
            WriteError(new ErrorRecord(
                ex,
                "TourFailed",
                ErrorCategory.NotSpecified,
                null));
        }
    }
}

