/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

class Track {
    constructor(name, sourceMediaElement) {
        this.name = name;
        this.source = sourceMediaElement;
        this.trackContext = new AudioContext();
        this.track = this.trackContext.createMediaElementSource(sourceMediaElement);
        this.gainEffect = this.trackContext.createGain();
        this.track
            .connect(this.gainEffect)
            .connect(this.trackContext.destination);
        
    }

    play() {
        if(this.trackContext.state === 'suspended') {
            this.trackContext.resume();
        }
        this.source.play();
    }
    
    pause() {
        this.source.pause();
    }
    
    stop() {
        this.source.stop();
    }

    /* value should be between 0 and 2*/
    setGain(value) {
        this.gainEffect.gain.value = value;
    }
    
    getGain() {
        return this.gainEffect.gain.value;
    }
    
}

class Mix {
    tracks = [];

    constructor() {
        this.playing = false;
    }

    play() {
        for(var i=0; i<this.tracks.length; i++) {
            this.tracks[i].play();
        }
        this.playing = true;
    }
    
    pause() {
        for(var i=0; i<this.tracks.length; i++) {
            this.tracks[i].pause();
        }
        this.playing = false;
    }
    
    stop() {
        this.tracks.forEach(function(value){
            value.stop();
        });
        this.playing = false;
    }
    
    setGain(masterGain) {
        this.tracks.forEach(function(value,index) {
           value.setGain(tracksSliders.item(index).value * masterGain); 
        });
    }
    
    addTrack(track){
        this.tracks.push(track);
    }
    
    getTracks() {
        return this.tracks;
    }
    
    isPlaying() {
        return this.playing;
    }
    
    setOnEnded(callbackFunction) {
        this.tracks[0].source.addEventListener("ended", callbackFunction );
    }
}

////////////////////////////////////////////////////////////////////////
// The four source tracks from DOM
const bassSource = document.getElementById("media_bass")
const drumsSource = document.getElementById("media_drums");
const vocalsSource = document.getElementById("media_vocals");
const otherSource = document.getElementById("media_other");

const allTracksDownloadedMonitoring = new Worker('/js/monitoring_tracks_availability.js');
allTracksDownloadedMonitoring.postMessage(
    {
        numberOfTracks:4
    });

bassSource.addEventListener("canplaythrough", function() {
    allTracksDownloadedMonitoring.postMessage("+1")
});

drumsSource.addEventListener("canplaythrough", function() {
    allTracksDownloadedMonitoring.postMessage("+1");
});

vocalsSource.addEventListener("canplaythrough", function() {
   allTracksDownloadedMonitoring.postMessage("+1"); 
});

otherSource.addEventListener("canplaythrough", function() {
   allTracksDownloadedMonitoring.postMessage("+1"); 
});


////////////////////////////////////////////////////////////////////////
// Creates tracks (for now there are only 4 tracks, so they are hardcoded)
let bassTrack = new Track("bass",bassSource);
let drumsTrack = new Track("drums", drumsSource);
let vocalsTrack = new Track("vocals", vocalsSource);
let otherTrack = new Track("other", otherSource);

// adds tracks to a list
let tracks = [];
tracks.push(bassTrack,drumsTrack,vocalsTrack,otherTrack);

// Creates mix and adds tracks
let mix = new Mix();
mix.addTrack(bassTrack);
mix.addTrack(drumsTrack);
mix.addTrack(vocalsTrack);
mix.addTrack(otherTrack);
mix.setOnEnded(function(){
    playPauseButton.innerHTML = '<i class="fas fa-play"></i>';
});

// references to buttons in DOM
let playPauseButton = document.getElementById("play_pause_button");
let stopButton = document.getElementById("stop_button");
let resetGainsButton = document.getElementById("reset_gains");


// references to sliders in DOM
let masterGain = document.getElementById("mix_gain");
let tracksSliders = document.querySelectorAll(".track-gain");

allTracksDownloadedMonitoring.onmessage = function(event) {
    if(event.data === "completed"){
        playPauseButton.disabled = false;
    }
}

////////////////////////////////////////////////////////////////////////
// define events for buttons
playPauseButton.addEventListener('click', function() {
   if(mix.isPlaying()) {
       mix.pause();
       playPauseButton.innerHTML = '<i class="fas fa-play"></i>';
   } else {
       mix.play();
       playPauseButton.innerHTML = '<i class="fas fa-pause"></i>';
   }
});

stopButton.addEventListener('click', function() {
   if(mix.isPlaying()) {
       mix.stop();
   } 
});

resetGainsButton.addEventListener("click", function() {
    tracksSliders.forEach(function(item) {
        item.value = 1.0;
        item.title = (parseFloat(item.value) * 100) + "%";
    });
    tracks.forEach(function(item) {
        item.setGain(1.0);
    });
});

////////////////////////////////////////////////////////////////////////
// events for sliders
masterGain.addEventListener("input", function() {
    mix.setGain(parseFloat(masterGain.value));
    masterGain.title = (parseFloat(masterGain.value) * 100) + "%";
});

tracksSliders.forEach(function (value,index) {
    value.addEventListener("input", function() {
        tracks[index].setGain(parseFloat(value.value));
        value.title = (parseFloat(value.value) * 100) + "%";
    });
});
