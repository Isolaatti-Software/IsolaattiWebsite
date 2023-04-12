const squadHeaderComponent = {
    props: {
        squadId: {
            required: true,
            type: String
        },
        preview: {
          required: false,
          type: Boolean,
          default: false
        }
    },
    data: function() {
        return {
            customHeaders: customHttpHeaders,
            userData: userData,
            squadInfo: undefined,
            squadHeaderEditMode: false, // this should only be toggled by squad admin
            squadExtendedDescriptionCutContent: true,
            errorUpdating: false,
            submitting: false,
            squadInfoForEdit: {
                name: "",
                description: "",
                extendedDescription: ""
            },
            squadImageEdition: false,
            leaveSquad:{
                requestInProgress: false,
                ownerCannotLeaveError: false,
                squadNotFound: false,
                userDoesNotBelong: false,
                unknownError: true
            }
        }
    },
    computed:  {
        userIsAdmin: function() {
            if(this.squadInfo === undefined) return false;
            return this.squadInfo.userId === this.userData.id;
        },
        updateValidated: function() {
            return this.squadInfoForEdit.name.length > 0
                && this.squadInfoForEdit.extendedDescription.length > 0
                && this.squadInfoForEdit.description.length > 0;
        },
        imageUrl: function() {
            return this.squadInfo.imageId === null ? "/res/imgs/groups.png" : `/api/images/image/${this.squadInfo.imageId}?mode=reduced`;
        }
    },
    methods: {
        goToEditMode: function() {
            this.squadHeaderEditMode = !this.squadHeaderEditMode;
            this.squadInfoForEdit.name = this.squadInfo.name;
            this.squadInfoForEdit.description = this.squadInfo.description;
            this.squadInfoForEdit.extendedDescription = this.squadInfo.extendedDescription;
        },
        fetchSquad: async function() {
            const response = await fetch(`/api/Squads/${this.squadId}`, {
                headers: this.customHeaders
            });

            this.squadInfo = await response.json();
            this.$emit("description", this.squadInfo.extendedDescription);
        },
        updateSquad: async function() {
            this.submitting = true;
            const response = await fetch(`/api/Squads/${this.squadId}/Update`, {
                method: "post",
                headers: this.customHeaders,
                body: JSON.stringify(this.squadInfoForEdit)
            });
            if(!response.ok) {
                this.errorUpdating = true;
                return;
            }
            const parsedResponse = await response.json();
            this.submitting = false;
            if(parsedResponse.result === "Success") {
                await this.fetchSquad();
                this.squadHeaderEditMode = false;
            }
        },
        onImageUpdated: function(imageId) {
            this.squadInfo.imageId = imageId;
            $('#modal-edit-photo').modal('hide');
        },
        leave: async function() {
            this.resetLeaveModal();
            this.leaveSquad.requestInProgress = true;
            const response = await fetch(`/api/Squads/${this.squadId}/Leave`, {
                method: "post",
                headers: this.customHeaders
            });
            this.leaveSquad.requestInProgress = false;
            
            if(response.ok){
                const result = await response.json();
                switch(result.result) {
                    case "left_squad": 
                        window.location = "/squads";
                        break;
                    
                }
            } else {
                if(response.status >= 500) {
                    this.leaveSquad.unknownError = true;
                    return;
                }
                
                const result = await response.json();
                switch(result.result) {
                    case "user_does_not_belong":
                        this.leaveSquad.error = true;
                        this.leaveSquad.userDoesNotBelong = true;
                        break;
                    case "squad_not_found":
                        this.leaveSquad.error = true;
                        this.leaveSquad.squadNotFound = true;
                        break;
                    case "owner_cannot_leave":
                        this.leaveSquad.error = true;
                        this.leaveSquad.ownerCannotLeaveError = true;
                        break;
                }
            }
            
        },
        resetLeaveModal: function() {
            this.leaveSquad.ownerCannotLeaveError = false;
            this.leaveSquad.squadNotFound = false;
            this.leaveSquad.userDoesNotBelong = false;
            this.leaveSquad.unknownError = false;
            this.leaveSquad.requestInProgress = false;
        }
    },
    mounted: async function() {
        await this.fetchSquad();
    },
    template: `
    <section v-if="squadInfo!==undefined">
    <div class="row m-0" v-if="!squadHeaderEditMode">
      <div class="col-12">
        <img src="" width="100" height="100" id="profile_photo" class="profile-pic" :src="imageUrl">
        <h1>{{squadInfo.name}}</h1>
        <p>{{squadInfo.description}}</p>
        <div class="d-flex align-items-center w-100" v-if="!preview">
          <button class="btn btn-outline-primary w-100" @click="$router.push({path: '/miembros', query:{opcion:'invitations', action:'new'}})">
            <i class="fa-solid fa-plus"></i> Invitar
          </button>
          <div class="dropdown ml-auto">
            <button class="btn" data-toggle="dropdown" aria-expanded="false" id="squad-dropdown">
              <i class="fa-solid fa-ellipsis"></i>
            </button>
            <div class="dropdown-menu" aria-labelledby="squad-dropdown">
              <button class="dropdown-item" v-if="userIsAdmin" @click="goToEditMode" href="#">Editar información</button>
              <button class="dropdown-item" data-toggle="modal" data-target="#modal-edit-photo">Cambiar imagen del squad</button>
              <button class="dropdown-item" v-if="userIsAdmin" data-toggle="modal" data-target="#modal-squad-settings">Configuración</button>
              <div class="dropdown-divider"></div>
              <button class="dropdown-item" data-toggle="modal" data-target="#modal-squad-leave">Salir</button>
            </div>
          </div>
        </div>
      </div>
    </div>
    <div class="row m-0" v-else>
      <div class="d-flex flex-column w-100">
        <h3>Editar información del squad</h3>
        <div class="alert alert-danger" v-if="errorUpdating">Ocurrió un error al actualizar</div>
        <div class="form-group">
          <label for="name">Nombre</label>
          <input id="name" class="form-control" v-model="squadInfoForEdit.name"/>
        </div>
        <div class="form-group">
          <label for="description">Descripción</label>
          <input id="description" class="form-control" v-model="squadInfoForEdit.description"/>
        </div>
        <div class="form-group">
          <label for="extDescription">Descripción extendida</label>
          <textarea id="extDescription" class="form-control" v-model="squadInfoForEdit.extendedDescription" placeholder="Markdown es compatible" rows="10"></textarea>
        </div>
        <div class="d-flex justify-content-end mt-1">
          <button class="btn btn-light mr-1" @click="goToEditMode" :disabled="submitting">Cancelar</button>
          <button class="btn btn-primary" @click="updateSquad" :disabled="!updateValidated || submitting">Guardar</button>
        </div>
      </div>
    </div>

    <div class="modal" id="modal-edit-photo" v-if="!preview">
      <div class="modal-dialog modal-dialog-scrollable modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title"><i class="fa-solid fa-image"></i> Cambiar imagen del squad</h5>
            <button type="button" class="close" data-dismiss="modal" aria-label="Close">
              &times;
            </button>
          </div>
          <div class="modal-body">
            <profile-image-maker @imageUpdated="onImageUpdated" :squad-id="squadId" :profile="true"></profile-image-maker>
          </div>
        </div>
      </div>
    </div>
    
    <div class="modal" id="modal-squad-settings" v-if="!preview">
      <div class="modal-dialog modal-dialog-scrollable modal-dialog-centered modal-xl">
        <div class="modal-content h-100">
          <div class="modal-header">
            <h5 class="modal-title">Ajustes del squad</h5>
            <button type="button" class="close" data-dismiss="modal" aria-label="Close">
              &times;
            </button>
          </div>
          <div class="modal-body">
            <squad-settings v-if="squadInfo!==undefined" :squad="squadInfo" />
          </div>
        </div>
      </div>
    </div>
    
    <div class="modal" id="modal-squad-leave" v-if="!preview">
      <div class="modal-dialog modal-dialog-scrollable modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Dejar el squad</h5>
            <button type="button" class="close" data-dismiss="modal" aria-label="Close">
              &times;
            </button>
          </div>
          <div class="modal-body">
            <p>¿Estás seguro de que quieres dejar el squad?</p>
            <div class="alert alert-danger" v-if="leaveSquad.ownerCannotLeaveError">
              No es posible salir del squad si eres el propietario. Antes, designa a un nuevo propietario.
            </div>
            <div class="d-flex justify-content-end">
              <button class="btn btn-light btn-sm mr-1" data-dismiss="modal" @click="resetLeaveModal">No, cancelar</button>
              <button class="btn btn-primary btn-sm" :disabled="leaveSquad.requestInProgress" @click="leave">
                <span v-if="!leaveSquad.requestInProgress">Sí, salir</span>
                <span v-else>
                  <div class="spinner-border spinner-border-sm" role="status">
                    <span class="sr-only">Loading...</span>
                  </div> 
                  Saliendo
                </span>
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
    
    </section>
    `
}

Vue.component('squad-header', squadHeaderComponent);
Vue.component('squad-description', {
    props: {
        text: {
            required: true,
            type: String
        }
    },
    methods: {
        compileMarkdown: function (raw) {
            return DOMPurify.sanitize(marked.parse(raw));
        }
    },
    template: `
    <div class="col-lg-8 bg-white" ref="extendedDescriptionContainer" v-html="compileMarkdown(text)"></div>
    `
});