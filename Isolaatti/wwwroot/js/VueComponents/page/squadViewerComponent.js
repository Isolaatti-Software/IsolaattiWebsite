
Vue.component('squad-view', {
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
            description: ""
        }
    },
    methods: {
        onDescription: function(text) {
            this.description = text;
        }
    },
    mounted: async function() {
        
    },
    template:`
        <div class="container">
          <div class="row">
            <div class="col-md-4">
              <squad-header :squad-id="squadId" @description="onDescription"></squad-header>
            </div>
            <div class="col-md-8">
              <squad-description :text="description" class="mt-2 mb-2"></squad-description>
              <div class="btn-group overflow-auto mb-2 mt-3 isolaatti-card w-100">
                <router-link to="/" class="btn" active-class="btn-primary" :exact="true">Inicio</router-link>
                <router-link :to="{path: '/miembros', query: {tab: 'members'}}" active-class="btn-primary" class="btn">Personas</router-link>
                <router-link :to="{path: '/imagenes'}" active-class="btn-primary" class="btn">Imágenes</router-link>
              </div>
              <router-view :squadId="squadId"></router-view>
            </div>
          </div>
        
        <div class="modal" id="modal-quit-squad">
          <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
              <div class="modal-header">
                <h5 class="modal-title">Salir del Squad</h5>
                <button type="button" class="close" data-dismiss="modal" aria-label="Close">&times</button>
              </div>
              <div class="modal-body">
                <p>¿Estás seguro que desea salir del Squad? Para volver tendrás que enviar solicitud o recibir invitación.</p>
              </div>
              <div class="modal-footer">
                <div class="d-flex justify-content-end">
                  <button type="button" class="btn btn-light btn-sm mr-2" data-dismiss="modal" aria-label="Close">Cancelar</button>
                  <button type="button" class="btn btn-primary btn-sm">Salir definitivamente</button>
                </div>
              </div>
            </div>
          </div>
        </div>
        </div>
    `
});