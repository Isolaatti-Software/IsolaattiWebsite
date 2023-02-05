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
            customHeaders: customHttpHeaders,
            showingDeleteActionRequestId: undefined,
            requestInProgress: false,
            deleting: false
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
            return `/api/images/profile_image/of_user/${profileId}?mode=small`
        },
        squadLink: function(squadId) {
            return `/squads/${squadId}`;
        },
        acceptRequest: async function(request) {
            this.requestInProgress = true;
            const form = new FormData();
            form.append("message", request.responseMessage);
            const response = await fetch(`/api/Squads/JoinRequests/${request.request.id}/Accept`, {
                method: "POST",
                headers: this.customHeaders,
                body: form
            });

            this.requestInProgress = false;

            if(response.ok) {
                const parsedResult = await response.json();
                this.$emit("request-update",parsedResult.request);
            }

        },
        rejectRequest: async function(request) {
            
        },
        showDeleteActions: function(request){
            this.showingDeleteActionRequestId = request.request.id;
        },
        hideDeleteActions: function() {
            this.showingDeleteActionRequestId = undefined;
        },
        deleteRequest: async function(request) {
            this.requestInProgress = true;
            this.deleting = true;
            const response = await fetch(`/api/Squads/JoinRequests/${request.request.id}/Remove`, {
                method: "delete",
                headers: this.customHeaders
            });
            this.requestInProgress = false;
            this.deleting = false;
            
            if(response.ok) {
                this.$emit('deleted', request.request.id)
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
                    {{ request.senderName }}</a>
                </strong>
                solicitó unirse a <strong>{{ request.squadName }}</strong>
               el
                <strong>{{ new Date(request.request.creationDate).toLocaleDateString("es") }}</strong>
              </div>
              <div class="small">
                <template v-if="request.request.joinRequestStatus === 0">
                  Solicitud recibida
                </template>
                <template v-else-if="request.request.joinRequestStatus === 1">
                  Solicitud aceptada
                </template>
                <template v-else-if="request.request.joinRequestStatus === 2">
                  Solicitud rechazada
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
                    v-if="request.admins"
                    v-model="request.responseMessage" placeholder="Escribe un mensaje de aceptación"></textarea>
                <div class="d-flex justify-content-end mt-1">
                  <button type="button" class="btn btn-primary"
                    v-if="request.admins"
                    @click="acceptRequest(request)">
                    Aceptar
                  </button>
                  <button type="button" 
                    class="btn btn-light btn-sm" 
                    v-if="request.request.senderUserId === userData.id && showingDeleteActionRequestId !== request.request.id"
                    @click="showDeleteActions(request)">
                    <i class="fa-solid fa-trash"></i>
                  </button>
                </div>
                <div v-if="showingDeleteActionRequestId === request.request.id" class="d-flex justify-content-end align-items-center">
         
                    <span class="mr-2">¿Eliminar?</span>
                    <button class="btn btn-danger btn-sm mr-1" :disabled="requestInProgress" @click="deleteRequest(request)">
                        <div class="spinner-border spinner-border-sm" v-if="deleting" role="status">
                            <span class="sr-only">Deleting...</span>
                        </div>
                        <span v-if="deleting"> Eliminando</span>
                        <span v-else>Sí, eliminar ahora</span>
                    </button>
                    <button class="btn btn-light btn-sm" @click="hideDeleteActions" :disabled="requestInProgress">No</button>
                    
                </div>
              </template>
              
            </template>

          </template>
        </div>


      </button>
      </div>
    `
})