Vue.component('profile', {
    props: {
        userId: {
            required: true,
            type: Number
        }
    },
    data: function () {
        return {
            customHeaders: customHttpHeaders
        }
    },
    template: `
      <div class="row  m-0">


      <div class="col-xl-3 col-lg-4">
        <profile-info :user-id="userId"></profile-info>
      </div>
      <div class="col-xl-9 col-lg-8">
        <div class="d-flex mt-4">
          <div class=" btn-group btn-group-sm">
            <router-link to="/" class="btn" active-class="btn-primary" :exact="true">Inicio</router-link>
            <router-link to="/interacciones" class="btn" active-class="btn-primary" :extact="true">Interacciones</router-link>
          </div>
        </div>
        <router-view :userId="userId"></router-view>
      </div>
      </div>
      
      
    `
})