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
let isolaattiMixer = new IsolaattiAudioMixer(mediaElements, function(event){
    /* Handle here slider of current position */
    seekPositionSlider.value = event.target.currentTime;
});

isolaattiMixer.prepareMix(function(){
    console.log("Mixer is ready!!");
    document.querySelector("#play_pause_button").disabled = false;
    defineEvents();
});

isolaattiMixer.setOnMixEnded(function(event){
    playButton.innerHTML = '<i class="fas fa-play"></i>';
    seekPositionSlider.value = 0;
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
    })

    // seeking slider
    seekPositionSlider.max = isolaattiMixer.getDuration();
    seekPositionSlider.addEventListener("input", function(event) {
       isolaattiMixer.seekTo(event.target.value);
    });
    
    // main gain slider
    let mainGainSlider = document.querySelector("#mix_gain");
    mainGainSlider.addEventListener("input", function() {
        isolaattiMixer.setMainGainValue(mainGainSlider.value);
    });
    
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

