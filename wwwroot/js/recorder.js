let recordedChunks = [];
document.addEventListener("DOMContentLoaded", () => {
    let tracks = document.querySelectorAll(".track-audio-element");
    let numberOfTracksThatCanPlayTrough = 0;

    let audioContextForMixing = new AudioContext();

    tracks.forEach((value) => {
        value.oncanplaythrough = () => {
            numberOfTracksThatCanPlayTrough++;
            if(numberOfTracksThatCanPlayTrough === tracks.length) {
                makeMix();
            }
        }
    });
    
    function makeMix() {
        let mixingDestination = audioContextForMixing.createMediaStreamDestination();
        tracks.forEach((value) => {
            let node = audioContextForMixing.createMediaElementSource(value);
            node.connect(mixingDestination);
        });

        let mixStream = mixingDestination.stream;
        let mixMediaRecorder = new MediaRecorder(mixStream);
        
        mixMediaRecorder.ondataavailable = (event) => {
            console.log("data");
            if(event.data.size > 0) {
                recordedChunks.push(event.data);
                console.log(recordedChunks);
            }
        }
        tracks.forEach((element) => {
            element.play();
        });
        mixMediaRecorder.start();
        setTimeout(()=>{
            mixMediaRecorder.stop();
            let blob = new Blob(recordedChunks, {'type' : 'audio/ogg; codecs=vorbis'});
            document.getElementById("mix-player").href = window.URL.createObjectURL(blob);
        }, 4000);
        
        
    }
});