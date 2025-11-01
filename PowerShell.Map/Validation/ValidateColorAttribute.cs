using System;
using System.Management.Automation;

namespace PowerShell.Map.Validation;

/// <summary>
/// Validates that a color parameter is either a hex code (#rrggbb or #rgb) or a supported color name
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ValidateColorAttribute : ValidateArgumentsAttribute
{
    private static readonly string[] SupportedColors = 
    {
        // Bootstrap theme colors
        "red", "blue", "green", "orange", "yellow", "violet", "purple",
        "indigo", "pink", "cyan", "teal",
        
        // Grayscale
        "black", "grey", "gray", "white", "silver",
        
        // Common web colors
        "darkred", "darkgreen", "darkblue", "lightred", "lightgreen", "lightblue",
        "navy", "lime", "magenta", "maroon", "olive", "brown", "gold",
        "crimson", "coral", "turquoise", "skyblue", "lavender", "plum", "salmon", "khaki"
    };

    protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
    {
        if (arguments == null || string.IsNullOrWhiteSpace(arguments.ToString()))
        {
            return; // null or empty is allowed (uses default)
        }

        string color = arguments.ToString()!;

        // Check if it's a hex color code
        if (color.StartsWith('#'))
        {
            // Valid formats: #rgb or #rrggbb
            if (color.Length == 4 || color.Length == 7)
            {
                // Verify all characters after # are hex digits
                for (int i = 1; i < color.Length; i++)
                {
                    if (!Uri.IsHexDigit(color[i]))
                    {
                        throw new ValidationMetadataException(
                            $"Invalid hex color code '{color}'. Use format #rgb or #rrggbb with valid hex digits (0-9, A-F).");
                    }
                }
                return; // Valid hex code
            }
            else
            {
                throw new ValidationMetadataException(
                    $"Invalid hex color code '{color}'. Use format #rgb (4 characters) or #rrggbb (7 characters).");
            }
        }

        // Check if it's a supported color name
        foreach (var supportedColor in SupportedColors)
        {
            if (string.Equals(color, supportedColor, StringComparison.OrdinalIgnoreCase))
            {
                return; // Valid color name
            }
        }

        // Not a valid hex code or color name
        var colorList = string.Join(", ", SupportedColors);
        throw new ValidationMetadataException(
            $"Invalid color '{color}'. Use a hex code (e.g., #FF5733) or one of the supported color names: {colorList}");
    }
}
