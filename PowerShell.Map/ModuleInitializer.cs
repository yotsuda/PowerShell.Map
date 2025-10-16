using System.Management.Automation;

namespace PowerShell.Map;

public class ModuleInitializer : IModuleAssemblyInitializer
{
    public void OnImport()
    {
        // Start the map server when module is imported
        var server = Server.MapServer.Instance;
        server.Start();
    }
}
