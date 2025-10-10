let leftMostPoint = 0.0;
let rightMostPoint = 0.0;
let topMostPoint = 0.0;
let bottomMostPoint = 0.0;

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
        const latAsFloat = parseFloat(item["Latitude"]);
        const longAsFloat = parseFloat(item["Longitude"]);

        if (index === 0) {
            leftMostPoint = rightMostPoint = longAsFloat;
            topMostPoint = bottomMostPoint = latAsFloat;
        }

        if (longAsFloat < leftMostPoint) {
            leftMostPoint = longAsFloat;
        }

        if (longAsFloat > rightMostPoint) {
            rightMostPoint = longAsFloat;
        }

        if (latAsFloat < leftMostPoint) {
            topMostPoint = latAsFloat;
        }

        if (latAsFloat > rightMostPoint) {
            bottomMostPoint = latAsFloat;
        }

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

    // calculate center point
    const midLong = (Math.abs(rightMostPoint - leftMostPoint) / 2) + leftMostPoint;
    const midLat = (Math.abs(topMostPoint - bottomMostPoint) / 2) + bottomMostPoint;

    const mapViewRegion = new mapkit.CoordinateRegion(
        new mapkit.Coordinate(midLat, midLong),
        new mapkit.CoordinateSpan(
            (Math.abs(topMostPoint - bottomMostPoint) + .05),
            (Math.abs(rightMostPoint - leftMostPoint) + .05),
        )
    );

    map.setRegionAnimated(mapViewRegion, true);
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

    // Center the map on the new location with appropriate zoom
    const region = new mapkit.CoordinateRegion(
        coordinate,
        new mapkit.CoordinateSpan(0.5, 0.5) // Adjust zoom level as needed
    );

    mapInstance.setRegionAnimated(region, true);
}
