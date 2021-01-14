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
let isolaattiMixer = new IsolaattiAudioMixer(mediaElements, function(event){
    /* Handle here slider of current position */
    seekPositionSlider.value = event.target.currentTime;
});

isolaattiMixer.prepareMix(function(){
    console.log("Mixer is ready!!");
    document.querySelector("#play_pause_button").disabled = false;
    defineEvents();
});

function defineEvents() {
    // set events here
    let playButton = document.querySelector("#play_pause_button");
    playButton.addEventListener("click", function() {
        if(isolaattiMixer.playing){
            isolaattiMixer.pauseMix();
        } else {
            isolaattiMixer.playMix();
        }
    });

    
    seekPositionSlider.max = isolaattiMixer.getDuration();
    seekPositionSlider.addEventListener("input", function(event){
       /* set audio position here */ 
    });
}

