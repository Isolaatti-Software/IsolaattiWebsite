Vue.component("audio-attachment", {
    props: {
        audioObject: {
            type: Object,
            required: false,
            default: null
        },
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
            userName: "",
            userId: -1,
            playbackStatus: {
                selected: false,
                playing: false,
                paused: false
            }
        }
    },
    methods: {
        play: async function () {
            await audioService.playPause(this.audioId);
        }
    },
    computed: {
        profileRelativeUrl: function () {
            return `/perfil/${this.userId}`;
        }
    },
    mounted: function () {
        const that = this;
        
        this.$nextTick(function () {
            // Only feteches data when the data is not passed as property. Data is usually passed when
            // this component is in a list, but not passed when it is on a discussion
            if(that.audioObject === null) {
                fetch(`/api/Audios/${that.audioId}`, {
                    method: "GET",
                    headers: customHttpHeaders
                }).then(res => res.json())
                    .then(audioMetadata => {
                        that.name = audioMetadata.name;
                        that.userName = audioMetadata.userName;
                        that.userId = audioMetadata.userId;
                    });
            } else {
                that.name = that.audioObject.name;
                that.userName = that.audioObject.userName;
                that.userId = that.audioObject.userId
            }
        })

        // I need to listen to these events to update UI when another 
        // audio on the whole system starts playing
        events.$on("audios.state", function (state, id) {
            if(state === "ended" && id === that.audioId) {
                that.playbackStatus.playing = false;
                that.playbackStatus.paused = false;
            }
            
            if(state === "playing") {
                if(id === that.audioId) {
                    that.playbackStatus.selected = true;
                    that.playbackStatus.playing = true;
                    that.playbackStatus.paused = false;
                } else {
                    that.playbackStatus.selected = false;
                    that.playbackStatus.playing = false;
                    that.playbackStatus.paused = false;
                }
            }
            
            if(state === "paused") {
                if(id === that.audioId) {
                    that.playbackStatus.selected = true;
                    that.playbackStatus.playing = false;
                    that.playbackStatus.paused = true;
                } else {
                    that.playbackStatus.selected = false;
                    that.playbackStatus.playing = false;
                    that.playbackStatus.paused = false;
                }
            }
        });
    },
    template: `
      <div class="audio-attachment-component" v-on:click="$emit('click')">
      <div class="btn-group-sm" v-if="playbackStatus.url !== null">
        
        <button type="button" class="btn btn-light btn-sm" v-on:click="play" v-if="playbackStatus.playing">
          <i class="fas fa-pause"></i>
        </button>
        <button type="button" class="btn btn-light btn-sm" v-on:click="play"
                v-else>
          <i class="fas fa-play"></i>
        </button>
        </div>
      <div class="d-flex flex-column overflow-hidden">
        <p class="m-0 text-ellipsis" :class="{'text-primary font-weight-bolder':playbackStatus.selected, 'text-black-50':!playbackStatus.selected}">{{name}}</p>
        <p class="text-black-50 m-0"><a :href="profileRelativeUrl">{{userName}}</a></p>
      </div>
      <button type="button" class="close ml-auto" v-if="canRemove" v-on:click="$emit('remove')">&times;</button>
      </div>
    `
})