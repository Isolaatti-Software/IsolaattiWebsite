
/*
About this program
 Erik Cavazos, 2021.
 The Isolaatti Project audio mixer.
 This library uses the WEBAudio API and is intended to be used in the browser.

MIT License

Copyright (c) 2021 Erik Everardo

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

class IsolaattiAudioMixer {
    // constants to define effect
    static TYPICAL_REVERB = 1;
    static LOW_PASS_FILTER = 2;
    // add more here...

    // pass an array of html audio elements
    constructor(mediaElements, onTimeChangeCallback) {
        this.audioElements = mediaElements;
        this.tracks = new Map();
        this.numberOfTracksThatCanPlayTrough = 0;
        this.onTimeChangeCallback = onTimeChangeCallback;
        this.audioContext = new AudioContext();
        
        this.mainGainNode = this.audioContext.createGain();
        this.mainDynamicsCompressor = this.audioContext.createDynamicsCompressor();

        this.mainGainNode.connect(this.mainDynamicsCompressor).connect(this.audioContext.destination);

        globalThis = this;


        // states
        this.prepared = false;
        this.playing = false;
    }

    prepareMix(onPrepared) {
        // creates audioSources and use them to create a Track class object
        this.audioElements.forEach(function(value) {
            let track = new Track(
                globalThis.audioContext.createMediaElementSource(value),
                globalThis.audioContext
            );
            globalThis.tracks.set(value.attributes.getNamedItem("track-name").value, track);
            value.oncanplaythrough = function(){
                globalThis.numberOfTracksThatCanPlayTrough++;
                if(globalThis.numberOfTracksThatCanPlayTrough === globalThis.tracks.size){
                    console.info("All audio elements can play trough. Creating tracks...");
                    globalThis.audioElements[0].ontimeupdate = globalThis.onTimeChangeCallback;
                    globalThis.tracks.forEach(function(value,key) {
                        value.getLastNode().connect(globalThis.mainGainNode);
                        console.info("Track created: " + key);
                    });
                    onPrepared();
                    globalThis.prepared = true;
                }
            };
        });
    }

    playMix() {
        globalThis.playing = true;
        if(globalThis.prepared) {
            if (globalThis.audioContext.state === 'suspended') {
                globalThis.audioContext.resume();
            }
            console.log("Playing now...");
            globalThis.audioElements.forEach(function(it){
                it.play();
            });
        } else {
            throw {
                name: "MixerNotPrepared",
                message: "Cannot play, mix is not prepared. Call to method prepare()"
            } 
        }
    }

    setMainGainValue(value) {
        globalThis.mainGainNode.gain.value = value;
    }

    setGainOfTrack(nameOfTrack, value) {
        globalThis.tracks.get(nameOfTrack).setGainValue(value);
    }

    // For now this method assumes all tracks are same long. 
    // Fix this in the future!
    getDuration(){
        return globalThis.audioElements[0].duration;
    }

    // Return the blob of the mix
    exportMix() {

    }
}

class Track {
    constructor(audioSource, audioContext) {
        this.gainNode = audioContext.createGain();
        this.stereoPanning = audioContext.createStereoPanner();
        this.dynamicsCompressor = audioContext.createDynamicsCompressor();

        // create nodes for effects here
        // ...
        // ...

        // connect nodes
        audioSource
                    .connect(this.gainNode)
                    .connect(this.stereoPanning);
    }

    setGainValue(value) {
        this.gainNode.gain.value = value;
    }
    // use the node returned to connect it to the main gain node
    getLastNode() {
        return this.stereoPanning;
    }
}