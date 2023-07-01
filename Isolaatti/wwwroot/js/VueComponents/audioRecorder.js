/*
* Isolaatti project, 2022
* Erik Cavazos
* Audio recorder component
*/

Vue.component('audio-recorder', {
    props: {},
    data: () => {
        return {
            // global variables
            customHeaders: customHttpHeaders,
            userData: userData,
            uploading: false,

            status: {
                recording: false,
                recorded: false,
                playing: false,
                paused: false,
                consolidated: false,

                errorAccessingMedia: false
            },
            recorder: {
                timeInSeconds: 0,
                mediaRecorder: null,
                mediaStream: null,
                audioData: [],
                resultantBlob: null
            },

            player: {
                audioPlayer: new Audio(),
                audioPlayerTimeIntervalId: 0,
                timeInSeconds: 0
            },

            audioName: ""
        }
    },
    computed: {
        progressStyle: function () {

            return (this.status.recorded || this.status.recording) && !this.status.playing && !this.status.paused ?
                `width:${(this.recorder.timeInSeconds * 100) / 120}%` :
                `width:${(this.player.timeInSeconds * 100) / this.recorder.timeInSeconds}%`
        },
        clockFormatTime: function () {
            let truncatedSecs =
                (this.status.recorded || this.status.recording) && !this.status.playing && !this.status.paused
                    ? Math.round(this.recorder.timeInSeconds) : Math.round(this.player.timeInSeconds);
            let truncatedTotalSecs = (this.status.recorded || this.status.recording) && !this.status.playing && !this.status.paused
                ? 120 : Math.round(this.recorder.timeInSeconds);

            return `${this.getClockFormatTime(truncatedSecs)} / ${this.getClockFormatTime(truncatedTotalSecs)}`;
        },
        progressIndicatorCssClass: function () {
            return `progress-bar progress-bar-striped progress-bar-animated ${(this.status.recorded || this.status.recording) && !this.status.playing && !this.status.paused ? "bg-danger" : ""}`;
        }
    },
    methods: {
        getClockFormatTime: function (secs) {
            let truncatedSecs = Math.round(secs)
            let minutes = truncatedSecs / 60;
            let seconds = truncatedSecs % 60;
            if (seconds < 10) {
                seconds = `0${seconds}`
            }
            return `${Math.trunc(minutes)}:${seconds}`;
        },

        requestMicrophone: function () {
            let audioStreamPromise = navigator.mediaDevices.getUserMedia({audio: true, video: false})

            audioStreamPromise.then((stream) => {
                this.recorder.mediaStream = stream;
                this.status.errorAccessingMedia = false;
            }).catch(error => {
                this.status.errorAccessingMedia = true;
            });
        },
        startRecording: function () {
            this.status.consolidated = false;
            this.recorder.timeInSeconds = 0;
            this.recorder.mediaRecorder = new MediaRecorder(this.recorder.mediaStream, {
                mimeType: 'audio/webm; codecs=opus',
                bitsPerSecond: 128000
            });
            this.recorder.mediaRecorder.start();
            let globalThis = this;

            // start interval that will be updating the progress bar.
            // stop this interval when recording ends
            let interval = setInterval(function () {
                globalThis.recorder.timeInSeconds += 1;
            }, 1000);

            // This timer is what limits the duration up to 2 minutes (120 seconds)
            let timer = setTimeout(function () {
                globalThis.stopRecording();
                clearInterval(interval);
            }, 120 * 1000);

            this.recorder.mediaRecorder.ondataavailable = function (e) {
                globalThis.recorder.audioData.push(e.data);
                console.log("Data pushed...");
            }
            this.recorder.mediaRecorder.onstop = function () {
                clearTimeout(timer);
                clearInterval(interval);
                globalThis.recorder.resultantBlob =
                    new Blob(globalThis.recorder.audioData, {'type': 'audio/webm; codecs=opus'});
            }
            this.status.recording = true;
        },
        stopRecording: function () {
            this.recorder.mediaRecorder.stop();
            this.status.recording = false;
            this.status.recorded = true;
            this.recorder.mediaStream.getTracks().forEach(track => track.stop());
            this.recorder.mediaStream = null;
        },
        playRecord: function () {
            this.status.playing = true;
            this.player.audioPlayer.src = window.URL.createObjectURL(this.recorder.resultantBlob);
            this.player.audioPlayer.volume = 1;
            if (this.status.paused) {

            }
            this.player.audioPlayer.play().then(function () {
                console.log("Playing recorded audio");
            }).catch(function (error) {
                console.error("Can't play recorded audio: " + error);
            });
        },
        pauseRecord: function () {
            this.player.audioPlayer.pause();
        },
        resetRecording: function () {
            this.status.recording = false;
            this.status.recorded = false;
            this.status.paused = false;
            this.status.playing = false;
            this.recorder.resultantBlob = null;
            this.recorder.audioData = [];
            this.player.audioPlayer.pause();
            this.status.consolidated = false;
            this.recorder.timeInSeconds = 0;
            this.player.timeInSeconds = 0;
        },
        consolidate: function () {
            this.status.consolidated = true;
            this.resetRecording();
            this.postAudio();
        },
        
        resetState: function() {
            this.$emit('remount')
        },

        postAudio: function () {
            const request = new XMLHttpRequest();
            const that = this;
            this.uploading = true;

            request.onload = function () {
                const response = JSON.parse(request.response);
                that.$emit("audio-posted", response.id);
                that.uploading = false;
                that.resetState();
            }

            request.open("POST", "/api/Audios/Create");
            request.setRequestHeader("Authorization", decodeURIComponent(authorization));
            const formData = new FormData();
            formData.append("name", this.audioName);
            formData.append("audioFile", this.recorder.resultantBlob);
            formData.append("duration", this.recorder.timeInSeconds);
            request.send(formData);
        }
    },
    mounted: function () {
        this.audioName = `Nuevo audio - ${Date.now()}`
        const globalThis = this;
        this.player.audioPlayer.onplaying = function () {
            globalThis.player.audioPlayerTimeIntervalId = 0;
            globalThis.player.audioPlayerTimeIntervalId = setInterval(function () {
                globalThis.player.timeInSeconds = globalThis.player.audioPlayer.currentTime;
            }, 1000)
        }
        this.player.audioPlayer.onended = function () {
            clearInterval(globalThis.player.audioPlayerTimeIntervalId);
            globalThis.player.timeInSeconds = 0;
            globalThis.status.playing = false;
        }
        this.player.audioPlayer.onpause = function () {
            globalThis.status.paused = true;
            globalThis.status.playing = false;
        }
    },
    template: `
      <div class="d-flex flex-column bg-light p-3 rounded">
      <div class="alert alert-danger" v-if="status.errorAccessingMedia">
        No es posible acceder a tu micrófono. Verifica su disponibilidad.
      </div>
      <input type="text" class="form-control mb-1" v-model="audioName">
      <div class="progress">
        <div :class="progressIndicatorCssClass" role="progressbar" :style="progressStyle" aria-valuenow="10"
             aria-valuemin="0" aria-valuemax="100"></div>
      </div>
      <span>{{ clockFormatTime }}</span>
      <div class="d-flex" v-if="!uploading">
        <!--          * This group shows to allow user to start recording *-->
        <div class="btn-group" v-if="!status.recording && !status.recorded">
          <button class="btn btn-danger" v-on:click="startRecording()" v-if="recorder.mediaStream !== null">
            Comenzar a grabar
          </button>
          <button class="btn btn-light" v-else v-on:click="requestMicrophone">Pedir micrófono</button>
        </div>

        <!--           This group is shown when an audio is being recorded-->
        <div class="btn btn-group" v-if="status.recording">
          <button class="btn btn-primary" v-on:click="stopRecording()">
            Detener
          </button>
        </div>

        <!--          This group shows when an audio has been recorded-->
        <div v-if="status.recorded" class="mr-auto">
          <button class="btn btn-info" v-on:click="playRecord()" v-if="!status.playing">
            <i class="fas fa-play"></i>
          </button>
          <button class="btn btn-info" v-on:click="pauseRecord()" v-else>
            <i class="fas fa-pause"></i>
          </button>
        </div>
        <div class="btn-group" v-if="status.recorded">

          <button class="btn btn-dark" v-on:click="resetRecording()">
            Descartar
          </button>
          <button class="btn btn-primary" v-on:click="postAudio">
            Continuar
          </button>
        </div>
      </div>
      <div v-else>
        Subiendo audio...
      </div>
      </div>
    `
})