const audioService =  (function() {
    // private
    
    let audioId = undefined;
    let audioName = undefined;
    let username = undefined;
    const audioContext = new AudioContext();
    const audio = new Audio();
    const audioSource = audioContext.createMediaElementSource(audio);
    const gainNode = audioContext.createGain();
    let duration = 10000000;
    
    audio.crossOrigin = "anonymous"
    
    // audio graph setup
    audioSource
        .connect(gainNode)
        .connect(audioContext.destination);

    function emitState(state) {
        events.$emit("audios.state", state, audioId);
    }
    
    async function emitAudioInfo() {
        const audioInfo = await (await fetch(`/api/Audios/${audioId}`, {
            method: "get",
            headers: customHttpHeaders
        })).json();
        events.$emit("audios.info", audioInfo.name, audioInfo.userName, audioInfo.userId);
    }
    
    function audioDownloadUrl(id) {
        return `/api/Audios/${id}.webm`;
    }
    
    audio.addEventListener("canplaythrough", async function () {
        await audio.play();
        emitState("playing");
        await emitAudioInfo();
    });
    
    audio.addEventListener("ended", function() {
        emitState("ended");
    });
    
    audio.addEventListener("durationchange", function() {
        if(audio.duration !== Infinity) {
            duration = audio.duration;
            events.$emit("audios.duration");
        }
    })
    
    // public
    return {
        playPause: async function(id) {
            if(audioContext.state === "suspended"){
                await audioContext.resume();
            }
            
            // user wants to resume from paused or ended
            if(audioId === id) {
                if(audio.paused || audio.ended) {
                    await audio.play();
                    emitState("playing");
                } else {
                    audio.pause();
                    emitState("paused");
                }
            } 
            // user want to play a new audio
            else {
                audioId = id;
                audio.src = audioDownloadUrl(id);
            }
        },
        setGain: function(value) {
            gainNode.gain.value = value;
        },
        getAudioDuration: function() {
            return duration;
        },
        getCurrentTime: function() {
          return audio.currentTime;  
        },
        seek: function(second) {
            audio.currentTime = second;
        }
    }
})();