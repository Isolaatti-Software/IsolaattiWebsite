Vue.component('new-post', {
    data: function() {
        return {
            mode: undefined, // or audio
            example: {
                template: "<p>Hola</p>"
            },
            audioRecorderKey: Date.now() // this is changed to force remount of the component
        }
    },
    methods: {
        onAudioRecorderRemount: function() {
            this.audioRecorderKey = Date.now()
        }
    },
    mounted: function() {
        const that = this;
    },
    template: `
      <div class="max-640 mt-4 p-3 isolaatti-card" style="">
      <div class="card-body">
        <h5 class="card-title"><i class="fa-solid fa-plus"></i> Nuevo</h5>
        

        
        <div class="max-640 mt-1">
            <div class="d-flex justify-content-end mb-1" v-if="mode !== undefined">
              <button class="btn btn-light btn-sm" @click="mode=undefined">&times;</button>
            </div>
            <new-discussion></new-discussion>
        </div>
      </div>
      </div>
    `
})