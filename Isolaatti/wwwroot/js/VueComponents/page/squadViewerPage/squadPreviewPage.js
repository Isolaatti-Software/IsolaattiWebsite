Vue.component('squad-preview', {
    props: {
        squadId: {
            required: true,
            type: String
        }
    },
    data: function() {
        return {
            customHeaders: customHttpHeaders,
            invitation: undefined,
            joinRequest: {
                message: ""
            },
            joinRequestSent: false,
            error: false,
            submitting: false
        }
    },
    methods: {
        searchUserInvitation: async function() {
            const response = await fetch(`/api/Invitations/Search?squadId=${this.squadId}`,{
                method: "GET",
                headers: this.customHeaders
            });
            this.invitation = await response.json();
        },
        searchUserJoinRequest: async function() {
            const response = await fetch(`/api/Squads/JoinRequests/Search?squadId=${this.squadId}`, {
                method: "GET",
                headers: this.customHeaders
            });
            if(response.ok) {
                this.joinRequestSent = (await response.json()).request !== null;
            }
            
        },
        acceptInvitation: async function() {
            this.submitting = true;
            const response = await fetch(`/api/Squads/Invitations/${this.invitation.invitation.id}/Accept`, {
                method: "POST",
                headers: this.customHeaders,
                body: JSON.stringify({
                    message: this.invitation.invitation.responseMessage
                })
            });
            
            this.submitting = false;
            window.location.reload();
            
        },
        makeJoinRequest: async function() {
            this.submitting = true;
            const response = await fetch(`/api/Squads/${this.squadId}/RequestJoin`, {
                method: "POST",
                headers: this.customHeaders,
                body: JSON.stringify({
                    message: this.joinRequest.message
                })
            });
            
            if(response.ok)
                this.joinRequestSent = true;
            else
                this.error = true;

            this.submitting = false;
        }
    },
    computed: {
        invited: function() {
            if(this.invitation === undefined) {
                return false;
            }
            return this.invitation.invitation !== null;
        }
    },
    mounted: async function() {
        await this.searchUserInvitation();
        await this.searchUserJoinRequest();
    },
    template: `
    <section class="container">
    <squad-header :squad-id="squadId" class="isolaatti-card mb-2" :preview="true"></squad-header>
    <div class="isolaatti-card mt-4">
      <div class="d-flex flex-column">
        <div class="display-3 text-center w-100"><i class="fa-solid fa-face-kiss-wink-heart"></i></div>
        <h2 class="text-center">No perteneces a este squad</h2>
        <template v-if="invited">
          <p class="text-center">
            Fuiste invitado por <strong>{{ invitation.senderName }}</strong>
            el <strong>{{new Date(invitation.invitation.creationDate).toLocaleDateString("es")}}</strong>
          </p>

          <div class="form-group w-100">
            <label for="response-message">Respuesta</label>
            <textarea id="response-message" v-model="invitation.invitation.responseMessage" class="form-control" placeholder="Gracias por invitarme, estoy dentro..."></textarea>
          </div>
          <button class="btn btn-primary mt-1"  @click="acceptInvitation">Aceptar</button>
        </template>
        
        <template v-else>
          <template v-if="!joinRequestSent">
            <textarea class="form-control mb-2"
                  v-model="joinRequest.message"
                  placeholder="Dile al admin porqué debería dejarte entrar..."></textarea>
            <div class="d-flex flex-column align-items-center justify-content-center w-100">
              <button class="btn btn-primary"
                      v-if="!invited"
                      @click="makeJoinRequest"
                      :disabled="joinRequest.message === ''">
                Enviar solicitud
              </button>
            </div>
          </template>
          <template v-else>
            <div class="d-flex align-items-center flex-column">
              <h5>Has enviado una solicitud para unirte.</h5>
              <div class="d-flex">
                <a class="btn btn-primary" href="/squads#/solicitudes_union">Consultar invitaciones de unión hechas por tí</a>
              </div>
            </div>
          </template>
        </template>
      </div>
    </div>
    </section>
    `
});