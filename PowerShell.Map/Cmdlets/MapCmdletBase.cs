using System.Management.Automation;
using PowerShell.Map.Server;

namespace PowerShell.Map.Cmdlets;

public abstract class MapCmdletBase : PSCmdlet
{
    // SSE connection configuration
    protected const int MaxSseConnectionAttempts = 5;
    protected const int SseConnectionPollIntervalMs = 1000;
    
    // Map update retry configuration
    protected const int MaxMapUpdateRetries = 3;
    protected const int MapUpdateRetryIntervalMs = 1000;

    protected void ExecuteWithRetry(MapServer server, Func<bool> updateAction)
    {
        bool windowExists = server.IsBrowserWindowOpen();
        WriteVerbose($"Browser window check: {(windowExists ? "exists" : "not found")}");
        
        // Case 1: No window - Update state FIRST, then open browser
        if (!windowExists)
        {
            WriteVerbose("No browser window found");
            
            // Update map state BEFORE opening browser
            // This ensures the initial /api/state fetch returns the correct location
            if (!updateAction())
            {
                WriteWarning("Failed to update map state");
                return;
            }
            WriteVerbose("Map state updated, opening browser...");
            
            // Now open browser - it will load the updated state from /api/state
            Helpers.LocationHelper.OpenBrowser(server.Url, msg => WriteWarning(msg));
            
            // Poll for SSE connection with short timeout intervals
            bool connected = false;
            for (int i = 0; i < MaxSseConnectionAttempts; i++)
            {
                System.Threading.Thread.Sleep(SseConnectionPollIntervalMs);
                if (server.HasActiveClients())
                {
                    connected = true;
                    WriteVerbose($"SSE connected after {i + 1} attempt(s)");
                    break;
                }
                WriteVerbose($"Waiting for SSE connection... (attempt {i + 1}/{MaxSseConnectionAttempts})");
            }
            
            if (!connected)
            {
                WriteWarning("Browser opened but SSE connection not established - map should still display correctly");
            }
            else
            {
                WriteVerbose("SSE connected successfully");
            }
            return;
        }
        
        // Case 2: Window exists - No SSE check needed, attempt update immediately
        WriteVerbose("Browser window exists, attempting update...");
        if (updateAction())
        {
            WriteVerbose("Map updated successfully");
            return;
        }
        
        // If failed, retry with short intervals
        WriteVerbose("Update failed, retrying...");
        for (int i = 0; i < MaxMapUpdateRetries; i++)
        {
            System.Threading.Thread.Sleep(MapUpdateRetryIntervalMs);
            if (updateAction())
            {
                WriteVerbose($"Map updated successfully after {i + 1} retry(ies)");
                return;
            }
            WriteVerbose($"Retry {i + 1}/{MaxMapUpdateRetries} failed");
        }
        
        WriteWarning("Failed to update map - please refresh the browser tab");
    }

    /// <summary>
    /// 色名または#rgb形式を16進数カラーコードに変換
    /// nullの場合はnullを返す（デフォルトの涙型マーカーを使用）
    /// </summary>
    protected static string? GetMarkerColor(string? color)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            return null; // No color = use default teardrop marker
        }

        // #rgb形式ならそのまま返す
        if (color.StartsWith("#"))
        {
            return color;
        }

        // 色名を変換 (Bootstrap colors + common web colors)
        return color.ToLower() switch
        {
            // Bootstrap theme colors
            "red" => "#dc3545",
            "blue" => "#0d6efd",
            "green" => "#198754",
            "orange" => "#fd7e14",
            "yellow" => "#ffc107",
            "violet" or "purple" => "#6f42c1",
            "indigo" => "#6610f2",
            "pink" => "#d63384",
            "cyan" => "#0dcaf0",
            "teal" => "#20c997",
            
            // Grayscale
            "black" => "#212529",
            "grey" or "gray" => "#6c757d",
            "white" => "#ffffff",
            "silver" => "#c0c0c0",
            
            // Common web colors
            "darkred" => "#8b0000",
            "darkgreen" => "#006400",
            "darkblue" => "#00008b",
            "lightred" => "#ff6b6b",
            "lightgreen" => "#90ee90",
            "lightblue" => "#add8e6",
            "navy" => "#000080",
            "lime" => "#00ff00",
            "magenta" => "#ff00ff",
            "maroon" => "#800000",
            "olive" => "#808000",
            "brown" => "#a52a2a",
            "gold" => "#ffd700",
            "crimson" => "#dc143c",
            "coral" => "#ff7f50",
            "turquoise" => "#40e0d0",
            "skyblue" => "#87ceeb",
            "lavender" => "#e6e6fa",
            "plum" => "#dda0dd",
            "salmon" => "#fa8072",
            "khaki" => "#f0e68c",
            
            _ => null // Unknown color = use default teardrop marker
        };
    }

    /// <summary>
    /// Convert Enable3D/Disable3D switches to nullable bool with validation
    /// </summary>
    /// <param name="enable3D">Enable3D switch parameter value</param>
    /// <param name="disable3D">Disable3D switch parameter value</param>
    /// <param name="result">Resulting bool? value (true/false/null)</param>
    /// <returns>True if parameters are valid, false if both are specified (error written)</returns>
    protected bool TryGetEnable3DParameter(bool enable3D, bool disable3D, out bool? result)
    {
        if (enable3D && disable3D)
        {
            WriteError(new ErrorRecord(
                new ArgumentException("Cannot specify both -Enable3D and -Disable3D"),
                "MutuallyExclusiveParameters",
                ErrorCategory.InvalidArgument,
                null));
            result = null;
            return false;
        }
        
        result = enable3D ? true : disable3D ? false : null;
        return true;
    }
}
