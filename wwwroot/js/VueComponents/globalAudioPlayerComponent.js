Vue.component("audio-player", {
    data: function () {
        return {
            name: "",
            audioId: undefined,
            playbackStatus: {
                url: undefined,
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
        this.$nextTick(function () {

        })

        audioPlayer.$on("play", function (url, type, id) {
            if (id === that.audioId)
                return;

            fetch(`api/Audios/${id}`, {
                method: "GET",
                headers: customHttpHeaders
            }).then(res => res.json())
                .then(audioMetadata => {
                    that.name = audioMetadata.name
                });

            console.log("Recibo play");
            that.audioId = id;
            that.playbackStatus.url = url;
            that.playbackStatus.playing = true;
            that.playbackStatus.paused = false;
            that.playbackStatus.stopped = false;

        });
        audioPlayer.$on("pause", function (url) {
            that.playbackStatus.stopped = false;
            that.playbackStatus.playing = false;
            that.playbackStatus.paused = true;

        });
        audioPlayer.$on("stop", function (url) {
            that.playbackStatus.stopped = true;
            that.playbackStatus.playing = false;
            that.playbackStatus.paused = false;
        });
    },
    template: `
    <div class="global-audio-player" v-if="playbackStatus.url!==undefined">
      <div class="d-flex align-items-center">
        <div class="d-flex align-items-center p-1 w-100">
          <div class="btn-group-sm" v-if="playbackStatus.url !== undefined">
            <button type="button" class="btn btn-outline-light btn-sm" v-on:click="play" v-if="playbackStatus.paused || playbackStatus.stopped">
              <i class="fas fa-play"></i>
            </button>
            <button type="button" class="btn btn-outline-light btn-sm" v-on:click="pause" v-if="playbackStatus.playing">
              <i class="fas fa-pause"></i>
            </button>
            <button type="button" class="btn btn-outline-light btn-sm" v-on:click="stop" v-if="playbackStatus.paused || playbackStatus.playing">
              <i class="fas fa-stop"></i>
            </button>
          </div>
          <span class="text-black-50 ml-1">{{name}}</span>
        </div>
      </div>

    </div>
    `
})