Vue.component("squad-user-profile", {
    props: {
        initialState: {
            type: Object,
            required: true
        },
        squadId: {
            required: true,
            type: String
        },
        selectedUser: {
            required: true,
            type: Object
        }
    },
    data: function() {
        return {
            customHeaders: customHttpHeaders,
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
            },
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
    },
    methods: {
        userProfileLink: function(userId) {
            return `/perfil/${userId}`
        },
        profileImageLink: function(imageId) {
            if(imageId === null || imageId === "") {
                return "/res/imgs/avatar.svg";
            }
            return `/api/images/image/${imageId}?mode=reduced`
        },
        fetchProfile: async function() {
            const response = await fetch(`/api/Squads/${this.squadId}/User/${this.selectedUser.id}`,{
                method: "get",
                headers: this.customHeaders
            });
            if(!response.ok) {
                this.errorGettingPermissions = true;
                return;
            }
            const profile = await response.json();
            if(profile.permissions !== null) {
                this.getCurrentPermissions(profile.permissions);
            }
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
        toUserPermissionsDto: function() {
            const permissionsArray = [];
            for(let [key, value] of Object.entries(this.selectedUserData.permissions)) {
                if(value.value) {
                    permissionsArray.push(value.id)
                }
            }
            return permissionsArray;
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

        'selectedUserData.permissions': {
            handler: async function() {
                if(this.selectedUserDataInitiated){
                    await this.updateSelectedUserPermissions();
                }
            },
            deep: true,
            immediate: false
        }
    },
    mounted: async function() {
        this.selectedUserDataInitiated = false;
        await this.fetchProfile();
        this.selectedUserDataInitiated = true;
    },
    template: `
    <section>
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

    <div class="d-flex justify-content-center" v-if="initialState.isOwner && selectedUserData.profile !== undefined">
      <button class="btn btn-outline-primary btn-sm" v-if="!selectedUserData.profile.isAdmin"
              data-toggle="modal"
              data-target="#modal-turn-into-admin-confirmation">
        Convertir en administrador
      </button>
      <button class="btn btn-outline-primary btn-sm" v-else
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
    
    <template v-if="selectedUserData.profile !== undefined && selectedUserData.profile.isAdmin">
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
        <div class="custom-control custom-switch">
          <input type="checkbox" class="custom-control-input" id="customSwitch5"
                 v-model="selectedUserData.permissions.modifyImage.value"
                 :disabled="!initialState.isOwner">
          <label class="custom-control-label" for="customSwitch4">Eliminar miembros</label>
        </div>
        <div class="custom-control custom-switch">
          <input type="checkbox" class="custom-control-input" id="customSwitch6"
                 v-model="selectedUserData.permissions.modifyImage.value"
                 :disabled="!initialState.isOwner">
          <label class="custom-control-label" for="customSwitch4">Agregar administradores</label>
        </div>
        <div class="custom-control custom-switch">
          <input type="checkbox" class="custom-control-input" id="customSwitch7"
                 v-model="selectedUserData.permissions.modifyImage.value"
                 :disabled="!initialState.isOwner">
          <label class="custom-control-label" for="customSwitch4">Moderar contenido</label>
        </div>
        <div class="custom-control custom-switch">
          <input type="checkbox" class="custom-control-input" id="customSwitch8"
                 v-model="selectedUserData.permissions.modifyImage.value"
                 :disabled="!initialState.isOwner">
          <label class="custom-control-label" for="customSwitch4">Editar permisos de administradores</label>
        </div>
        <div class="alert-danger alert mt-1 alert-dismissible fade show" role="alert" v-if="selectedUserData.errorSettingPermissions">
          Error al guardar ajustes
          <button type="button" class="close" data-dismiss="alert" aria-label="Close">
            <span aria-hidden="true">&times;</span>
          </button>
        </div>
      </div>
    </template>
    
    <!-- Turn into admin modal -->
    <div class="modal" id="modal-turn-into-admin-confirmation">
      <div class="modal-dialog modal-dialog-centered">
        <turn-into-admin-squad-modal-content :squad-id="squadId" :selected-user="selectedUser"/>
      </div>
    </div>
    
    <!-- Remove administrator modal-->
    <div class="modal" id="modal-remove-admin-confirmation">
      <div class="modal-dialog modal-dialog-centered">
        <remove-admin-from-squad-modal-content v-if="selectedUser !== undefined"
                                               :selected-user="selectedUser"
                                               :squad-id="squadId"/>
      </div>
    </div>
    <!-- Remove user from squad modal-->
    <div class="modal" id="modal-remove-user-from-squad-confirmation">
      <div class="modal-dialog modal-dialog-centered">
        <remove-squad-user-modal-content v-if="selectedUser !== undefined"
                                         :selected-user="selectedUser"
                                         :squad-id="squadId"/>
      </div>
    </div>
    </section>
    `
})