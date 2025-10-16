using System.Management.Automation;
using PowerShell.Map.Server;

namespace PowerShell.Map.Cmdlets;

public abstract class MapCmdletBase : PSCmdlet
{
    protected const int BrowserConnectionWaitMs = 2000;

    protected void ExecuteWithRetry(MapServer server, Func<bool> updateAction)
    {
        bool windowExists = server.IsBrowserWindowOpen();
        WriteVerbose($"Browser window check: {(windowExists ? "exists" : "not found")}");
        
        // Case 1: No window - Open new tab and poll for SSE connection
        if (!windowExists)
        {
            WriteVerbose("Opening new browser tab...");
            Helpers.LocationHelper.OpenBrowser(server.Url, msg => WriteWarning(msg));
            
            // Poll for SSE connection with short timeout intervals
            bool connected = false;
            const int maxAttempts = 5;
            for (int i = 0; i < maxAttempts; i++)
            {
                System.Threading.Thread.Sleep(1000); // Check every 1 second
                if (server.HasActiveClients())
                {
                    connected = true;
                    WriteVerbose($"SSE connected after {i + 1} attempt(s)");
                    break;
                }
                WriteVerbose($"Waiting for SSE connection... (attempt {i + 1}/{maxAttempts})");
            }
            
            if (!connected)
            {
                WriteWarning("Failed to establish SSE connection - browser may not have opened correctly");
                return;
            }
            
            // After SSE connection established, update the map
            if (!updateAction())
            {
                WriteWarning("Failed to update map despite SSE connection");
            }
            else
            {
                WriteVerbose("Map updated successfully");
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
        const int maxRetries = 3;
        for (int i = 0; i < maxRetries; i++)
        {
            System.Threading.Thread.Sleep(1000);
            if (updateAction())
            {
                WriteVerbose($"Map updated successfully after {i + 1} retry(ies)");
                return;
            }
            WriteVerbose($"Retry {i + 1}/{maxRetries} failed");
        }
        
        WriteWarning("Failed to update map - please refresh the browser tab");
    }
}
