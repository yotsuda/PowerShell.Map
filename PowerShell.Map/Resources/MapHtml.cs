namespace PowerShell.Map.Resources;

public static class MapHtml
{
    public const string Template = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>PowerShell.Map</title>
    <link rel=""stylesheet"" href=""https://unpkg.com/maplibre-gl@4.7.1/dist/maplibre-gl.css"" />
    <script src=""https://unpkg.com/maplibre-gl@4.7.1/dist/maplibre-gl.js""></script>
    <style>
        html, body { margin: 0; padding: 0; overflow: hidden; }
        #map { height: 100vh; width: 100vw; }
        .header-container {
            position: absolute;
            top: 10px;
            right: 10px;
            z-index: 1000;
            display: flex;
            flex-direction: column;
            align-items: stretch;
            gap: 8px;
            width: 220px;
        }
        .logo {
            background: rgba(255, 255, 255, 0.95);
            padding: 8px 16px;
            border-radius: 4px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.2);
            font-family: 'Segoe UI', sans-serif;
            font-size: 14px;
            font-weight: 600;
            color: #2563eb;
            text-align: center;
            width: 100%;
            box-sizing: border-box;
        }
        .update-indicator {
            background: rgba(255, 255, 255, 0.9);
            padding: 6px 12px;
            border-radius: 4px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.2);
            font-family: sans-serif;
            font-size: 11px;
            color: #666;
            text-align: center;
            width: 100%;
            box-sizing: border-box;
        }
        .reset-button {
            background: rgba(255, 255, 255, 0.95);
            padding: 8px 16px;
            border-radius: 4px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.2);
            font-family: sans-serif;
            font-size: 12px;
            cursor: pointer;
            border: 1px solid #ddd;
            color: #333;
            transition: all 0.2s;
            width: 100%;
            text-align: center;
            box-sizing: border-box;
        }
        .reset-button:hover {
            background: rgba(255, 255, 255, 1);
            box-shadow: 0 4px 8px rgba(0,0,0,0.3);
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
        .maplibregl-popup-content {
            padding: 10px;
            font-family: sans-serif;
        }
        .camera-info {
            background: rgba(255, 255, 255, 0.95);
            padding: 10px 16px;
            border-radius: 4px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.2);
            font-family: 'Segoe UI', sans-serif;
            font-size: 12px;
            width: 100%;
            box-sizing: border-box;
        }
        .camera-info-title {
            font-weight: 600;
            color: #2563eb;
            margin-bottom: 8px;
            font-size: 13px;
        }
        .camera-info-item {
            display: flex;
            justify-content: space-between;
            margin: 4px 0;
            color: #333;
        }
        .camera-info-label {
            font-weight: 500;
            margin-right: 12px;
        }
        .camera-info-value {
            color: #666;
            font-family: monospace;
        }
        .toggle-3d {
            background: rgba(255, 255, 255, 0.95);
            padding: 10px 16px;
            border-radius: 4px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.2);
            font-family: 'Segoe UI', sans-serif;
            font-size: 13px;
            cursor: pointer;
            border: 1px solid #ddd;
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 10px;
            transition: all 0.2s;
            user-select: none;
            width: 100%;
            box-sizing: border-box;
        }
        .toggle-3d:hover {
            background: rgba(255, 255, 255, 1);
            box-shadow: 0 4px 8px rgba(0,0,0,0.3);
        }
        .toggle-switch {
            position: relative;
            width: 40px;
            height: 20px;
            background: #ccc;
            border-radius: 10px;
            transition: background 0.3s;
        }
        .toggle-switch.active {
            background: #2563eb;
        }
        .toggle-knob {
            position: absolute;
            top: 2px;
            left: 2px;
            width: 16px;
            height: 16px;
            background: white;
            border-radius: 50%;
            transition: transform 0.3s;
        }
        .toggle-switch.active .toggle-knob {
            transform: translateX(20px);
        }
        #location-description {
            position: absolute;
            bottom: 250px;
            left: 50%;
            transform: translateX(-50%);
            background: rgba(0, 0, 0, 0.92);
            color: white;
            padding: 16px 24px;
            border-radius: 4px;
            min-width: 280px;
            max-width: 400px;
            font-family: 'Segoe UI', sans-serif;
            font-size: 15px;
            line-height: 1.6;
            z-index: 1000;
            display: none;
            box-shadow: 0 3px 12px rgba(0,0,0,0.6);
            border: 2px solid rgba(255, 255, 255, 0.3);
            text-align: center;
            cursor: move;
            user-select: none;
        }
        #location-description.visible {
            display: block;
        }
        #location-description.dragging {
            opacity: 0.8;
        }
        /* Permanent label popup styles */
        .permanent-label .maplibregl-popup-content {
            background-color: rgba(255, 255, 255, 0.95);
            border-radius: 4px;
            box-shadow: 0 2px 6px rgba(0,0,0,0.3);
            padding: 4px 10px;
            border: 1px solid rgba(0,0,0,0.1);
        }
        .permanent-label .maplibregl-popup-tip {
            border-top-color: rgba(255, 255, 255, 0.95);
        }
    </style>
</head>
<body>
    <div id=""location-description""></div>
    <div id=""map""></div>
    <div class=""header-container"">
        <a href=""https://github.com/yotsuda/PowerShell.Map#readme"" target=""_blank"" rel=""noopener noreferrer"" class=""logo"">🗺️ PowerShell.Map</a>
        <div class=""camera-info"" id=""cameraInfo"">
            <div class=""camera-info-title"">📷 Camera Info</div>
            <div class=""camera-info-item"">
                <span class=""camera-info-label"">Bearing:</span>
                <span class=""camera-info-value"" id=""bearingValue"">0°</span>
            </div>
            <div class=""camera-info-item"">
                <span class=""camera-info-label"">Pitch:</span>
                <span class=""camera-info-value"" id=""pitchValue"">0°</span>
            </div>
            <div class=""camera-info-item"">
                <span class=""camera-info-label"">Zoom:</span>
                <span class=""camera-info-value"" id=""zoomValue"">13.0</span>
            </div>
        </div>
        <div class=""toggle-3d"" id=""toggle3d"">
            <span>🌍 3D</span>
            <div class=""toggle-switch"" id=""toggleSwitch"">
                <div class=""toggle-knob""></div>
            </div>
        </div>
        <button class=""reset-button"" id=""resetBtn"" title=""Reset camera view (Bearing=0, Pitch=0)"">🧭 Reset View</button>
        <div class=""update-indicator"" id=""indicator"">Initializing...</div>
    </div>
    <div class=""debug-log"" id=""debugLog""></div>
    <script>
        let map;
        let currentMarkers = [];
        let currentRoute = null;
        let debugMode = false;
        let is3DEnabled = false;
        let initialZoom = 13;
        let currentDescriptionMarker = null; // Track marker with visible description

        function debugLog(message) {
            if (!debugMode) return;
            const log = document.getElementById('debugLog');
            const time = new Date().toLocaleTimeString();
            log.innerHTML = '[' + time + '] ' + message + '<br>' + log.innerHTML;
        }


        function getTerrainExaggeration(zoom) {
            // Dynamically adjust terrain exaggeration based on zoom level
            // to minimize building-terrain interference in urban areas
            if (zoom >= 15) {
                // Urban detail level: minimal terrain to reduce building conflicts
                return 0.3;
            } else if (zoom >= 13) {
                // District level: natural terrain
                return 1.0;
            } else if (zoom >= 10) {
                // Wide area: moderate terrain emphasis for mountains
                return 1.5;
            } else {
                // Regional/mountain level: balanced terrain emphasis
                return 2.0;
            }
        }

        function updateTerrainExaggeration() {
            if (!map || !is3DEnabled) return;
            const zoom = map.getZoom();
            const exaggeration = getTerrainExaggeration(zoom);
            try {
                map.setTerrain({
                    'source': 'terrain',
                    'exaggeration': exaggeration
                });
                debugLog('Terrain exaggeration updated: ' + exaggeration.toFixed(1) + ' (zoom: ' + zoom.toFixed(1) + ')');
            } catch (e) {
                debugLog('Error updating terrain exaggeration: ' + e.message);
            }
        }

        function updateCameraInfo() {
            if (!map) return;
            const bearing = map.getBearing();
            const pitch = map.getPitch();
            const zoom = map.getZoom();
            
            document.getElementById('bearingValue').textContent = bearing.toFixed(0) + '°';
            document.getElementById('pitchValue').textContent = pitch.toFixed(0) + '°';
            document.getElementById('zoomValue').textContent = zoom.toFixed(1);
        }

        function checkMarkerVisibility() {
            if (!map || !currentDescriptionMarker) {
                return;
            }
            
            const descriptionOverlay = document.getElementById('location-description');
            if (!descriptionOverlay.classList.contains('visible')) {
                return;
            }
            
            // Check if currentDescriptionMarker is a Marker object or coordinates
            let isVisible;
            
            if (currentDescriptionMarker.getElement) {
                // It's a Marker object - check if visible in viewport
                const markerEl = currentDescriptionMarker.getElement();
                const rect = markerEl.getBoundingClientRect();
                
                // Check against actual viewport (no margin)
                isVisible = rect.right >= 0 && 
                           rect.left <= window.innerWidth && 
                           rect.bottom >= 0 && 
                           rect.top <= window.innerHeight;
                
                console.log('Marker viewport visibility: visible=' + isVisible +
                           ' left=' + rect.left.toFixed(1) + 
                           ' right=' + rect.right.toFixed(1) + '/' + window.innerWidth +
                           ' top=' + rect.top.toFixed(1) + 
                           ' bottom=' + rect.bottom.toFixed(1) + '/' + window.innerHeight);
            } else {
                // It's a coordinate object - find the actual marker
                const lng = currentDescriptionMarker.lng;
                const lat = currentDescriptionMarker.lat;
                
                // Find marker with matching coordinates
                const marker = currentMarkers.find(m => {
                    const lngLat = m.getLngLat();
                    return Math.abs(lngLat.lng - lng) < 0.0001 && Math.abs(lngLat.lat - lat) < 0.0001;
                });
                
                if (marker && marker.getElement) {
                    // Found the marker - check its DOM position
                    currentDescriptionMarker = marker; // Update to use marker object
                    const markerEl = marker.getElement();
                    const rect = markerEl.getBoundingClientRect();
                    
                    isVisible = rect.right >= 0 && 
                               rect.left <= window.innerWidth && 
                               rect.bottom >= 0 && 
                               rect.top <= window.innerHeight;
                    
                    console.log('Marker (found) viewport visibility: visible=' + isVisible +
                               ' left=' + rect.left.toFixed(1) + 
                               ' right=' + rect.right.toFixed(1) + '/' + window.innerWidth);
                } else {
                    // Fallback to map.project()
                    const markerPoint = map.project([lng, lat]);
                    const mapCanvas = map.getCanvas();
                    
                    isVisible = markerPoint.x >= 0 && markerPoint.x <= mapCanvas.width && 
                               markerPoint.y >= 0 && markerPoint.y <= mapCanvas.height;
                    
                    console.log('Marker coord visibility: visible=' + isVisible +
                               ' x=' + markerPoint.x.toFixed(1) + '/' + mapCanvas.width);
                }
            }
            
            if (!isVisible) {
                console.log('HIDING description - marker out of viewport');
                descriptionOverlay.classList.remove('visible');
                currentDescriptionMarker = null;
            }
        }
        function toggle3DBuildings() {
            is3DEnabled = !is3DEnabled;
            const toggleSwitch = document.getElementById('toggleSwitch');
            
            if (is3DEnabled) {
                toggleSwitch.classList.add('active');
                enable3DFeatures();
                debugLog('3D features enabled via toggle');
            } else {
                toggleSwitch.classList.remove('active');
                remove3DFeatures();
                debugLog('3D features disabled via toggle');
            }
        }


        function initMap(state) {
            debugLog('Initializing map... Enable3D: ' + state.enable3D);
            
            initialZoom = state.zoom;
            
            // Create map
            map = new maplibregl.Map({
                container: 'map',
                style: 'https://tiles.openfreemap.org/styles/liberty',
                center: [state.longitude, state.latitude],
                zoom: state.zoom,
                pitch: state.pitch || 0,
                bearing: state.bearing || 0
            });


            // Add scale control
            map.addControl(new maplibregl.ScaleControl());

            // Reset button handler
            document.getElementById('resetBtn').addEventListener('click', () => {
                map.flyTo({
                    pitch: 0,
                    bearing: 0,
                    duration: 1000
                });
                debugLog('View reset to default');
            });


            map.on('load', () => {
                debugLog('Map loaded');
                
                // Handle 3D features based on enable3D flag
                is3DEnabled = state.enable3D;
                
                // Update toggle switch UI
                const toggleSwitch = document.getElementById('toggleSwitch');
                if (is3DEnabled) {
                    toggleSwitch.classList.add('active');
                }
                
                if (state.enable3D) {
                    enable3DFeatures();
                } else {
                    // Remove any 3D building layers that might exist in the style
                    remove3DFeatures();
                }
                
                updateMapState(state);
                updateCameraInfo();
                updateIndicator('Ready');
                
                // Initialize draggable description box
                initDescriptionDrag();
            });

            map.on('error', (e) => {
                debugLog('Map error: ' + e.error.message);
            });

            // Update camera info on map movement
            map.on('move', updateCameraInfo);
            map.on('zoom', () => {
                updateCameraInfo();
                updateTerrainExaggeration();
            });
            map.on('rotate', updateCameraInfo);
            map.on('pitch', updateCameraInfo);
            
            // Check marker visibility in real-time during map movement
            map.on('move', checkMarkerVisibility);
            map.on('moveend', checkMarkerVisibility);
            map.on('drag', checkMarkerVisibility);
            
            // Close description when clicking on map (not on marker)
            map.on('click', () => {
                const descriptionOverlay = document.getElementById('location-description');
                descriptionOverlay.classList.remove('visible');
                currentDescriptionMarker = null;
            });
            // 3D toggle button handler
            document.getElementById('toggle3d').addEventListener('click', toggle3DBuildings);
        }
        function initDescriptionDrag() {
            const descBox = document.getElementById('location-description');
            let isDragging = false;
            let startMouseX;
            let startMouseY;
            let startLeft;
            let startBottom;

            // Reset position function - call this when description is shown
            descBox.resetPosition = function() {
                descBox.style.transform = 'translateX(-50%)';
                descBox.style.left = '50%';
                descBox.style.bottom = '250px';
            };

            descBox.addEventListener('mousedown', dragStart);
            document.addEventListener('mousemove', drag);
            document.addEventListener('mouseup', dragEnd);

            function dragStart(e) {
                if (!descBox.classList.contains('visible')) return;
                
                isDragging = true;
                
                // Get current position including transform
                const rect = descBox.getBoundingClientRect();
                const viewportHeight = window.innerHeight;
                
                // Store initial mouse position
                startMouseX = e.clientX;
                startMouseY = e.clientY;
                
                // Store initial element position (convert to absolute)
                startLeft = rect.left;
                startBottom = viewportHeight - rect.bottom;
                
                // Convert to absolute positioning
                descBox.style.transform = 'none';
                descBox.style.left = startLeft + 'px';
                descBox.style.bottom = startBottom + 'px';
                
                descBox.classList.add('dragging');
            }

            function drag(e) {
                if (!isDragging) return;
                
                e.preventDefault();
                
                // Calculate total movement from start
                const deltaX = e.clientX - startMouseX;
                const deltaY = e.clientY - startMouseY;
                
                // Update position based on initial position + delta
                descBox.style.left = (startLeft + deltaX) + 'px';
                // Note: bottom increases upward, but clientY increases downward, so we negate deltaY
                descBox.style.bottom = (startBottom - deltaY) + 'px';
            }

            function dragEnd() {
                isDragging = false;
                descBox.classList.remove('dragging');
            }
        }

        function enable3DFeatures() {
            debugLog('Enabling 3D features...');
            
            // Add terrain source (Terrarium format from AWS)
            if (!map.getSource('terrain')) {
                try {
                    map.addSource('terrain', {
                        'type': 'raster-dem',
                        'tiles': ['https://s3.amazonaws.com/elevation-tiles-prod/terrarium/{z}/{x}/{y}.png'],
                        'encoding': 'terrarium',
                        'tileSize': 256,
                        'maxzoom': 15
                    });
                    debugLog('Terrain source added');
                } catch (e) {
                    debugLog('Error adding terrain source: ' + e.message);
                }
            }
            
            // Enable terrain with dynamic exaggeration based on zoom level
            const zoom = map.getZoom();
            const exaggeration = getTerrainExaggeration(zoom);
            try {
                map.setTerrain({
                    'source': 'terrain',
                    'exaggeration': exaggeration
                });
                debugLog('Terrain enabled with exaggeration: ' + exaggeration.toFixed(1) + ' (zoom: ' + zoom.toFixed(1) + ')');
            } catch (e) {
                debugLog('Error enabling terrain: ' + e.message);
            }
            
            if (!map.getSource('openmaptiles')) {
                debugLog('OpenMapTiles source not found, 3D buildings may not be available');
                return;
            }
            
            try {
                map.addLayer({
                    'id': '3d-buildings',
                    'source': 'openmaptiles',
                    'source-layer': 'building',
                    'type': 'fill-extrusion',
                    'minzoom': 14,
                    'paint': {
                        'fill-extrusion-color': '#ccc',
                        'fill-extrusion-height': ['coalesce', ['get', 'render_height'], 5],
                        'fill-extrusion-base': ['coalesce', ['get', 'render_min_height'], 0],
                        'fill-extrusion-opacity': 0.6
                    }
                });
                debugLog('3D buildings layer added');
            } catch (e) {
                debugLog('Error adding 3D buildings: ' + e.message);
            }
        }


        function remove3DFeatures() {
            debugLog('Removing 3D features...');
            
            // Remove terrain
            try {
                map.setTerrain(null);
                debugLog('Terrain disabled');
            } catch (e) {
                debugLog('Error disabling terrain: ' + e.message);
            }
            
            // Remove terrain source
            if (map.getSource('terrain')) {
                try {
                    map.removeSource('terrain');
                    debugLog('Terrain source removed');
                } catch (e) {
                    debugLog('Error removing terrain source: ' + e.message);
                }
            }
            
            // Check all layers and remove any 3D building layers
            const style = map.getStyle();
            if (!style || !style.layers) return;
            
            const buildingLayerPatterns = ['3d-building', 'building-3d', 'building-extrusion'];
            
            style.layers.forEach(layer => {
                if (layer.type === 'fill-extrusion' || 
                    buildingLayerPatterns.some(pattern => layer.id.toLowerCase().includes(pattern))) {
                    try {
                        map.removeLayer(layer.id);
                        debugLog('Removed 3D layer: ' + layer.id);
                    } catch (e) {
                        debugLog('Could not remove layer ' + layer.id + ': ' + e.message);
                    }
                }
            });
        }

        function updateMapState(state) {
            debugLog('Updating map state... Enable3D: ' + state.enable3D);
            debugMode = state.debugMode;
            document.getElementById('debugLog').className = debugMode ? 'debug-log visible' : 'debug-log';
            
            updateMapContent(state);
        }

        function updateMapContent(state) {

            // Clear existing markers and their label popups
            currentMarkers.forEach(marker => {
                if (marker._labelPopup) {
                    marker._labelPopup.remove();
                }
                marker.remove();
            });
            currentMarkers = [];

            // Clear existing route
            if (currentRoute) {
                if (map.getLayer('route')) map.removeLayer('route');
                if (map.getSource('route')) map.removeSource('route');
                currentRoute = null;
            }

            // Handle 3D features
            is3DEnabled = state.enable3D;
            
            // Update toggle switch UI
            const toggleSwitch = document.getElementById('toggleSwitch');
            if (is3DEnabled) {
                toggleSwitch.classList.add('active');
            } else {
                toggleSwitch.classList.remove('active');
            }
            
            // Add or remove 3D features
            if (state.enable3D) {
                if (!map.getLayer('3d-buildings')) {
                    enable3DFeatures();
                }
            } else {
                if (map.getLayer('3d-buildings')) {
                    map.removeLayer('3d-buildings');
                    debugLog('3D buildings layer removed');
                }
            }

            // Calculate camera angles
            // Calculate camera angles
            const targetPitch = state.pitch !== undefined ? state.pitch : (state.enable3D ? 60 : 0);
            const targetBearing = state.bearing !== undefined ? state.bearing : 0;

            let singleMarker = null; // Track single marker for description

            // Add markers
            if (state.markers && state.markers.length > 0) {
                debugLog(`Adding ${state.markers.length} markers`);
                state.markers.forEach(marker => {
                    addMarker(marker.longitude, marker.latitude, marker.label, marker.color, marker.description);
                });
                
                // Fit bounds to show all markers
                const bounds = new maplibregl.LngLatBounds();
                state.markers.forEach(marker => {
                    bounds.extend([marker.longitude, marker.latitude]);
                });
                map.fitBounds(bounds, { 
                    padding: 50, 
                    animate: state.animate, 
                    duration: state.duration * 1000,
                    pitch: targetPitch,
                    bearing: targetBearing
                });
            } else if (state.marker) {
                // Single marker
                debugLog(`Adding single marker: ${state.marker}`);
                singleMarker = addMarker(state.longitude, state.latitude, state.marker, '#dc3545', state.locationDescription);
                flyTo(state.longitude, state.latitude, state.zoom, state.animate, state.duration, targetPitch, targetBearing);
            } else {
                // Just move to location
                flyTo(state.longitude, state.latitude, state.zoom, state.animate, state.duration, targetPitch, targetBearing);
            }

            // Add route markers
            if (state.routeMarkers && state.routeMarkers.length > 0) {
                debugLog(`Adding ${state.routeMarkers.length} route markers`);
                state.routeMarkers.forEach(marker => {
                    addMarker(marker.longitude, marker.latitude, marker.label, marker.color, marker.description);
                });
            }

            // Add route line
            if (state.routeCoordinates && state.routeCoordinates.length > 0) {
                debugLog(`Adding route with ${state.routeCoordinates.length} points`);
                addRoute(state.routeCoordinates, state.routeColor || '#0066ff', state.routeWidth || 4);
                
                // If zoom is user-specified, fly to start point with specified zoom
                // Otherwise, fit bounds to show entire route
                if (state.zoom !== null && state.zoom !== undefined) {
                    debugLog(`User-specified zoom: ${state.zoom}, flying to start point`);
                    flyTo(state.longitude, state.latitude, state.zoom, state.animate, state.duration, targetPitch, targetBearing);
                } else {
                    debugLog('Auto-fitting bounds to show entire route');
                    const bounds = new maplibregl.LngLatBounds();
                    state.routeCoordinates.forEach(coord => {
                        bounds.extend(coord);
                    });
                    map.fitBounds(bounds, { 
                        padding: 50, 
                        animate: state.animate, 
                        duration: state.duration * 1000,
                        pitch: targetPitch,
                        bearing: targetBearing
                    });
                }
            }


            updateCameraInfo();

            // Update location description (show after animation completes)
            const descriptionOverlay = document.getElementById('location-description');
            descriptionOverlay.classList.remove('visible'); // Hide during animation
            currentDescriptionMarker = null; // Reset marker tracking
            
            if (state.locationDescription) {
                descriptionOverlay.textContent = state.locationDescription;
                
                // Reset position to center before showing
                if (descriptionOverlay.resetPosition) {
                    descriptionOverlay.resetPosition();
                }
                
                // Find the marker object for this location
                let marker = null;
                if (singleMarker !== null) {
                    marker = singleMarker;
                    console.log('Using singleMarker for description tracking');
                } else {
                    // Find marker with matching coordinates from currentMarkers
                    marker = currentMarkers.find(m => {
                        const lngLat = m.getLngLat();
                        return Math.abs(lngLat.lng - state.longitude) < 0.0001 && 
                               Math.abs(lngLat.lat - state.latitude) < 0.0001;
                    });
                    if (marker) {
                        console.log('Found marker from currentMarkers for description tracking');
                    } else {
                        console.log('WARNING: No marker found - creating one');
                        // Create a marker for tracking (will be visible but that's ok for now)
                        marker = addMarker(state.longitude, state.latitude, '', '#dc3545', null);
                    }
                }
                
                // Track the marker object for visibility checking
                currentDescriptionMarker = marker;
                
                // Show description after animation completes
                const delayMs = state.animate ? (state.duration * 1000) : 0;
                setTimeout(() => {
                    descriptionOverlay.classList.add('visible');
                }, delayMs);
            }
            updateIndicator('Updated');
        }

        function addMarker(lng, lat, label, color, description) {
            const el = document.createElement('div');
            el.className = 'marker';
            el.style.width = '20px';
            el.style.height = '20px';
            el.style.borderRadius = '50%';
            el.style.backgroundColor = color || '#dc3545';
            el.style.border = '2px solid white';
            el.style.boxShadow = '0 2px 4px rgba(0,0,0,0.3)';

            const marker = new maplibregl.Marker(el)
                .setLngLat([lng, lat])
                .addTo(map);

            // Add label as a permanent popup
            if (label) {
                const popup = new maplibregl.Popup({
                    closeButton: false,
                    closeOnClick: false,
                    closeOnMove: false,
                    offset: 25,
                    className: 'permanent-label'
                })
                .setLngLat([lng, lat])
                .setHTML(`<div style=""font-weight: bold; font-size: 13px; color: #333; white-space: nowrap;"">${label}</div>`)
                .addTo(map);
                
                // Store popup reference with marker
                marker._labelPopup = popup;
            }
            // Add click event for description
            if (description) {
                const markerElement = marker.getElement();
                markerElement.style.cursor = 'pointer';
                markerElement.addEventListener('click', function(e) {
                    e.stopPropagation();
                    const descriptionOverlay = document.getElementById('location-description');
                    descriptionOverlay.textContent = description;
                    descriptionOverlay.style.whiteSpace = 'normal';
                    
                    // Reset position to center before showing
                    if (descriptionOverlay.resetPosition) {
                        descriptionOverlay.resetPosition();
                    }
                    
                    descriptionOverlay.classList.add('visible');
                    
                    // Track current marker object for visibility check
                    currentDescriptionMarker = marker;
                    debugLog('Marker clicked, set currentDescriptionMarker');
                });
            }

            currentMarkers.push(marker);
            return marker; // Return marker object for tracking
        }

        function addRoute(coordinates, color, width) {
            map.addSource('route', {
                'type': 'geojson',
                'data': {
                    'type': 'Feature',
                    'properties': {},
                    'geometry': {
                        'type': 'LineString',
                        'coordinates': coordinates
                    }
                }
            });

            map.addLayer({
                'id': 'route',
                'type': 'line',
                'source': 'route',
                'layout': {
                    'line-join': 'round',
                    'line-cap': 'round'
                },
                'paint': {
                    'line-color': color,
                    'line-width': width
                }
            });

            currentRoute = 'route';
        }

        function flyTo(lng, lat, zoom, animate, duration, pitch, bearing) {
            if (animate) {
                map.flyTo({
                    center: [lng, lat],
                    zoom: zoom,
                    pitch: pitch || 0,
                    bearing: bearing || 0,
                    duration: duration * 1000
                });
            } else {
                map.jumpTo({
                    center: [lng, lat],
                    zoom: zoom,
                    pitch: pitch || 0,
                    bearing: bearing || 0
                });
            }
        }

        function updateIndicator(text) {
            const indicator = document.getElementById('indicator');
            indicator.textContent = text;
            setTimeout(() => {
                indicator.style.opacity = '0';
                setTimeout(() => {
                    indicator.style.opacity = '1';
                }, 200);
            }, 100);
        }

        // Connect to SSE
        const eventSource = new EventSource('/api/events');
        
        eventSource.onopen = () => {
            debugLog('SSE connected');
        };

        eventSource.onmessage = (event) => {
            try {
                const state = JSON.parse(event.data);
                debugLog('Received state update');
                
                if (!map) {
                    initMap(state);
                } else {
                    updateMapState(state);
                }
            } catch (e) {
                debugLog('Error parsing state: ' + e.message);
            }
        };

        eventSource.onerror = (error) => {
            debugLog('SSE error');
            updateIndicator('Connection lost');
        };

        // Initial fetch
        fetch('/api/state')
            .then(response => response.json())
            .then(state => {
                debugLog('Initial state loaded');
                initMap(state);
            })
            .catch(error => {
                debugLog('Error loading initial state: ' + error.message);
            });
    </script>
</body>
</html>";
}
