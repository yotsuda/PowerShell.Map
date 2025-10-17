# PowerShell.Map Module
# This script module is loaded as a nested module for cleanup purposes only.
# The binary module (PowerShell.Map.dll) is loaded as RootModule.

# Module cleanup - stop server when module is removed
$MyInvocation.MyCommand.ScriptBlock.Module.OnRemove = {
    # Suppress all error messages during cleanup
    $ErrorActionPreference = 'SilentlyContinue'
    
    try {
        $server = [PowerShell.Map.Server.MapServer]::Instance
        $server.Stop()
        Write-Verbose "Map server stopped during module cleanup"
    }
    catch [System.ObjectDisposedException] {
        # HttpListener already disposed - this is expected when called multiple times
        Write-Verbose "Map server already stopped (ObjectDisposedException caught)"
    }
    catch {
        # Silently ignore all other errors during cleanup
        # No warning message to keep output clean
    }
    finally {
        # Clear any errors that may have been added to $Error
        $Error.Clear()
    }
}
