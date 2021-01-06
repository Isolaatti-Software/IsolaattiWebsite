var numberOfTracks = 0;
var counter = 0;
onmessage = function(event) {
    if(typeof event.data === "object") {
        numberOfTracks = event.data.numberOfTracks;
    }
    
    if(event.data === "+1") {
        counter++;
        if(numberOfTracks === counter) {
            postMessage("completed");
        }
    }
}