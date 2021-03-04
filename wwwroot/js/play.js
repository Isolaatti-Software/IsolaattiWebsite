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
let mainGainBar = document.querySelector("#mainGainBar");

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
    let localStorage = window.localStorage;

    let updateTime = 10;

    let selector = document.querySelector("#update-time-selector");
    let enabledGraphsSwitch = document.getElementById("enable-gain-graphs");

    
    if(localStorage.getItem("update-time-ms") !== null) {
        updateTime = parseInt(localStorage.getItem("update-time-ms"));
        selector.value = updateTime
    }
    // enables or disables the selector depending on the value stored
    selector.disabled = localStorage.getItem("gainGraphsDisabled") === "1" || false;
    
    // sets checked depending on the value stored
    enabledGraphsSwitch.checked = localStorage
        .getItem("gainGraphsDisabled") === "0" || localStorage.getItem("gainGraphsDisabled") === null;
    
    // these are for storing references to intervals, they are used below
    // to clearInterval
    let mainGainBarIntervalId = setInterval(function (){           // at the same time it defines the interval
        drawMainGainBar(isolaattiMixer.getAudioAnalyserNode());
    },updateTime);
    let gainBarsOfEveryTrack = new Map();

    // this is executed when the value of the control changes
    selector.addEventListener("change", function(){
        // sets the main bar update time
        clearInterval(mainGainBarIntervalId);
        mainGainBarIntervalId = setInterval(function (){
            drawMainGainBar(isolaattiMixer.getAudioAnalyserNode());
        },parseInt(selector.value));

        // sets the tracks bars update time
        isolaattiMixer.getTracks().forEach(function(track,name) {
            // this stops rendering the graph
            clearInterval(gainBarsOfEveryTrack.get(name));  
            // and then the interval is set again, with the new update time
            gainBarsOfEveryTrack.set(name,setInterval(function() {
                drawGainBarOfTrack(name,track.getAudioAnalyserNode());
            }, parseInt(selector.value)));
        });

        // stores the setting so that it can be used in the future
        localStorage.setItem("update-time-ms",selector.value);
    });
    
    // this is the "enable/disable gain graphs" switch event
    document.getElementById("enable-gain-graphs").addEventListener("input", function(e)  {
        // changes the selector disabled attribute
        selector.disabled = !this.checked;
        localStorage.setItem("gainGraphsDisabled", !this.checked ? "1" : "0" );
        
        // Here the graphs are enabled or disabled depending on what the value of switch (this) is
        if(this.checked) {
            // enabling master bar
            mainGainBarIntervalId = setInterval(function() {
                drawMainGainBar(isolaattiMixer.getAudioAnalyserNode());
            }, parseInt(selector.value));
            
            // enabling every track bar
            isolaattiMixer.getTracks().forEach(function(track, name) {
               gainBarsOfEveryTrack.set(name, setInterval(function() {
                   drawGainBarOfTrack(name,track.getAudioAnalyserNode())
               },parseInt(selector.value)));
            });
        } else {
            // disabling master bar
            clearInterval(mainGainBarIntervalId);
            mainGainBar.value = 0;
            
            // disabling every track bar
            isolaattiMixer.getTracks().forEach(function(track,name) {
                clearInterval(gainBarsOfEveryTrack.get(name));
                document.querySelectorAll(".track-average-gain-bar").forEach(function(value) {
                   value.value = 0; 
                });
            });
        }
    });
    
    isolaattiMixer.getTracks().forEach(function(track,name) {
        // this stores references for every interval. Use this
        // to do clearInterval and setInterval on event of changing
        gainBarsOfEveryTrack.set(name,setInterval(function() {
            drawGainBarOfTrack(name,track.getAudioAnalyserNode());
        },updateTime));
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

document.getElementById("button-settings").addEventListener("click", function(){
    $("#modal-performance-settings").modal('show');
});

