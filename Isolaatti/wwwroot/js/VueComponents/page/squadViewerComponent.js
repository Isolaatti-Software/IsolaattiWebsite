
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
            userData: userData
        }
    },
    methods: {
        fetchSquadMembers: async function() {
            
        },
        fetchSquadPosts: async function() {
            
        }
    },
    computed: {
        
    },
    mounted: async function() {
        
    },
    template:`
        <div class="container">
          <div class="row isolaatti-card">
            <div class="col-12">
              <squad-header :squad-id="squadId"></squad-header>
            </div>
          </div>
          <div class="row mt-2 isolaatti-card">
            <div class="col-12">
              <div class="btn-group overflow-auto mb-1 mt-1">
                <router-link to="/" class="btn" active-class="btn-primary" :exact="true">Inicio</router-link>
                <router-link :to="{path: '/miembros', query: {tab: 'members'}}" active-class="btn-primary" class="btn">Personas</router-link>
              </div>
            </div>
          </div>
          <div class="row">
          
            <div class="col-12 p-0">
              <div class="mt-2">
                <router-view :squadId="squadId"></router-view>
              </div>
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