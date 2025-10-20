namespace PowerShell.Map.Server;

/// <summary>
/// Map view modes for OpenStreetMap display
/// </summary>
public enum MapView
{
    /// <summary>
    /// Standard 2D map view (default)
    /// </summary>
    Default,
    
    /// <summary>
    /// 3D view with buildings and terrain
    /// </summary>
    Map3D,
    
    /// <summary>
    /// Dark theme map
    /// </summary>
    Dark,
    
    /// <summary>
    /// Outdoor/Topographic map
    /// </summary>
    Outdoors,
    
    /// <summary>
    /// Cycling routes emphasized
    /// </summary>
    Cycling,
    
    /// <summary>
    /// Public transit emphasized
    /// </summary>
    Transit
}
