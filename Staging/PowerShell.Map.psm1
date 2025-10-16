# PowerShell.Map Module

# Import the binary module
Import-Module "$PSScriptRoot\PowerShell.Map.dll"

# Module cleanup - stop server when module is removed
$MyInvocation.MyCommand.ScriptBlock.Module.OnRemove = {
    $server = [PowerShell.Map.Server.MapServer]::Instance
    $server.Stop()
    Write-Verbose "Map server stopped during module cleanup"
}

# Export all cmdlets from the binary module
Export-ModuleMember -Cmdlet * -Function * -Alias *