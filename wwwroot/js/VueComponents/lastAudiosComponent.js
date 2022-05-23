Vue.component('last-audios', {
    data: function () {
        return {
            customHeaders: customHttpHeaders,
            audios: [],
            loading: true
        }
    },
    mounted: function () {
        fetch("/api/Audios/Newest", {
            method: "GET",
            headers: this.customHeaders
        }).then(res => res.json())
            .then(data => {
                this.audios = data;
                this.loading = false;
            });
    },
    template: `
      <section class="d-flex flex-column">
      <h5 class="mt-2"><i class="fa-solid fa-ear-listen"></i> Escucha al mundo</h5>
      <div v-if="loading" class="d-flex justify-content-center mt-2">
        <div class="spinner-border" role="status">
          <span class="sr-only">Cargando más contenido...</span>
        </div>
      </div>
      <audio-attachment v-for="audio in audios" :audio-id="audio.id" class="mb-1"></audio-attachment>
      </section>
    `
})