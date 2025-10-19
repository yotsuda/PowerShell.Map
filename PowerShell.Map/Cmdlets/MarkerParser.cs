using System.Collections;
using System.Management.Automation;
using PowerShell.Map.Server;

namespace PowerShell.Map.Helpers;

/// <summary>
/// Parses various marker input formats into MapMarker objects
/// </summary>
public static class MarkerParser
{
    /// <summary>
    /// Parses an array of markers from various formats (string, Hashtable, MapMarker, PSObject)
    /// </summary>
    /// <param name="markers">Array of marker objects to parse</param>
    /// <param name="colorResolver">Function to resolve color names to hex codes</param>
    /// <param name="writeVerbose">Optional callback for verbose messages</param>
    /// <param name="writeWarning">Optional callback for warning messages</param>
    /// <returns>List of parsed MapMarker objects</returns>
    public static List<MapMarker> ParseMarkers(
        object[] markers,
        Func<string?, string> colorResolver,
        Action<string>? writeVerbose = null,
        Action<string>? writeWarning = null)
    {
        var markerList = new List<MapMarker>();

        foreach (var markerObj in markers)
        {
            MapMarker? parsedMarker = null;

            // String format (simplest)
            if (markerObj is string locationStr)
            {
                parsedMarker = ParseStringMarker(locationStr, colorResolver, writeVerbose, writeWarning);
            }
            // MapMarker object (type-safe)
            else if (markerObj is MapMarker mapMarker)
            {
                parsedMarker = mapMarker;
            }
            // Hashtable format (detailed control)
            else if (markerObj is Hashtable markerHash)
            {
                parsedMarker = ParseHashtableMarker(markerHash, colorResolver, writeVerbose, writeWarning);
            }
            // PSObject format (from pipeline with custom objects)
            else if (markerObj is PSObject psObj)
            {
                parsedMarker = ParsePSObjectMarker(psObj, colorResolver, writeVerbose, writeWarning);
            }
            else
            {
                writeWarning?.Invoke($"Unsupported marker type: {markerObj?.GetType().Name}, skipping");
                continue;
            }

            if (parsedMarker != null)
            {
                markerList.Add(parsedMarker);
                writeVerbose?.Invoke($"Added marker: {parsedMarker.Label ?? $"{parsedMarker.Latitude},{parsedMarker.Longitude}"} at {parsedMarker.Latitude}, {parsedMarker.Longitude}");
            }
        }

        return markerList;
    }

    /// <summary>
    /// Parses marker from string format: "Location" or "Location|Label" or "Location|Label|Color"
    /// </summary>
    private static MapMarker? ParseStringMarker(
        string locationStr,
        Func<string?, string> colorResolver,
        Action<string>? writeVerbose,
        Action<string>? writeWarning)
    {
        var parts = locationStr.Split('|');
        var location = parts[0].Trim();
        var label = parts.Length > 1 ? parts[1].Trim() : null;
        var color = parts.Length > 2 ? parts[2].Trim() : null;

        if (string.IsNullOrEmpty(location))
        {
            writeWarning?.Invoke("Empty location string, skipping");
            return null;
        }

        // Check if coordinate string
        bool isCoordinates = CoordinateValidator.IsCoordinateString(location);

        if (!LocationHelper.TryParseLocation(location, out double markerLat, out double markerLon,
            writeVerbose, writeWarning))
        {
            writeWarning?.Invoke($"Could not parse location: {location}, skipping");
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
            Color = colorResolver(color),
            Location = location,
            Status = "Success",
            GeocodingSource = isCoordinates ? "Coordinates" : "Nominatim"
        };
    }

    /// <summary>
    /// Parses marker from Hashtable format
    /// </summary>
    private static MapMarker? ParseHashtableMarker(
        Hashtable markerHash,
        Func<string?, string> colorResolver,
        Action<string>? writeVerbose,
        Action<string>? writeWarning)
    {
        var location = markerHash["Location"]?.ToString();
        var label = markerHash["Label"]?.ToString();
        var color = markerHash["Color"]?.ToString();

        if (string.IsNullOrEmpty(location))
        {
            writeWarning?.Invoke("Marker without Location property, skipping");
            return null;
        }

        // Check if coordinate string
        bool isCoordinates = CoordinateValidator.IsCoordinateString(location);

        if (!LocationHelper.TryParseLocation(location, out double markerLat, out double markerLon,
            writeVerbose, writeWarning))
        {
            writeWarning?.Invoke($"Could not parse location: {location}, skipping");
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
            Color = colorResolver(color),
            Location = location,
            Status = "Success",
            GeocodingSource = isCoordinates ? "Coordinates" : "Nominatim"
        };
    }

    /// <summary>
    /// Parses marker from PSObject format
    /// </summary>
    private static MapMarker? ParsePSObjectMarker(
        PSObject psObj,
        Func<string?, string> colorResolver,
        Action<string>? writeVerbose,
        Action<string>? writeWarning)
    {
        var location = psObj.Properties["Location"]?.Value?.ToString();
        var label = psObj.Properties["Label"]?.Value?.ToString();
        var color = psObj.Properties["Color"]?.Value?.ToString();

        // If Location property exists
        if (!string.IsNullOrEmpty(location))
        {
            if (!LocationHelper.TryParseLocation(location, out double markerLat, out double markerLon,
                writeVerbose, writeWarning))
            {
                writeWarning?.Invoke($"Could not parse location: {location}, skipping");
                return null;
            }

            return new MapMarker
            {
                Latitude = markerLat,
                Longitude = markerLon,
                Label = label,
                Color = colorResolver(color)
            };
        }

        // Use Latitude/Longitude properties directly (for CSV pipeline)
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
                    Color = colorResolver(color)
                };
            }
            else
            {
                writeWarning?.Invoke($"Could not parse Latitude/Longitude: {latStr}, {lonStr}, skipping");
                return null;
            }
        }

        writeWarning?.Invoke("Marker without Location or Latitude/Longitude properties, skipping");
        return null;
    }
}