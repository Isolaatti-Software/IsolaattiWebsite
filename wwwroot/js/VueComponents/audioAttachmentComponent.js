Vue.component("audio-attachment", {
    props: {
        audioId: {
            type: String,
            required: false
        },
        canRemove: {
            type: Boolean,
            default: false,
            required: false
        }
    },
    data: function () {
        return {
            name: "",
            playbackStatus: {
                url: "",
                playing: false,
                paused: false,
                stopped: true
            }
        }
    },
    methods: {
        play: function () {
            audioPlayer.play(this.playbackStatus.url, "post", this.audioId);
        },
        pause: function () {
            audioPlayer.pause();
        },
        stop: function () {
            audioPlayer.stop();
        }
    },
    mounted: function () {
        const that = this;

        this.playbackStatus.url = `/api/Audios/${this.audioId}/Play`;
        this.$nextTick(function () {
            fetch(`api/Audios/${that.audioId}`, {
                method: "GET",
                headers: customHttpHeaders
            }).then(res => res.json())
                .then(audioMetadata => {
                    that.name = audioMetadata.name
                });
        })

        // I need to listen to these events to update UI when another 
        // audio on the whole system starts playing
        audioPlayer.$on("play", function (url) {
            console.log("Recibo play");
            if (url === that.playbackStatus.url) {
                that.playbackStatus.playing = true;
                that.playbackStatus.paused = false;
                that.playbackStatus.stopped = false;
            }
        });
        audioPlayer.$on("pause", function (url) {

            if (url === that.playbackStatus.url) {
                that.playbackStatus.stopped = false;
                that.playbackStatus.playing = false;
                that.playbackStatus.paused = true;
            }
        });
        audioPlayer.$on("stop", function (url) {
            if (url === that.playbackStatus.url) {
                that.playbackStatus.stopped = true;
                that.playbackStatus.playing = false;
                that.playbackStatus.paused = false;
            }
        });
    },
    template: `
      <div class="audio-attachment-component" v-on:click="$emit('click')">
      <div class="btn-group-sm" v-if="playbackStatus.url !== null">
        <button type="button" class="btn btn-light btn-sm" v-on:click="play"
                v-if="playbackStatus.paused || playbackStatus.stopped">
          <i class="fas fa-play"></i>
        </button>
        <button type="button" class="btn btn-light btn-sm" v-on:click="pause" v-if="playbackStatus.playing">
          <i class="fas fa-pause"></i>
        </button>
        <button type="button" class="btn btn-light btn-sm" v-on:click="stop"
                v-if="playbackStatus.paused || playbackStatus.playing">
          <i class="fas fa-stop"></i>
        </button>
        </div>
          <span class="text-black-50">{{name}}</span>
          <button type="button" class="close ml-auto" v-if="canRemove" v-on:click="$emit('remove')">&times;</button>
        </div>
    `
})