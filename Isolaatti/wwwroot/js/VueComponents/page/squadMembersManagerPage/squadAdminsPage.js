Vue.component("squad-admins-page", {
    props: {
        squadId: {
            type: String,
            required: true
        },
        users: {
            type: Array,
            required: true
        },
        initialState: {
            type: Object,
            required: true
        }
    },
    data: function() {
        return {
            customHeaders: customHttpHeaders,
            selectedUser: undefined,
            selectedUserDataInitiated: false,
            selectedUserData: {
                // id is used to send it to the backend
                errorSettingPermissions: false,
                errorGettingPermissions: false,
                profile: undefined,
                permissions: {
                    modifySquadName: {
                        value: false,
                        id: "modify_name"
                    },
                    modifyDescription: {
                        value: false,
                        id: "modify_description"
                    },
                    modifyAudio: {
                        value: false,
                        id: "modify_audio"
                    },
                    modifyImage: {
                        value: false,
                        id: "modify_image"
                    }
                }
            }
        }
    },
    methods: {
        loadDetails: function(user) {
            this.selectedUser = user;
        },
        profileImageLink: function(imageId) {
            if(imageId === null || imageId === "") {
                return "/res/imgs/avatar.svg";
            }
            return `/api/images/image/${imageId}?mode=reduced`
        },
        userProfileLink: function(userId) {
            return `/perfil/${userId}`
        },
        toUserPermissionsDto: function() {
            const permissionsArray = [];
            for(let [key, value] of Object.entries(this.selectedUserData.permissions)) {
                if(value.value) {
                    permissionsArray.push(value.id)
                }
            }
            return permissionsArray;
        },
        getCurrentSquadUserProfile: async function() {
            const response = await fetch(`/api/Squads/${this.squadId}/User/${this.selectedUser.id}`,{
                method: "get",
                headers: this.customHeaders
            });
            if(!response.ok) {
                this.selectedUserData.errorGettingPermissions = true;
                return;
            }
            const profile = await response.json();
            this.getCurrentPermissions(profile.permissions);
            this.selectedUserData.profile = profile;
        },
        getCurrentPermissions: function(permissionsArray) {
            const modifyDescription = this.selectedUserData.permissions.modifyDescription;
            modifyDescription.value = permissionsArray.findIndex(item => item === modifyDescription.id) >= 0;

            const modifySquadName = this.selectedUserData.permissions.modifySquadName;
            modifySquadName.value = permissionsArray.findIndex(item => item === modifySquadName.id) >= 0;

            const modifyImage = this.selectedUserData.permissions.modifyImage;
            modifyImage.value = permissionsArray.findIndex(item => item === modifyImage.id) >= 0;

            const modifyAudio = this.selectedUserData.permissions.modifyAudio
            modifyAudio.value = permissionsArray.findIndex(item => item === modifyAudio.id) >= 0;
            
        },
        updateSelectedUserPermissions: async function() {
            // compose and send request here
            const requestBody = {
                permissionsList: this.toUserPermissionsDto()
            }
            
            
            const response = await fetch(`/api/Squads/${this.squadId}/User/${this.selectedUser.id}/SetPermissions`,{
                method: "post",
                headers: this.customHeaders,
                body: JSON.stringify(requestBody)
            });
            
            if(!response.ok) {
                this.selectedUserData.errorSettingPermissions = true;
            }
            
        }
    },
    watch: {
        selectedUser: async function() {
            this.selectedUserDataInitiated = false;
            await this.getCurrentSquadUserProfile();
            this.selectedUserDataInitiated = true;
            
        },
        'selectedUserData.permissions': {
            handler: async function() {
                if(this.selectedUserDataInitiated) {
                    await this.updateSelectedUserPermissions();
                }
            },
            deep: true,
            immediate: false
        }
    },
    template: `
      <div>
      <h5>Administradores actuales</h5>
      <div class="row">
        <div class="col-lg-6">
          <users-grid :users="users" v-on:itemClick="loadDetails"/>
        </div>
        <div class="col-lg-6">
          <p v-if="selectedUser === undefined">Selecciona un miembro para ver detalles.</p>
          <div v-else>
            
            <div class="d-flex w-100 justify-content-center">
              <img :src="profileImageLink(selectedUser.imageId)" class="profile-pic"/>
            </div>
            <h4 class="text-center">{{selectedUser.name}}</h4>
            <div class="d-flex justify-content-center w-100">
              <a :href="userProfileLink(selectedUser.id)" target="_blank" class="btn btn-outline-primary btn-sm">Ir al perfil</a> 
            </div>
            <h5>Datos generales </h5>
            <template v-if="selectedUserData.profile !== undefined">
              <table class="table">
                <tr>
                  <td>Fecha de union</td>
                  <td>{{ new Date(selectedUserData.profile.joined).toLocaleDateString()}}</td>
                </tr>
                <tr>
                  <td>Fecha de ultima interaccion</td>
                  <td>{{ selectedUserData.profile.lastInteraction !== null ? new Date(selectedUserData.profile.lastInteraction).toLocaleDateString() : 'No hay datos'}}</td>
                </tr>
                <tr>
                  <td>Ranking</td>
                  <td>{{selectedUserData.profile.ranking}}</td>
                </tr>
              </table>
            </template>
            
            <div class="d-flex justify-content-center">
              <button class="btn btn-outline-primary btn-sm" 
                      data-toggle="modal" 
                      data-target="#modal-remove-admin-confirmation">
                Quitar administrador
              </button>
              <button class="btn btn-outline-danger btn-sm ml-1"
                      data-toggle="modal" 
                      data-target="#modal-remove-user-from-squad-confirmation">
                Sacar del squad
              </button>
            </div>
            <h5 class="mt-1">Permisos</h5>
            <div class="alert-danger alert mt-1 alert-dismissible fade show" role="alert" v-if="selectedUserData.errorGettingPermissions">
              Error al obtener información de permisos
              <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                <span aria-hidden="true">&times;</span>
              </button>
            </div>
            <p>Este administrador puede llevar a cabo lo siguiente:</p>
            <div class="d-flex flex-column">
              <div class="custom-control custom-switch">
                <input type="checkbox" class="custom-control-input" id="customSwitch1" 
                       v-model="selectedUserData.permissions.modifySquadName.value" 
                       :disabled="!initialState.isOwner">
                <label class="custom-control-label" for="customSwitch1">Modificar nombre del squad</label>
              </div>
              <div class="custom-control custom-switch">
                <input type="checkbox" class="custom-control-input" id="customSwitch2" v-model="selectedUserData.permissions.modifyDescription.value" :disabled="!initialState.isOwner">
                <label class="custom-control-label" for="customSwitch2">Modificar descripción y descripción extendida del squad</label>
              </div>
              <div class="custom-control custom-switch">
                <input type="checkbox" class="custom-control-input" id="customSwitch3" 
                       v-model="selectedUserData.permissions.modifyAudio.value" 
                       :disabled="!initialState.isOwner">
                <label class="custom-control-label" for="customSwitch3">Modificar audio del squad</label>
              </div>
              <div class="custom-control custom-switch">
                <input type="checkbox" class="custom-control-input" id="customSwitch4" 
                       v-model="selectedUserData.permissions.modifyImage.value" 
                       :disabled="!initialState.isOwner">
                <label class="custom-control-label" for="customSwitch4">Modificar imagen del squad</label>
              </div>
              <div class="alert-danger alert mt-1 alert-dismissible fade show" role="alert" v-if="selectedUserData.errorSettingPermissions">
                Error al guardar ajustes
                <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                  <span aria-hidden="true">&times;</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
      <!-- Remove administrator modal-->
      <div class="modal" id="modal-remove-admin-confirmation">
        <div class="modal-dialog modal-dialog-centered">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">Quitar administrador</h5>
              <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                <span aria-hidden="true">&times;</span>
              </button>
            </div>
            <div class="modal-body" v-if="selectedUser !== undefined">
              <p>Al hacer esto, {{selectedUser.name}} dejará de ser administrador para ser miembro normal</p>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" data-dismiss="modal">No, cancelar</button>
              <button type="button" class="btn btn-primary">Sí, proceder</button>
            </div>
          </div>
        </div>
      </div>
      <!-- Remove user from squad modal-->
      <div class="modal" id="modal-remove-user-from-squad-confirmation">
        <div class="modal-dialog modal-dialog-centered">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">Sacar del squad</h5>
              <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                <span aria-hidden="true">&times;</span>
              </button>
            </div>
            <div class="modal-body" v-if="selectedUser !== undefined">
              <p>¿Confirmas que deseas sacar del squad a {{selectedUser.name}}?</p>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" data-dismiss="modal">No, cancelar</button>
              <button type="button" class="btn btn-primary">Sí, proceder</button>
            </div>
          </div>
        </div>
      </div>
      </div>
    `
})