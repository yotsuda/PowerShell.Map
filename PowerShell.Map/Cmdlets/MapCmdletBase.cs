using System.Management.Automation;
using PowerShell.Map.Server;

namespace PowerShell.Map.Cmdlets;

public abstract class MapCmdletBase : PSCmdlet
{
    protected const int BrowserConnectionWaitMs = 2000;

    protected void ExecuteWithRetry(MapServer server, Func<bool> updateAction)
    {
        // Check if browser window exists first
        bool windowExists = server.IsBrowserWindowOpen();
        WriteVerbose($"Browser window check: {(windowExists ? "exists" : "not found")}");
        
        // Try to update the map first
        bool success = updateAction();
        WriteVerbose($"First update attempt: {(success ? "succeeded" : "failed")}");

        // If window doesn't exist, open browser regardless of updateAction result
        if (!windowExists)
        {
            WriteVerbose("No browser window detected, opening browser");
            Helpers.LocationHelper.OpenBrowser(server.Url, msg => WriteWarning(msg));
            System.Threading.Thread.Sleep(BrowserConnectionWaitMs);
            success = updateAction();
            WriteVerbose($"Update after opening browser: {(success ? "succeeded" : "failed")}");
            
            if (!success)
            {
                WriteVerbose("Still no connection after opening browser, retrying once more");
                System.Threading.Thread.Sleep(BrowserConnectionWaitMs);
                success = updateAction();
                WriteVerbose($"Final retry: {(success ? "succeeded" : "failed")}");
            }
        }
        // If window exists but update failed, wait and retry
        else if (!success)
        {
            WriteVerbose("Browser window exists but SSE not connected yet, waiting...");
            System.Threading.Thread.Sleep(BrowserConnectionWaitMs);
            success = updateAction();
            WriteVerbose($"Update after waiting: {(success ? "succeeded" : "failed")}");
            
            if (!success)
            {
                WriteVerbose("Still no connection, retrying once more");
                System.Threading.Thread.Sleep(BrowserConnectionWaitMs);
                success = updateAction();
                WriteVerbose($"Final retry: {(success ? "succeeded" : "failed")}");
            }
        }
        
        if (!success)
        {
            WriteWarning("Failed to update map - browser may not have opened correctly.");
        }
        else
        {
            WriteVerbose("Map updated successfully");
        }
    }
}
