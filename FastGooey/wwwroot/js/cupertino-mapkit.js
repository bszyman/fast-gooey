
let mapInstance = null;

const calloutDelegate = {
    calloutRightAccessoryForAnnotation: function(annotation) {
        let accessoryViewRight = document.createElement("a");
        accessoryViewRight.className = "right-accessory-view";
        accessoryViewRight.href = annotation.data.pageLink;
        accessoryViewRight.appendChild(document.createTextNode("ⓘ"));

        return accessoryViewRight;
    }
};

async function fitMapToAnnotations() {
    if (!mapInstance || mapInstance.annotations.length === 0) {
        return;
    }

    const mapkit = await getMapKitAsync();

    if (mapInstance.annotations.length === 1) {
        const annotation = mapInstance.annotations[0];
        const cameraDistance = 250000; // 250km in meters (fits about half a state)

        // Create a camera with the desired properties
        const camera = new mapkit.CameraZoomRange(cameraDistance, cameraDistance);

        // Set region instead of using separate camera commands
        const span = new mapkit.CoordinateSpan(2.5, 2.5); // Roughly matches 250km view
        const region = new mapkit.CoordinateRegion(annotation.coordinate, span);

        mapInstance.setRegionAnimated(region, true);

        return;
    }

    // Use showItems to automatically fit the viewport to all annotations
    mapInstance.showItems(
        mapInstance.annotations,
        {
            animate: true,
            padding: new mapkit.Padding(50, 50, 50, 50) // Add padding around edges
        }
    );
}

async function getMapKitAsync() {
    if (!window.mapkit || window.mapkit.loadedLibraries.length === 0) {
        await new Promise(resolve => {window.initMapKit = resolve});
        delete window.initMapKit;
    }
    return window.mapkit;
}

async function initMapKit() {
    if (document.getElementById("mapKit") === null) {
        return;
    }

    const mapkit = await getMapKitAsync();

    // Destroy existing map instance if it exists
    if (mapInstance) {
        mapInstance.destroy();
        mapInstance = null;
    }

    const map = new mapkit.Map("mapKit");
    mapInstance = map;

    const mapDataElement = document.getElementById("map-data-points");
    if (!mapDataElement) {
        return;
    }

    const locationData = JSON.parse(mapDataElement.innerText);

    locationData.forEach((item, index) => {
        const coordinate = new mapkit.Coordinate(parseFloat(item["Latitude"]), parseFloat(item["Longitude"]));
        const annotation = new mapkit.MarkerAnnotation(
            coordinate,
            {
                title: item["Title"] ?? "",
                subtitle: item["Subtitle"] ?? "",
                callout: calloutDelegate,
                clusteringIdentifier: item["ClusteringKey"] ?? "default",
                data: {
                    pageLink: item["PageLink"] ?? "#"
                }
            });

        map.addAnnotation(annotation);
    });

    fitMapToAnnotations();
}

// Listen for HTMX content loaded events and reinitialize map
document.addEventListener('htmx:afterSwap', function(event) {
    // Check if the swapped content contains a map element
    if (event.detail.target.querySelector('#mapKit') || event.detail.target.id === 'workspace') {
        initMapKit();
    }
});

async function updateMapWithLocation(latitude, longitude, title) {
    if (!mapInstance) {
        console.error("Map instance not initialized");
        return;
    }

    const mapkit = await getMapKitAsync();

    // Remove all existing annotations
    mapInstance.removeAnnotations(mapInstance.annotations);

    // Add new annotation
    const coordinate = new mapkit.Coordinate(latitude, longitude);
    const annotation = new mapkit.MarkerAnnotation(
        coordinate,
        {
            title: title,
            subtitle: `${latitude.toFixed(5)}° N, ${longitude.toFixed(5)}° W`
        }
    );

    mapInstance.addAnnotation(annotation);

    await fitMapToAnnotations();
}

async function addLocationToMap(latitude, longitude, title) {
    if (!mapInstance) {
        console.error("Map instance not initialized");
        return;
    }

    const mapkit = await getMapKitAsync();

    // Add new annotation
    const coordinate = new mapkit.Coordinate(latitude, longitude);
    const annotation = new mapkit.MarkerAnnotation(
        coordinate,
        {
            title: title,
            subtitle: `${latitude.toFixed(5)}° N, ${longitude.toFixed(5)}° W`
        }
    );

    mapInstance.addAnnotation(annotation);

    fitMapToAnnotations();
}