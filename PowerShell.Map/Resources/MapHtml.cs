namespace PowerShell.Map.Resources;

public static class MapHtml
{
    public const string Template = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>PowerShell.Map</title>
    <link rel=""stylesheet"" href=""https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"" />
    <script src=""https://unpkg.com/leaflet@1.9.4/dist/leaflet.js""></script>
    <style>
        body { margin: 0; padding: 0; }
        #map { height: 100vh; width: 100vw; }
        .update-indicator {
            position: absolute;
            top: 10px;
            right: 10px;
            background: rgba(255, 255, 255, 0.9);
            padding: 8px 12px;
            border-radius: 4px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.2);
            z-index: 1000;
            font-family: sans-serif;
            font-size: 12px;
        }
        .debug-log {
            position: absolute;
            bottom: 10px;
            left: 10px;
            background: rgba(0, 0, 0, 0.8);
            color: #0f0;
            padding: 10px;
            border-radius: 4px;
            z-index: 1000;
            font-family: monospace;
            font-size: 11px;
            max-width: 400px;
            max-height: 200px;
            overflow-y: auto;
            display: none;
        }
        .debug-log.visible {
            display: block;
        }
    </style>
</head>
<body>
    <div id=""map""></div>
    <div class=""update-indicator"">🗺️ PowerShell.Map</div>
    <div class=""debug-log"" id=""debug""></div>
    
    <script>
        let map = null;
        let markers = [];
        let routeLayer = null;
        let lastState = null;
        let eventSource = null;
        let debugMode = false;

        // Color mapping for circle markers
        function getMarkerColor(color) {
            const colors = {
                'red': '#dc3545',
                'blue': '#0d6efd',
                'green': '#198754',
                'orange': '#fd7e14',
                'violet': '#6f42c1',
                'yellow': '#ffc107',
                'grey': '#6c757d',
                'black': '#212529',
                'gold': '#ffd700'
            };
            return colors[color?.toLowerCase()] || '#dc3545';
        }

        function log(msg) {
            if (!debugMode) return;
            
            const debug = document.getElementById('debug');
            const time = new Date().toLocaleTimeString();
            debug.innerHTML = `[${time}] ${msg}<br>` + debug.innerHTML;
            console.log(`[${time}] ${msg}`);
        }

        function initMap() {
            map = L.map('map').setView([35.6586, 139.7454], 13);
            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '© OpenStreetMap contributors',
                maxZoom: 19
            }).addTo(map);
            log('Map initialized');
        }

        function updateMap(state) {
            if (!map) {
                log('ERROR: map is null');
                return;
            }

            // Update debug mode
            if (state.debugMode !== debugMode) {
                debugMode = state.debugMode;
                const debugEl = document.getElementById('debug');
                if (debugMode) {
                    debugEl.classList.add('visible');
                    log('Debug mode enabled');
                } else {
                    debugEl.classList.remove('visible');
                }
            }

            const stateStr = JSON.stringify(state);
            if (stateStr === lastState) {
                return; // No change
            }
            
            log(`Updating map: ${state.latitude}, ${state.longitude} @ zoom ${state.zoom}`);
            lastState = stateStr;

            // Update view
            map.setView([state.latitude, state.longitude], state.zoom, {
                animate: true,
                duration: 0.5
            });

            // Clear all markers
            markers.forEach(m => map.removeLayer(m));
            markers = [];

            // Add multiple markers if provided
            if (state.markers && state.markers.length > 0) {
                state.markers.forEach(markerData => {
                    const color = getMarkerColor(markerData.color);
                    
                    // Use CircleMarker for colored markers
                    const marker = L.circleMarker(
                        [markerData.latitude, markerData.longitude],
                        {
                            radius: 10,
                            fillColor: color,
                            color: '#fff',
                            weight: 2,
                            opacity: 1,
                            fillOpacity: 0.8
                        }
                    ).addTo(map);
                    
                    if (markerData.label) {
                        marker.bindPopup(markerData.label);
                    }
                    
                    markers.push(marker);
                    log(`Marker added: ${markerData.label || 'unnamed'} at ${markerData.latitude}, ${markerData.longitude}`);
                });
                
                // Open first marker popup if available
                if (markers.length > 0 && state.markers[0].label) {
                    markers[0].openPopup();
                }
            }
            // Add route markers if provided (default pin icons)
            if (state.routeMarkers && state.routeMarkers.length > 0) {
                state.routeMarkers.forEach(markerData => {
                    const marker = L.marker([markerData.latitude, markerData.longitude])
                        .addTo(map);
                    
                    if (markerData.label) {
                        marker.bindPopup(markerData.label);
                    }
                    
                    markers.push(marker);
                    log(`Route marker added: ${markerData.label || 'unnamed'} at ${markerData.latitude}, ${markerData.longitude}`);
                });
                
                // Open first route marker popup if available
                if (state.routeMarkers.length > 0 && state.routeMarkers[0].label) {
                    // Find the first route marker in the markers array
                    const firstRouteMarkerIndex = markers.length - state.routeMarkers.length;
                    if (firstRouteMarkerIndex >= 0) {
                        markers[firstRouteMarkerIndex].openPopup();
                    }
                }
            }
            // Add single marker if provided (backward compatibility)
            else if (state.marker) {
                const marker = L.marker([state.latitude, state.longitude])
                    .addTo(map)
                    .bindPopup(state.marker)
                    .openPopup();
                markers.push(marker);
                log(`Marker added: ${state.marker}`);
            }

            // Update route
            if (routeLayer) {
                map.removeLayer(routeLayer);
                routeLayer = null;
            }

            if (state.routeCoordinates && state.routeCoordinates.length > 0) {
                // Convert [lon, lat] to [lat, lon] for Leaflet
                const latLngs = state.routeCoordinates.map(coord => [coord[1], coord[0]]);
                
                routeLayer = L.polyline(latLngs, {
                    color: state.routeColor || '#0066ff',
                    weight: state.routeWidth || 4,
                    opacity: 0.7
                }).addTo(map);
                
                log(`Route added with ${state.routeCoordinates.length} points`);
            }
        }

        function connectSSE() {
            log('Connecting to SSE endpoint: /api/events');
            eventSource = new EventSource('/api/events');
            
            eventSource.onmessage = function(event) {
                try {
                    const state = JSON.parse(event.data);
                    log(`SSE received: lat=${state.latitude}, lng=${state.longitude}, zoom=${state.zoom}`);
                    updateMap(state);
                } catch (err) {
                    log(`ERROR: Failed to parse SSE data: ${err.message}`);
                    console.error('Failed to parse SSE data:', err);
                }
            };
            
            eventSource.onerror = function(err) {
                log(`SSE connection error: ${err}`);
                console.error('SSE connection error:', err);
                
                // Attempt to reconnect after 3 seconds
                setTimeout(() => {
                    log('Attempting to reconnect SSE...');
                    connectSSE();
                }, 3000);
            };
            
            eventSource.onopen = function() {
                log('SSE connection established');
            };
        }

        // Initialize
        log('Starting PowerShell.Map with SSE');
        initMap();
        connectSSE();
    </script>
</body>
</html>";
}
