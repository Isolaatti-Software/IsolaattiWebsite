let recordedChunks = [];
let resultBlob;
let tracksData = [];
let audioContext = new AudioContext();
let offlineAudioContext = new OfflineAudioContext(2, (Math.trunc(duration) + 1) * 44000, 44000);
let startRecordingButton = document.getElementById("play");
let horizontalBarMicrophone = document.getElementById("mic-graphical");
let recordingResultContainer = document.getElementById("result-container");
let playingBackingTrack = false;


let currentStateText = document.getElementById("current-state-text");
currentStateText.innerText = "Making backing track: downloading tracks..."
trackUrls.forEach((value) => {
   let request = new XMLHttpRequest();
   request.open("GET", value, true);
   request.responseType = 'arraybuffer';
   request.onload = () => {
       tracksData.push(request.response);
           if(tracksData.length === trackUrls.length) {
               // it already has the data
               console.log("all tracks were downloaded");
               currentStateText.innerHTML = "Making backing track: tracks downloaded!";
               createMix();
           }
   }
   request.send();
});

function createMix() {
    
    let nodesGenerated = 0;
    tracksData.forEach((value) => {
        audioContext.decodeAudioData(value, (buffer) => {
            nodesGenerated++;
            let sourceBuffer = offlineAudioContext.createBufferSource();
            sourceBuffer.buffer = buffer;
            sourceBuffer.connect(offlineAudioContext.destination);
            sourceBuffer.start();
            
            console.log("track generated: " + nodesGenerated);
            
            // when all tracks were decoded and connected to the offline 
            // audio context destination I can start rendering the backing track
            if(nodesGenerated === tracksData.length) {
                console.log("starting rendering");
                currentStateText.innerHTML = "Making backing track: mixing tracks";
                offlineAudioContext.startRendering().then(function(renderedBuffer) {
                    console.log("Mix is ready");
                    currentStateText.innerHTML = "Backing track is ready. You can start recording!";
                    
                }).catch(function(reason){
                    console.error("error rendering " + reason);
                })
            }
        });

    });
}

function startPlayingBackingTrack() {
    
}

function startRecording() {
    
}

function drawMicGraphical (audioAnalyserNode) {
    let bufferLenght = audioAnalyserNode.frequencyBinCount;
    let dataArray = new Uint8Array(bufferLenght);

    audioAnalyserNode.getByteFrequencyData(dataArray);

    let sumOfFrequencies = 0;
    for(let i = 0; i < bufferLenght; i++) {
        sumOfFrequencies += dataArray[i];
    }
    horizontalBarMicrophone.value = sumOfFrequencies/bufferLenght;
}