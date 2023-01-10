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
    </section>
    `
});
