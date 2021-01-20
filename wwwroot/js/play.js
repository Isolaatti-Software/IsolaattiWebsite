/*
* Isolaatti project
* Erik Cavazos, 2021
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
* 
* This file should be placed in the Pages/WebApp/Play.cshtml
*/
let mediaElements = document.querySelectorAll(".track-audio-element");
let seekPositionSlider = document.querySelector("#seek-position-slider");
let playButton = document.querySelector("#play_pause_button");
let timeLabel = document.querySelector("#time-container");
let stopButton = document.querySelector("#stop_button");
let mainGainLabel = document.querySelector("#mix_gain_label");

let isolaattiMixer = new IsolaattiAudioMixer(mediaElements, function(event){
    /* Handle here slider of current position */
    seekPositionSlider.value = event.target.currentTime;
    timeLabel.innerHTML =
        `${getClockFormatFromSeconds(event.target.currentTime)}/${getClockFormatFromSeconds(isolaattiMixer.getDuration())}`;
});



isolaattiMixer.prepareMix(function(){
    console.log("Mixer is ready!!");
    document.querySelector("#play_pause_button").disabled = false;
    defineEvents();
    timeLabel.innerHTML = `--/${getClockFormatFromSeconds(isolaattiMixer.getDuration())}`;
    isolaattiMixer.getAudioAnalyserNode().fftSize = 64;
    
    setInterval(function (){
        drawMainGainBar(isolaattiMixer.getAudioAnalyserNode());
    },10)
    isolaattiMixer.getTracks().forEach(function(track,name) {
        setInterval(function() {
            drawGainBarOfTrack(name,track.getAudioAnalyserNode());
        });
    });
});

isolaattiMixer.setOnMixEnded(function(event){
    playButton.innerHTML = '<i class="fas fa-play"></i>';
    seekPositionSlider.value = 0;
});

// hide top bar button
let hideTopBarButton = document.querySelector("#hide-top-bar-button");
let topBar = document.querySelector("nav.nav.custom-nav.sticky-top");
hideTopBarButton.addEventListener("click", function() {
    if(topBar.style.display === "none") {
        topBar.style.display = "flex";
        hideTopBarButton.innerHTML = '<i class="fas fa-chevron-up"></i>'
    } else {
        topBar.style.display = "none";
        hideTopBarButton.innerHTML = '<i class="fas fa-chevron-down"></i>';
    }
});

function defineEvents() {
    // set events here
    playButton.addEventListener("click", function() {
        if(isolaattiMixer.playing){
            isolaattiMixer.pauseMix();
            playButton.innerHTML = '<i class="fas fa-play"></i>';
        } else {
            isolaattiMixer.playMix();
            playButton.innerHTML = '<i class="fas fa-pause"></i>';
        }
    });
    
    stopButton.addEventListener("click", function() {
       isolaattiMixer.pauseMix();
       playButton.innerHTML = '<i class="fas fa-play"></i>';
       isolaattiMixer.seekTo(0);
    });
    
    // seeking slider
    seekPositionSlider.max = isolaattiMixer.getDuration();
    seekPositionSlider.addEventListener("input", function(event) {
       isolaattiMixer.seekTo(event.target.value);
    });
    
    // main gain slider
    let mainGainSlider = document.querySelector("#mix_gain");
    mainGainSlider.addEventListener("input", function() {
        isolaattiMixer.setMainGainValue(mainGainSlider.value);
        mainGainLabel.innerHTML = `${mainGainSlider.value*100}%`
    });
    
    // mute/unmute buttons of every track
    let mute_unmuteButtonsOfEveryTrack = document.getElementsByClassName("mute-track-btn");
    for(let button of mute_unmuteButtonsOfEveryTrack) {
        button.addEventListener("click", function() {
            isolaattiMixer.setGainOfTrack(button.attributes.getNamedItem("target-track").value, 0.0);
        });
    }
    
    // gains of every track
    let tracksGainSliders = document.querySelectorAll(".track-gain");
    tracksGainSliders.forEach(function(slider) {
        slider.addEventListener("input",function() {
            isolaattiMixer.setGainOfTrack(slider.attributes.getNamedItem("target-track").value, slider.value);
        });
    });
    
    // stereo panner of every track
    let stereoPanningSliders = document.querySelectorAll(".balance_slider");
    stereoPanningSliders.forEach(function(slider) {
       slider.addEventListener("input", function() {
           isolaattiMixer.setPannerValueOfTrack(slider.attributes.getNamedItem("target-track").value, slider.value);
       });
    });
}

function getClockFormatFromSeconds(secs){
    let truncatedSecs = Math.round(secs);
    let minutes = truncatedSecs / 60;
    let seconds = truncatedSecs % 60;
    if (seconds < 10) {
        seconds = `0${seconds}`
    }
    return `${Math.trunc(minutes)}:${seconds}`;
}
let mainGainBar = document.querySelector("#mainGainBar");
function drawMainGainBar(audioAnalyserNode) {
    let bufferLenght = audioAnalyserNode.frequencyBinCount;
    
    let dataArray = new Uint8Array(bufferLenght);
    
    audioAnalyserNode.getByteFrequencyData(dataArray);
    let sumOfFrequencies = 0;
    for(let i = 0; i < bufferLenght; i++) {
        sumOfFrequencies += dataArray[i];
    }
    mainGainBar.value = sumOfFrequencies/bufferLenght;
}

function drawGainBarOfTrack(nameOfTrack,audioAnalyserNode) {
    let progressElement = document.querySelector(`.track-average-gain-bar[target-track=${nameOfTrack}]`);
    
    let bufferLenght = audioAnalyserNode.frequencyBinCount;
    let dataArray = new Uint8Array(bufferLenght);
    
    audioAnalyserNode.getByteFrequencyData(dataArray);
    
    let sumOfFrequencies = 0;
    for(let i = 0; i < bufferLenght; i++) {
        sumOfFrequencies += dataArray[i];
    }
    progressElement.value = sumOfFrequencies/bufferLenght;
}