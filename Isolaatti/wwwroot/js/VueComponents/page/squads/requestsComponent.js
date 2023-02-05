const requestsComponent = {
    data: function() {
        return {
            customHeaders: customHttpHeaders,
            requests: [],
            loading: true,
            error: false,
            editionMode: false,
            overallMode: "forMe" //or fromMe
        }
    },
    methods: {
        fetchRequestsForMe: async function() {
            this.loading = true;
            const response = await fetch(`/api/Squads/JoinRequests/JoinRequestsForMe`, {
                headers: this.customHeaders
            });
            try {
                this.requests = (await response.json());
            } catch (e) {
                this.error = true;
            }
            this.loading = false;
        },
        fetchRequestsFromMe: async function() {
            this.loading = true;
            const response = await fetch(`/api/Squads/JoinRequests/MyJoinRequests`, {
                headers: this.customHeaders
            });
            try {
                this.requests = (await response.json());
            } catch(e) {
                this.error = true;
            }
            this.loading = false;
        },
        onRequestUpdate: function(invitation) {
            const index = this.invitations.findIndex(inv => inv.invitation.id === invitation.id);
            const temp = this.invitations;
            temp[index].invitation = invitation;
            this.invitations = temp;
        },
        onDeleted: function(requestId) {
            const index = this.requests.findIndex(req => req.request.id === requestId);
            this.requests.splice(index, 1);
        }
    },
    watch:{
        overallMode: {
            handler: async function(val, oldVal) {
                if(val === oldVal) {
                    return;
                }
                this.invitations = [];
                switch(val) {
                    case "forMe": await this.fetchRequestsForMe()
                        break;
                    case "fromMe": await this.fetchRequestsFromMe();
                        break;
                }
            },
            immediate: true
        }
    },
    mounted: async function() {

    },
    template: `
    <section class="isolaatti-card">
      <h5>Solicitudes</h5>
      <div class="btn-group btn-group-sm w-100">
        <button class="btn" :class="{'btn-primary':overallMode==='forMe'}" @click="overallMode='forMe'">
          Para tí
        </button>
        <button class="btn" :class="{'btn-primary':overallMode==='fromMe'}" @click="overallMode='fromMe'">
          De tí
        </button>
      </div>
      <squad-requests-list v-if="!loading" :items="requests" @request-update="onRequestUpdate" @deleted="onDeleted"></squad-requests-list>
      <div v-if="loading" class="d-flex justify-content-center mt-2">
        <div class="spinner-border" role="status">
          <span class="sr-only">Cargando más contenido...</span>
        </div>
      </div>
      <div v-if="!loading && requests.length === 0">
        <p class="m-2 text-center"> No hay contenido que mostrar <i class="fa-solid fa-face-sad-cry"></i></p>
      </div>
      
    </section>
    `
}
Vue.component('squads-requests', requestsComponent);