Vue.component('turn-into-admin-squad-modal-content',{
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
        sendAddAdminRequest: async function() {
            this.sending = true;
            this.error = undefined;
            const response = await fetch(`/api/squads/${this.squadId}/AddAdmin`,{
                method: "post",
                headers: this.customHeaders,
                body: JSON.stringify({
                    id: this.selectedUser.id
                })
            });
            this.sending = false;
            if(response.ok) {
                window.location.reload();
            }
            try {
                this.error = await response.json();
            } catch(e) {
                this.error = {
                    result: "unknown_server_error"
                }
            }

        }
    },
    template: `
        <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">Convertir en administrador</h5>
              <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                <span aria-hidden="true">&times;</span>
              </button>
            </div>
            <div class="modal-body" v-if="selectedUser !== undefined">
              <div class="alert alert-danger" v-if="error">
                {{error}}
              </div>
              <p>Al hacer esto, <strong>{{selectedUser.name}}</strong> será convertido en administrador</p>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" data-dismiss="modal" :disabled="sending">No, cancelar</button>
              <button type="button" class="btn btn-primary" @click="sendAddAdminRequest" :disabled="sending">Sí, proceder</button>
            </div>
          </div>
    `
})