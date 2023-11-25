/*
* Isolaatti project, 2022
* Erik Cavazos
* Audios list component
*/


Vue.component("audios-list-select", {
    data: function () {
        return {
            userData: userData
        }
    },
    methods: {
        audioSelected: function (audioId) {
            this.$emit('audio-selected', audioId);
        }
    },
    template: `
      <section>
      <div class="d-flex justify-content-center">
        <button type="button" class="btn btn-primary btn-sm" data-toggle="modal"
                data-target="#modalSelectExistingAudio">
          Seleccionar audio existente
        </button>
      </div>


      <!-- Modal -->
      <div class="modal fade" id="modalSelectExistingAudio">
        <div class="modal-dialog">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">Seleccionar audio existente</h5>
              <button type="button" class="close" data-dismiss="modal">
                <span aria-hidden="true">&times;</span>
              </button>
            </div>
            <div class="modal-body">
              <audios-list :user-id="userData.id" v-on:audio-selected="audioSelected"></audios-list>
            </div>
          </div>
        </div>
      </div>
      </section>
    `
});

Vue.component("audios-list", {
    props: {
        userId: {
            type: Number,
            required: true
        },
    },
    data: function () {
        return {
            customHeaders: customHttpHeaders,
            audios: [],
            loadMore: true
        }
    },
    computed:  {
        fetchUrl: function() {
            if(this.audios.length < 1) {
                return `/api/Audios/OfUser/${this.userId}`;
            } else {
                return `/api/Audios/OfUser/${this.userId}?lastAudioId=${this.audios[this.audios.length - 1].id}`;
            }
        }
    },
    methods: {
        fetchAudios: function () {
            fetch(this.fetchUrl, {
                method: "GET",
                headers: this.customHeaders
            }).then(res => res.json()).then((audios) => {
                this.audios = this.audios.concat(audios.data);
                this.loadMore = audios.length >= 10;
            });

        },
        onAudioDeleted: function(audioId){
            const index = this.audios.findIndex(audio=>audio.id === audioId);
            console.log("Index to remove",index);
            if(index >= 0){
                this.audios.splice(index, 1);
            }
        }
    },
    mounted: function () {
        this.fetchAudios();
    },
    template: `
      <div>
      <p class="m-2 text-center" v-if="audios.length === 0"> No hay contenido que mostrar <i
          class="fa-solid fa-face-sad-cry"></i></p>
      <div class="d-flex flex-column">
        <audio-attachment :audio-object="audio" v-for="audio in audios" v-on:select="$emit('audio-selected',audio.id)"
                          class="mt-1" :key="audio.id" @audioDeleted="onAudioDeleted"></audio-attachment>
        <div class="d-flex mt-2 justify-content-center">
          <button class="btn" v-if="loadMore" @click="fetchAudios">
            Cargar más
          </button>
        </div>
      </div>
      </div>
    `
})