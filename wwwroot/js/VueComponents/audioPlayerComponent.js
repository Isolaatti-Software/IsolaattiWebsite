Vue.component('audio-player', {
    data: function() {
        return {
            state: "waiting", // playing, ended, paused
            audioId: undefined,
            loaded: false,
            time: 0,
            duration: 0.0,
            name: "",
            userName: "",
            userId: 0
        }
    },
    computed: {
      userProfile: function() {
          return `/perfil/${this.userId}`;
      },
      userImage: function() {
          return `/api/Fetch/GetUserProfileImage?userId=${this.userId}`;
      }  
    },
    methods: {
        play: async function() {
          await audioService.playPause(this.audioId);
        },
        seek: function(e) {
            audioService.seek(e.target.value);
        }
    },
    mounted: async function() {
        const that = this;
        
        // this is the interval to update the seeking bar on the audio player
        let intervalId = undefined;
        
        function setUpdateInterval() {
            intervalId = setInterval(function() {
                that.time = audioService.getCurrentTime();
            }, 1000)
        }
        
        function stopUpdateInterval() {
            clearInterval(intervalId);
        }
        
        events.$on("audios.state", function(state, id) {
            that.loaded = true;
            that.state = state;
            that.audioId = id;
            switch(state) {
                case "playing":
                    setUpdateInterval();
                    break;
                case "ended":
                    stopUpdateInterval();
                    setTimeout(function() {
                        that.time = 0;
                    }, 1000);
                    
                    break;
            }
        });
        
        events.$on("audios.info", function(name, userName, userId) {
            that.name = name;
            that.userName = userName;
            that.userId = userId;
        });
        
        events.$on("audios.duration", function() {
            that.duration = audioService.getAudioDuration();
        });
    },
    template: `
<section class="d-flex mb-1 p-3 w-100 bg-light align-items-center" v-if="loaded">
    <img width="50" height="50" :src="userImage" class="mr-1" alt="Foto del usuario">
    <div class="d-flex flex-column">
      <div>{{name}}</div>
      <div><a :href="userProfile">{{userName}}</a></div>
    </div>
<!--    <button class="btn">-->
<!--      <i class="fa-solid fa-angle-up"></i>-->
<!--    </button>-->
    <div class="d-flex flex-column mr-2 ml-2 flex-fill align-items-center">
      <button class="btn"  @click="play">
        <i v-if=" state==='ended' || state==='paused' " class="fa-solid fa-play"></i>
        <i v-if=" state==='playing'" class="fa-solid fa-pause"></i>
      </button>
      <input type="range" class="form-control-range" 
             style="max-width: 400px" 
             min="0" 
             :max="duration" 
             :value="time" 
             @input="seek">
    </div>
    <div>
      <button class="btn">
        <i class="fa-solid fa-volume-high"></i>
      </button>
    </div>
</section>
    `
});