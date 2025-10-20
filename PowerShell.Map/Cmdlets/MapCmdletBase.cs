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
    /// </summary>
    protected string GetMarkerColor(string? color)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            return "#dc3545"; // Default: red
        }

        // #rgb形式ならそのまま返す
        if (color.StartsWith("#"))
        {
            return color;
        }

        // 色名を変換
        return color.ToLower() switch
        {
            "red" => "#dc3545",
            "blue" => "#0d6efd",
            "green" => "#198754",
            "orange" => "#fd7e14",
            "violet" => "#6f42c1",
            "yellow" => "#ffc107",
            "grey" => "#6c757d",
            "black" => "#212529",
            "gold" => "#ffd700",
            _ => "#dc3545" // Unknown color defaults to red
        };
    }
}
