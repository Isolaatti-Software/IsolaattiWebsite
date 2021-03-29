let recordedChunks = [];
let tracksData = [];
let audioContext = new AudioContext();
let offlineAudioContext = new OfflineAudioContext(2, (Math.trunc(duration) + 1) * 44000, 44000);

trackUrls.forEach((value) => {
   let request = new XMLHttpRequest();
   request.open("GET", value, true);
   request.responseType = 'arraybuffer';
   request.onload = () => {
       tracksData.push(request.response);
           if(tracksData.length === trackUrls.length) {
               // it already has the data
               console.log("all tracks were downloaded");
               createMix();
           }
   }
   request.send();
});

function createMix() {
    
    let nodesGenerated = 0;
    tracksData.forEach((value) => {
        audioContext.decodeAudioData(value, (buffer) => {
            console.log("track generated: " + nodesGenerated);
            nodesGenerated++;
            let sourceBuffer = offlineAudioContext.createBufferSource();
            sourceBuffer.buffer = buffer;
            sourceBuffer.connect(offlineAudioContext.destination);
            sourceBuffer.start();
            
            if(nodesGenerated === tracksData.length) {
                console.log("starting rendering");
                offlineAudioContext.startRendering().then(function(renderedBuffer) {
                    console.log("Mix is ready");
                    let mix = audioContext.createBufferSource();
                    mix.buffer = renderedBuffer;
                    mix.connect(audioContext.destination);

                    document.getElementById("play").addEventListener("click", function(){
                        mix.start();
                    });
                }).catch(function(reason){
                    console.error("error rendering " + reason);
                })
            }
        });

    });


    
}