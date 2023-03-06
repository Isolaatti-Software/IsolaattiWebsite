Vue.component('squad-invitations-list', {
    props: {
        items: {
            required: true,
            type: Array
        }
    },
    data: function() {
        return {
            userData: userData,
            editionMode: false,
            selectedInvitationId: undefined,
            selectedInvitationResponseMessage: null,
            customHeaders: customHttpHeaders,
            performingRequest: false
        }
    },
    methods: {
        selectInvitation: function(id) {
            if(this.selectedInvitationId === id) {
                this.selectedInvitationId = undefined;
            } else {
                this.selectedInvitationId = id;
            }

        },
        setEditionMode: function() {
            this.editionMode = !this.editionMode;
        },
        profileLink: function(profileId) {
            return `/perfil/${profileId}`;
        },
        profileImageLink: function(profileId) {
            return `/api/images/profile_image/of_user/${profileId}?mode=small`
        },
        squadLink: function(squadId) {
            return `/squads/${squadId}`;
        },
        acceptInvitation: async function(invitation) {
            this.performingRequest = true;
            const response = await fetch(`/api/Squads/Invitations/${invitation.invitation.id}/Accept`, {
                method: "POST",
                headers: this.customHeaders,
                body: JSON.stringify({
                    message: invitation.invitation.responseMessage
                })
            });
            this.performingRequest = false;
            
            if(response.ok) {
                const parsedResult = await response.json();
                this.$emit("invitation-update",parsedResult.invitation);
            }
        },
        rejectInvitation: async function(invitation) {
            this.performingRequest = true;
            const response = await fetch(`/api/Squads/Invitations/${invitation.invitation.id}/Decline`, {
                method: "post",
                headers: this.customHeaders,
                body: JSON.stringify({
                    data: invitation.invitation.responseMessage
                })
            })
            this.performingRequest = false;
            
            if(response.ok) {
                this.$emit("invitation-update", await response.json());
            }
        }
    },
    template: `
      <div class="list-group list-group-flush">
      <button v-for="invitation in items"
              class="list-group-item list-group-item-action"
              :key="invitation.invitation.id"
      >
        <div class="d-flex flex-column">
          <div class="d-flex align-content-center">
            <div class="mb-2">
              <div class="m-0">
                <img class="user-avatar" :src="profileImageLink(invitation.invitation.recipientUserId)" alt="Foto">
                <strong>
                  <a :href="profileLink(invitation.invitation.senderUserId)">
                    {{ invitation.senderName }}</a>
                </strong>
                invitó a <strong><a :href="profileLink(invitation.invitation.recipientUserId)">{{ invitation.recipientName }}</a></strong> a unirse
                a <strong>
                <a :href="squadLink(invitation.invitation.squadId)">
                  {{ invitation.squadName === null ? 'Squad eliminado' : invitation.squadName }}
                </a>
              </strong> el
                <strong>{{ new Date(invitation.invitation.creationDate).toLocaleDateString("es") }}</strong>
              </div>
              <div class="small">
                <template v-if="invitation.invitation.invitationStatus === 0">
                  Invitación enviada
                </template>
                <template v-else-if="invitation.invitation.invitationStatus === 1">
                  Invitación aceptada
                </template>
                <template v-else-if="invitation.invitation.invitationStatus === 2">
                  Invitación rechazada
                </template>
              </div>
            </div>
            <button class="btn ml-auto"
                    v-if="selectedInvitationId === invitation.invitation.id"
                    @click="selectInvitation(invitation.invitation.id)">
              <i class="fa-solid fa-arrow-up"></i>
            </button>
            <button class="btn ml-auto"
                    v-else
                    @click="selectInvitation(invitation.invitation.id)">
              <i class="fa-solid fa-arrow-down"></i>
            </button>
          </div>
          <template v-if="selectedInvitationId === invitation.invitation.id">
            <template v-if="!editionMode">
              <template v-if="invitation.invitation.message !== null">
                <p><strong>Mensaje de invitación:</strong> "{{ invitation.invitation.message }}"</p>
              </template>
              <template v-if="invitation.invitation.responseMessage!==null">
                <p><strong>Mensaje de respuesta:</strong> "{{ invitation.invitation.responseMessage }}"</p>
              </template>
              <template v-if="invitation.invitation.invitationStatus === 0">
                <textarea class="form-control" 
                          v-if="invitation.invitation.recipientUserId === userData.id" 
                          v-model="invitation.invitation.responseMessage"></textarea>
                <div class="d-flex w-100 justify-content-end">
                  <button type="button" class="btn btn-light mt-1"
                          v-if="invitation.invitation.recipientUserId === userData.id"
                          @click="rejectInvitation(invitation)">
                    Rechazar
                  </button>
                  <button type="button" class="btn btn-primary mt-1"
                          v-if="invitation.invitation.recipientUserId === userData.id"
                          @click="acceptInvitation(invitation)">
                    Aceptar
                  </button>
                  <button type="button" class="btn btn-light btn-sm" v-if="invitation.invitation.senderUserId === userData.id">
                    <i class="fa-solid fa-trash"></i>
                  </button>
                </div>
              </template>
              
            </template>

          </template>
        </div>


      </button>
      </div>
    `
})