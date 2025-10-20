namespace PowerShell.Map.Server;

/// <summary>
/// Represents a location with optional metadata (description, label, color)
/// Used across multiple cmdlets for consistent location specification
/// </summary>
public class TourLocation
{
    /// <summary>
    /// Location name or coordinates (e.g., "Tokyo" or "35.6762,139.6503")
    /// </summary>
    public string Location { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional description to display when visiting this location
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Optional label for the marker (defaults to location name or reverse geocoded name)
    /// </summary>
    public string? Label { get; set; }
    
    /// <summary>
    /// Optional marker color (color name or hex code)
    /// </summary>
    public string? Color { get; set; }
}