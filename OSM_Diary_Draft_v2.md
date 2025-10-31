Subject: New PowerShell Module for OpenStreetMap Visualization

# Introducing PowerShell.Map v1.0.0

I'm excited to share PowerShell.Map - a new module that brings interactive OpenStreetMap visualization to the PowerShell command line!

## What it does

PowerShell.Map opens interactive maps in your browser directly from PowerShell commands:

~~~powershell
# Display multiple locations
Show-OpenStreetMap Paris, London, Rome

# Show routes with turn-by-turn directions
Show-OpenStreetMapRoute -From Tokyo -To Osaka

# Create animated tours
Start-OpenStreetMapTour Tokyo, Kyoto, Osaka -Duration 2
~~~

## Use cases

PowerShell.Map brings OSM's powerful features to the command line:

- **Zero setup**: Interactive maps with a single command - no web development needed
- **Scriptable**: Automate map generation from CSV, databases, or any data source
- **AI-powered**: Let Claude Desktop create maps via natural language

## Technical stack

This project is built entirely on OpenStreetMap's ecosystem:

- **Leaflet.js** for interactive map display
- **OSM tiles** for base maps
- **Nominatim** for geocoding
- **OSRM** for routing and directions
- **Built-in HTTP server** for seamless browser integration

Thank you to the OSM community for providing these excellent services!

### AI integration

PowerShell.Map integrates with Claude Desktop via [PowerShell.MCP](https://github.com/yotsuda/PowerShell.MCP), enabling natural language map visualization. 

First, tell Claude to set up the module:

**"In PowerShell console, run Import-Module PowerShell.Map and check how to use it."**

Then simply ask for what you want:

- **"Show me a tour of famous temples in Kyoto"**
- **"Create a route from Tokyo Tower to Senso-ji Temple"**
- **"Show me a walking tour of cafes in Paris"**
- **"Plan a hot spring tour across Japan with routes between locations"**

Claude translates these requests into PowerShell.Map commands and displays the interactive map instantly.

**Links:**

- **PowerShell Gallery**: https://www.powershellgallery.com/packages/PowerShell.MCP
- **GitHub**: https://github.com/yotsuda/PowerShell.MCP

### Installation

Requires PowerShell 7.2+: https://learn.microsoft.com/powershell/scripting/install/installing-powershell

~~~powershell
Install-Module PowerShell.Map
Import-Module PowerShell.Map
Show-OpenStreetMap "Your City"
~~~

## Links

- **GitHub**: https://github.com/yotsuda/PowerShell.Map
- **PowerShell Gallery**: https://www.powershellgallery.com/packages/PowerShell.Map
- **License**: MIT

## Attribution

Uses OpenStreetMap data © OpenStreetMap contributors  
https://www.openstreetmap.org/copyright

Feedback, issues, and contributions are welcome on GitHub!
