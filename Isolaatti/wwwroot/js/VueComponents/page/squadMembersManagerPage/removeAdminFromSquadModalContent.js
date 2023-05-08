Vue.component('remove-admin-from-squad-modal-content',{
    props: {
        squadId: {
            type: String,
            required: true
        },
        selectedUser: {
            required: true,
            type: Object
        }
    },
    data: function() {
        return {
            customHeaders: customHttpHeaders,
            error: undefined,
            sending: false
        }
    },
    methods:  {
        removeAdminFromSquad: async function() {
            try {
                this.sending = true;
                const response = await fetch(`/api/Squads/${this.squadId}/RemoveAdmin`,{
                    method: "post",
                    headers: this.customHeaders,
                    body: JSON.stringify({
                        id: this.selectedUser.id
                    })
                });
                this.sending = false;
                if(!response.ok){
                    try {
                        const e = await response.json();
                        this.error = `Error en el servidor: ${e.result}`;
                    } catch(e) {
                        this.error = "Error desconocido"
                    }
                    return;
                }
                window.location.reload();
            } catch(e) {
                this.error = "Error de red";
            }
        }
    },
    template: `
        <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">Quitar administrador</h5>
              <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                <span aria-hidden="true">&times;</span>
              </button>
            </div>
            <div class="modal-body" v-if="selectedUser !== undefined">
              <div class="alert alert-danger">
                {{error}}
              </div>
              <p>Al hacer esto, {{selectedUser.name}} dejará de ser administrador para ser miembro normal</p>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" data-dismiss="modal" :disabled="sending">No, cancelar</button>
              <button type="button" class="btn btn-primary" @click="removeAdminFromSquad" :disabled="sending">Sí, proceder</button>
            </div>
          </div>
    `
})