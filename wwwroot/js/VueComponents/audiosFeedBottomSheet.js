Vue.component('audio-card', {
    props: {
        audio: {
            type: Object,
            required: true
        }
    },
    computed: {
        userImage: function () {
            return `/api/Fetch/GetUserProfileImage?userId=${this.audio.userId}`
        }
    },
    template: `
      <div class="audio-card">
        <img :src="userImage" class="h-100 ml-auto mr-auto"/>
        <audio-attachment :audio-id="audio.id" :can-remove="false" class="mt-auto"></audio-attachment>
      </div>
    `
});

Vue.component('bottom-sheet-audio-feed', {
    props: {
        state: {
            type: String,
            required: true
        }
    },
    data: function () {
        return {
            customHeaders: customHttpHeaders,
            audios: []
        }
    },
    methods: {
        fetchAudiosFeed: async function () {
            const response = await fetch('/api/Audios/Feed', {
                method: "get",
                headers: this.customHeaders
            });
            this.audios = await response.json();
        }
    },
    mounted: async function () {
        await this.fetchAudiosFeed();
    },
    template: `
    <div class="audios-feed-bottom-sheet" :class="{
      'expand': state==='show', 
      'hide': state==='hide', 
      'collapse': state==='collapse' 
    }" v-if="state!=='hide'">
    <h1 class="text-white">Escucha al mundo</h1>
      <div class="audios-feed-bottom-sheet-container">
        <audio-card v-for="audio in audios" :audio="audio" :key="audio.id"></audio-card>
      </div>
      
    </div>
    `
})