
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
        <div class="container-fluid">
        <div class="row m-0 isolaatti-card">
          <div class="col-12">
            <squad-header :squad-id="squadId"></squad-header>
          </div>
        </div>
        <div class="row m-0">
          <div class="col-xl-3 col-lg-4">
            <div class="isolaatti-card mt-2">
              <div class="d-flex flex-column w-100">
                <h3 class="m-2 text-center">Squad</h3>
                <router-link to="/" class="btn btn-light mb-1">Actividad</router-link>
                <router-link to="/contenido" class="btn btn-light mb-1">Contenido</router-link>
                <router-link to="/miembros" class="btn btn-light mb-1">Miembros</router-link>
                <router-link to="/wiki" class="btn btn-light mb-1">Wiki</router-link>
                <router-link to="/ajustes" class="btn btn-light mb-1">Ajustes</router-link><hr>
                <button class="btn btn-danger mb-1" data-toggle="modal" data-target="#modal-quit-squad">Salir</button>
              </div>
            </div>
          </div>
          <div class="col-xl-9 col-lg-8">
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