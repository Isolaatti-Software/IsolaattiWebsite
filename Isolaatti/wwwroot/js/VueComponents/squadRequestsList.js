Vue.component('squad-requests-list', {
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
            selectedRequestId: undefined,
            selectedInvitationResponseMessage: null,
            customHeaders: customHttpHeaders
        }
    },
    methods: {
        selectRequest: function(id) {
            if(this.selectedRequestId === id) {
                this.selectedRequestId = undefined;
            } else {
                this.selectedRequestId = id;
            }

        },
        setEditionMode: function() {
            this.editionMode = !this.editionMode;
        },
        profileLink: function(profileId) {
            return `/perfil/${profileId}`;
        },
        profileImageLink: function(profileId) {
            return `/api/images/profile_images/of_user/${profileId}?mode=small`
        },
        squadLink: function(squadId) {
            return `/squads/${squadId}`;
        },
        acceptRequest: async function(request) {
            const response = await fetch(`/api/Squads/Invitations/${invitation.id}/Accept`, {
                method: "POST",
                headers: this.customHeaders,
                body: JSON.stringify({
                    message: invitation.responseMessage
                })
            });

            if(response.ok) {
                const parsedResult = await response.json();
                this.$emit("invitation-update",parsedResult.invitation);
            }

        }
    },
    template: `
      <div class="list-group list-group-flush">
      <button v-for="request in items"
              class="list-group-item list-group-item-action"
              :key="request.request.id"
      >
        <div class="d-flex flex-column">
          <div class="d-flex align-content-center">
            <div class="mb-2">
              <div class="m-0">
                <img class="user-avatar" :src="profileImageLink(request.request.senderUserId)" alt="Foto">
                <strong>
                  <a :href="profileLink(request.request.senderUserId)">
                    {{ request.username }}</a>
                </strong>
                solicitó unirse a <strong>{{ request.squadName }}</strong>
               el
                <strong>{{ new Date(request.request.creationDate).toLocaleDateString("es") }}</strong>
              </div>
              <div class="small">
                <template v-if="request.request.joinRequestStatus === 0">
                  Solicitud enviada
                </template>
                <template v-else-if="request.request.joinRequestStatus === 1">
                  Invitación aceptada
                </template>
                <template v-else-if="request.request.joinRequestStatus === 2">
                  Invitación rechazada
                </template>
              </div>
            </div>
            <button class="btn ml-auto"
                    v-if="selectedRequestId === request.request.id"
                    @click="selectRequest(request.request.id)">
              <i class="fa-solid fa-arrow-up"></i>
            </button>
            <button class="btn ml-auto"
                    v-else
                    @click="selectRequest(request.request.id)">
              <i class="fa-solid fa-arrow-down"></i>
            </button>
          </div>
          <template v-if="selectedRequestId === request.request.id">
            <template v-if="!editionMode">
              <template v-if="request.request.message !== null">
                <p><strong>Mensaje de invitación:</strong> "{{ request.request.message }}"</p>
              </template>
              <template v-if="request.request.responseMessage!==null">
                <p><strong>Mensaje de respuesta:</strong> "{{ request.request.responseMessage }}"</p>
              </template>
              <template v-if="request.request.joinRequestStatus === 0">
                <textarea class="form-control" 
                          v-if="request.request.recipientUserId === userData.id" 
                          v-model="request.request.responseMessage"></textarea>
                <div class="d-flex justify-content-end">
                  <button type="button" class="btn btn-primary mr-auto"
                          v-if="invitation.invitation.recipientUserId === userData.id"
                          @click="acceptRequest(invitation.invitation)">
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