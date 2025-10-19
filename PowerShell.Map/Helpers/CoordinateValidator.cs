namespace PowerShell.Map.Helpers;

/// <summary>
/// Provides validation methods for coordinate strings
/// </summary>
public static class CoordinateValidator
{
    /// <summary>
    /// Checks if the input string is in "latitude,longitude" coordinate format
    /// </summary>
    /// <param name="input">Input string to check</param>
    /// <returns>True if the string is a valid coordinate format</returns>
    public static bool IsCoordinateString(string input)
    {
        if (string.IsNullOrWhiteSpace(input) || !input.Contains(','))
            return false;
            
        var parts = input.Split(',');
        return parts.Length == 2 &&
               double.TryParse(parts[0].Trim(), out _) &&
               double.TryParse(parts[1].Trim(), out _);
    }
}