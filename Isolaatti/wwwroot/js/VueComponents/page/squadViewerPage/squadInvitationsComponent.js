﻿/**
 * Squad invitations
 * This component shows the invitations made from this squad.
 */

Vue.component('squad-invitations',{
    props: {
        squadId: {
            type: String,
            required: true
        }
    },
    data: function() {
        return {
            customHeaders: customHttpHeaders,
            invitations: [],
            loading: true,
            error: false,
            selectedInvitationId: undefined,
            editionMode: false,
            overallMode: "list" // or new
        }
    },
    methods: {
        fetchInvitations: async function() {
            const response = await fetch(`/api/Squads/${this.squadId}/Invitations`, {
                headers: this.customHeaders
            });
            try {
                this.invitations = (await response.json()).invitations;
                this.loading = false;
            } catch (e) {
                this.error = true;
            }
        },
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
        toggleOverallMode: function() {
            if(this.overallMode === "list") {
                this.overallMode = "new";
            } else {
                this.overallMode = "list"
            }
        },
        profileImageLink: function(profileId) {
            return `/api/images/profile_image/of_user/${profileId}?mode=small`
        },
        onInvitationsCreated: async function(){
            console.log("Creado");
            this.overallMode = "list";
            await this.fetchInvitations();
        }
    },
    watch: {
        $route: {
            deep: true,
            immediate: true,
            handler: function(to, from) {
                const action = to.query.action;
                if(action !== undefined)
                    this.overallMode = action;
            }
        }
    },
    mounted: async function() {
        await this.fetchInvitations();
    },
    template: `
    <section class="isolaatti-card">
      <div class="d-flex justify-content-end mt-1">
        <button type="button" class="btn btn-light btn-sm" @click="toggleOverallMode">
          <i v-if="overallMode==='list'" class="fa-solid fa-plus"></i>
          <i v-if="overallMode==='new'" class="fa-solid fa-xmark"></i>
        </button>
      </div>
      <template v-if="overallMode==='list'">
        <template v-if="invitations.length === 0 && !loading">
          <p class="m-2 text-center"> No hay contenido que mostrar <i
              class="fa-solid fa-face-sad-cry"></i></p>
          <div class="d-flex justify-content-center">
            <button class="btn btn-primary" @click="toggleOverallMode">
              Invita a alguien
            </button>
          </div>
        </template>
        <div v-if="loading" class="d-flex justify-content-center mt-2">
          <div class="spinner-border" role="status">
            <span class="sr-only">Cargando más contenido...</span>
          </div>
        </div>
        <div class="list-group list-group-flush">
          <button v-for="invitation in invitations"
                  class="list-group-item list-group-item-action"
                  :key="invitation.invitation.id"
          >
            <div class="d-flex flex-column">
              <div class="d-flex align-content-center">
                <div class="mb-2">
                  <div class="m-0">
                    <a :href="profileLink(invitation.invitation.senderUserId)">
                      <img class="user-avatar" :src="profileImageLink(invitation.invitation.recipientUserId)">
                      {{ invitation.senderName }}</a>
                    invitó a <a :href="profileLink(invitation.invitation.recipientUserId)">{{ invitation.recipientName }}</a> a unirse el
                    <strong>{{ new Date(invitation.invitation.creationDate).toLocaleDateString("es")}}</strong>
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
                  <div class="d-flex justify-content-end">
                    <button type="button" class="btn btn-light btn-sm"
                            v-if="invitation.invitation.invitationStatus === 0"
                            @click="setEditionMode">
                      <i class="fa-solid fa-pencil"></i>
                    </button>
                    <button type="button" class="btn btn-light btn-sm">
                      <i class="fa-solid fa-trash"></i>
                    </button>
                  </div>
                </template>
                <template v-else>
                  <textarea class="form-control" v-model="invitation.invitation.message"></textarea>
                  <div class="d-flex justify-content-end mt-1">
                    <button type="button" class="btn btn-sm mr-1" @click="setEditionMode">
                      Cancelar
                    </button>
                    <button type="button" class="btn btn-sm btn-primary">
                      Guardar
                    </button>
                  </div>
                </template>

              </template>
            </div>


          </button>
        </div>
      </template>
      <template v-else-if="overallMode==='new'">
        <squad-new-invitation :squad-id="squadId" @created="onInvitationsCreated"></squad-new-invitation>
      </template>
    </section>
    `
});