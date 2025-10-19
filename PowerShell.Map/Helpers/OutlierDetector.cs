using PowerShell.Map.Server;

namespace PowerShell.Map.Helpers;

/// <summary>
/// Detects outlier markers that are too far from the main cluster
/// </summary>
public static class OutlierDetector
{
    private const double WidelySpacedMarkersThresholdKm = 1000;
    private const int OutlierDistanceMultiplier = 5;
    private const double MinimumOutlierThresholdKm = 500;

    /// <summary>
    /// Detects outliers from a set of markers based on distance from median
    /// </summary>
    /// <param name="markers">Array of markers with valid coordinates</param>
    /// <param name="writeVerbose">Optional callback for verbose messages</param>
    /// <param name="writeWarning">Optional callback for warning messages</param>
    /// <returns>Tuple of valid markers and outlier markers</returns>
    public static (MapMarker[] valid, MapMarker[] outliers) DetectOutliers(
        MapMarker[] markers,
        Action<string>? writeVerbose = null,
        Action<string>? writeWarning = null)
    {
        if (markers.Length < 2)
        {
            return (markers, Array.Empty<MapMarker>());
        }

        // Calculate minimum distance between any two markers (cluster density check)
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

        // Special handling for widely spaced markers
        // If even the closest markers are more than 1000km apart
        if (minDistance > WidelySpacedMarkersThresholdKm)
        {
            writeWarning?.Invoke($"Markers are very far apart (minimum distance: {minDistance:F0}km). Only the first marker will be displayed on the map. Please verify your input data.");
            // Return only the first marker as valid, rest as outliers
            var firstMarker = new[] { markers[0] };
            var restMarkers = markers.Skip(1).ToArray();
            return (firstMarker, restMarkers);
        }

        // Calculate median coordinates (all markers here have valid coordinates)
        var latitudes = markers.Select(m => m.Latitude!.Value).OrderBy(x => x).ToArray();
        var longitudes = markers.Select(m => m.Longitude!.Value).OrderBy(x => x).ToArray();
        
        double medianLat = latitudes.Length % 2 == 0
            ? (latitudes[latitudes.Length / 2 - 1] + latitudes[latitudes.Length / 2]) / 2
            : latitudes[latitudes.Length / 2];
            
        double medianLon = longitudes.Length % 2 == 0
            ? (longitudes[longitudes.Length / 2 - 1] + longitudes[longitudes.Length / 2]) / 2
            : longitudes[longitudes.Length / 2];

        // Calculate distance from median for each marker
        var markerDistances = markers.Select(m => new
        {
            Marker = m,
            Distance = CalculateDistance(medianLat, medianLon, m.Latitude!.Value, m.Longitude!.Value)
        }).ToArray();

        // Calculate median of distances
        var distances = markerDistances.Select(md => md.Distance).OrderBy(d => d).ToArray();
        double medianDistance = distances.Length % 2 == 0
            ? (distances[distances.Length / 2 - 1] + distances[distances.Length / 2]) / 2
            : distances[distances.Length / 2];

        // Threshold: 5x median distance, or minimum 500km
        double threshold = Math.Max(medianDistance * OutlierDistanceMultiplier, MinimumOutlierThresholdKm);

        // Detect outliers
        var validMarkers = new List<MapMarker>();
        var outlierMarkers = new List<MapMarker>();

        foreach (var md in markerDistances)
        {
            if (md.Distance > threshold)
            {
                outlierMarkers.Add(md.Marker);
                writeVerbose?.Invoke($"Outlier detected: {md.Marker.Location} (distance: {md.Distance:F1}km from median, threshold: {threshold:F1}km)");
            }
            else
            {
                validMarkers.Add(md.Marker);
            }
        }

        return (validMarkers.ToArray(), outlierMarkers.ToArray());
    }

    /// <summary>
    /// Calculates distance between two points using Haversine formula (in km)
    /// </summary>
    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double EarthRadiusKm = 6371;
        
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        return EarthRadiusKm * c;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}
