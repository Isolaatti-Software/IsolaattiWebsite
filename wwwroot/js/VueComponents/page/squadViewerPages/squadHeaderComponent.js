﻿const squadHeaderComponent = {
    props: {
        squadId: {
            required: true,
            type: String
        }
    },
    data: function() {
        return {
            customHeaders: customHttpHeaders,
            userData: userData,
            squadInfo: undefined,
            squadHeaderEditMode: false, // this should only be toggled by squad admin

            errorUpdating: false,
            submitting: false,
            squadInfoForEdit: {
                name: "",
                description: "",
                extendedDescription: ""
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
        }
    },
    methods: {
        compileMarkdown: function (raw) {
            return DOMPurify.sanitize(marked.parse(raw));
        },
        toggleEditMode: function() {
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
        }
    },
    mounted: async function() {
        await this.fetchSquad();
    },
    template: `
    <section v-if="squadInfo!==undefined">
    <div class="row m-0" v-if="userIsAdmin">
      <div class="col-12 d-flex justify-content-end">
        <button class="btn btn-sm" @click="toggleEditMode" v-if="!squadHeaderEditMode">
          <i class="fa-solid fa-pencil"></i>
        </button>
      </div>
    </div>

    <div class="row m-0" v-if="!squadHeaderEditMode">
      <div class="col-lg-4">
        <h1>{{squadInfo.name}}</h1>
        <p>{{squadInfo.description}}</p>
      </div>
      <div class="col-lg-8" v-html="compileMarkdown(squadInfo.extendedDescription)"></div>
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
          <button class="btn btn-light mr-1" @click="toggleEditMode" :disabled="submitting">Cancelar</button>
          <button class="btn btn-primary" @click="updateSquad" :disabled="!updateValidated || submitting">Guardar</button>
        </div>
      </div>
    </div>
    </section>
    `
}

Vue.component('squad-header', squadHeaderComponent);