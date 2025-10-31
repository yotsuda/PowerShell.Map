
        let map;
        let currentMarkers = [];
        let currentRoute = null;
        let debugMode = false;
        let is3DEnabled = false;
        let initialZoom = 13;

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
                // Urban detail level: nearly flat to avoid building conflicts
                return 0.2;
            } else if (zoom >= 13) {
                // District level: subtle terrain
                return 0.7;
            } else if (zoom >= 10) {
                // Wide area: moderate terrain emphasis
                return 1.5;
            } else {
                // Regional/mountain level: strong terrain emphasis
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

            // 3D toggle button handler
            document.getElementById('toggle3d').addEventListener('click', toggle3DBuildings);
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

            // Clear existing markers
            currentMarkers.forEach(marker => marker.remove());
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
            const targetPitch = state.pitch !== undefined ? state.pitch : (state.enable3D ? 60 : 0);
            const targetBearing = state.bearing !== undefined ? state.bearing : 0;

            // Add markers
            if (state.markers && state.markers.length > 0) {
                debugLog(`Adding ${state.markers.length} markers`);
                state.markers.forEach(marker => {
                    addMarker(marker.longitude, marker.latitude, marker.label, marker.color);
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
                addMarker(state.longitude, state.latitude, state.marker, '#dc3545');
                flyTo(state.longitude, state.latitude, state.zoom, state.animate, state.duration, targetPitch, targetBearing);
            } else {
                // Just move to location
                flyTo(state.longitude, state.latitude, state.zoom, state.animate, state.duration, targetPitch, targetBearing);
            }

            // Add route markers
            if (state.routeMarkers && state.routeMarkers.length > 0) {
                debugLog(`Adding ${state.routeMarkers.length} route markers`);
                state.routeMarkers.forEach(marker => {
                    addMarker(marker.longitude, marker.latitude, marker.label, marker.color);
                });
            }

            // Add route line
            if (state.routeCoordinates && state.routeCoordinates.length > 0) {
                debugLog(`Adding route with ${state.routeCoordinates.length} points`);
                addRoute(state.routeCoordinates, state.routeColor || '#0066ff', state.routeWidth || 4);
                
                // Fit bounds to route
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


            updateCameraInfo();
            updateIndicator('Updated');
        }

        function addMarker(lng, lat, label, color) {
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

            if (label) {
                const popup = new maplibregl.Popup({ offset: 25 })
                    .setHTML(`<strong>${label}</strong>`);
                marker.setPopup(popup);
            }

            currentMarkers.push(marker);
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
    
