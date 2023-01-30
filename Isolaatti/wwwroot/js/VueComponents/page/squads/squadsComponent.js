const squadsComponent = {
    data: function() {
        return {
            path: ""
        }
    },
    methods: {

    },
    mounted: function() {

    },
    template: `
        <div class="container">
          <div class="row m-0">
            <div class="col-xl-3 col-lg-4">
              <aside class="mt-4 p-2 isolaatti-card">
                <div class="translucent-white-card">
                  <h3 class="m-2"><i class="fa-solid fa-people-group"></i> Squads</h3>
                  <div class="d-flex flex-row flex-lg-column mt-3 flex-wrap justify-content-md-center">
                    <router-link to="/crear" class="btn" active-class="btn-primary" :exact="true"><i class="fa-solid fa-plus"></i> Nuevo squad</router-link>
                    <div class="dropdown-divider w-100"></div>
                    <router-link to="/" class="btn btn-sm" active-class="btn-primary" :exact="true">Squads donde eres miembro</router-link>
                    <router-link to="/tuyos" class="btn btn-sm" active-class="btn-primary" :exact="true">Tus Squads</router-link>
                    <router-link to="/solicitudes_union" class="btn btn-sm" active-class="btn-primary" :exact="true">Solicitudes</router-link>
                    <router-link to="/invitaciones" class="btn btn-sm" active-class="btn-primary" :exact="true">Invitaciones</router-link>
                  </div>
                </div>
              </aside>
            </div>
            <div class="col-xl-9 col-lg-8">
              <router-view class="mt-4"></router-view>
            </div>
          
          </div>
        </div>
    `
};

// This is "this page component"
Vue.component('squads-page', squadsComponent);