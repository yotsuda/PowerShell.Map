namespace PowerShell.Map.Server;

/// <summary>
/// Represents a location in a tour with optional description
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
}