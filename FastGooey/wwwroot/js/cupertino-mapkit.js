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

    const map = new mapkit.Map("mapKit");
    mapInstance = map;
    
    const locationData = JSON.parse(document.getElementById("map-data-points").innerText);

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

    fitMapToAnnotations();
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
