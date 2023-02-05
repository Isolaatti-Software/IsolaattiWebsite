Vue.component('squad-requests', {
    props: {
        squadId: {
            type: String,
            required: false
        },
        userId: {
            type: Number,
            required: false
        }
    },
    data: function() {
        return {
            customHeaders: customHttpHeaders,
            requests: [],
            loading: true,
            error: false
        }
    },
    methods: {
        fetchRequests: async function() {
            this.loading = true;
            const response = await fetch(`/api/Squads/${this.squadId}/JoinRequests`, {
                headers: this.customHeaders
            });
            this.loading = false;
            if(response.ok) {
                this.requests = await response.json();
                console.log(this.requests);
            } else {
                this.error = true
            }

        }
    },
    mounted: async function() {
        await this.fetchRequests();
    },
    template: `
    <section class="isolaatti-card">
      <template v-if="requests.length === 0 && !loading">
        <p class="m-2 text-center"> No hay contenido que mostrar <i
            class="fa-solid fa-face-sad-cry"></i></p>
      </template>
      <div v-if="loading" class="d-flex justify-content-center mt-2">
          <div class="spinner-border" role="status">
            <span class="sr-only">Cargando más contenido...</span>
          </div>
      </div>
      <squad-requests-list :items="requests"></squad-requests-list>
    </section>
    `
});
