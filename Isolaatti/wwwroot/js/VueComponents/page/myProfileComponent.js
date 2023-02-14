Vue.component('my-profile', {
    data: function () {
        return {
            userData: userData,
        }
    },
    methods: {},
    mounted: async function () {

    },
    template: `
      <div class="row m-0">
      <div class="col-xl-3 col-lg-4">
        <profile-info :user-id="userData.id"></profile-info>
      </div>
      <div class="col-xl-9 col-lg-8">
        
        
        
        <div class="d-flex mt-4">
          <div class=" btn-group btn-group-sm">
            <router-link to="/" class="btn" active-class="btn-primary" :exact="true">Inicio</router-link>
            <router-link to="/interacciones" class="btn" active-class="btn-primary" :extact="true">Interacciones</router-link>
          </div>
        </div>

        <router-view></router-view>
      </div>
      </div>
    `

})