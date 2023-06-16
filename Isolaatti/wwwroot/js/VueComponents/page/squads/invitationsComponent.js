const invitationsComponent = {
    data: function() {
        return {
            customHeaders: customHttpHeaders,
            invitations: [],
            loading: true,
            error: false,
            editionMode: false,
            selectedInvitationId: undefined,
            overallMode: "forMe" //or fromMe
        }
    },
    methods: {
        fetchInvitationsForMe: async function() {
            this.loading = true;
            const response = await fetch(`/api/Users/Me/Squads/InvitationsForMe`, {
                headers: this.customHeaders
            });
            try {
                this.invitations = (await response.json());
            } catch (e) {
                this.error = true;
            }
            this.loading = false;
        },
        fetchInvitationsFromMe: async function() {
            const response = await fetch(`/api/Users/Me/Squads/MyInvitations`, {
                headers: this.customHeaders
            });
            try {
                this.invitations = (await response.json());
            } catch(e) {
                this.error = true;
            }
            this.loading = false;
        },
        onInvitationUpdate: function(invitation) {
            const index = this.invitations.findIndex(inv => inv.invitation.id === invitation.id);
            const temp = this.invitations;
            temp[index].invitation = invitation;
            this.invitations = temp;
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
                    case "forMe": await this.fetchInvitationsForMe();
                        break;
                    case "fromMe": await this.fetchInvitationsFromMe();
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
      
      <h5>Invitaciones</h5>
      <div class="btn-group btn-group-sm w-100">
        <button class="btn" :class="{'btn-primary':overallMode==='forMe'}" @click="overallMode='forMe'">
          Para tí
        </button>
        <button class="btn" :class="{'btn-primary':overallMode==='fromMe'}" @click="overallMode='fromMe'">
          De tí
        </button>
      </div>
      <squad-invitations-list :items="invitations" @invitation-update="onInvitationUpdate"></squad-invitations-list>
      <div v-if="loading" class="d-flex justify-content-center mt-2">
        <div class="spinner-border" role="status">
          <span class="sr-only">Cargando más contenido...</span>
        </div>
      </div>
    <div v-if="!loading && invitations.length === 0">
      <p class="m-2 text-center"> No hay contenido que mostrar <i class="fa-solid fa-face-sad-cry"></i></p>
    </div>
      
    </section>
    `
}

Vue.component('squads-invitations', invitationsComponent);