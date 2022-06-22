/*
* Isolaatti project, 2022
* Erik Cavazos
* Audios list component
*/


Vue.component("audios-list-select", {
    data: function () {
        return {}
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
              <audios-list :user-id="1" v-on:audio-selected="audioSelected"></audios-list>
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
            audios: []
        }
    },
    methods: {
        fetchAudios: function () {
            fetch(`/api/Audios/OfUser/${this.userId}`, {
                method: "GET",
                headers: this.customHeaders
            }).then(res => res.json()).then((audios) => {
                this.audios = this.audios.concat(audios);
            });

        }
    },
    mounted: function () {
        this.fetchAudios();
    },
    template: `
      <div>
      <h5 class="mt-2 w-100 text-center"><i class="fa-solid fa-ear-listen"></i> Audios</h5>
      <p class="m-2 text-center" v-if="audios.length === 0"> No hay contenido que mostrar <i
          class="fa-solid fa-face-sad-cry"></i></p>
      <div class="d-flex flex-column">
        <audio-attachment :audio-id="audio.id" v-for="audio in audios" v-on:click="$emit('audio-selected',audio.id)"
                          data-dismiss="modal" class="mt-1"></audio-attachment>
      </div>
      </div>
    `
})