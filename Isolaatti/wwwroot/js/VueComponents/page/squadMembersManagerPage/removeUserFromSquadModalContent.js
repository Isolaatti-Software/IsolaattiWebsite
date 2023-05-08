Vue.component('remove-squad-user-modal-content', {
    props: {
        selectedUser: {
            required: true,
            type: Object
        }
    },
    template: `
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
    `
})