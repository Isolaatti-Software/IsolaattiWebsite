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
        <div class="container-fluid">
        <div class="row">
          <div class="col-12 m-0 jumbotron">
            <h3 class="text-center"><i class="fa-solid fa-people-group"></i> Squads</h3>
            <p class="text-center">Permanece en contacto con las personas que te importan</p>
          </div>
        </div>
          <div class="row">
            <div class="col-12">
              <div class="container">
                <div class="row">
                  <div class="col-lg-2"></div>
                  <div class="col-lg-8">
                    <router-view class="mt-4"></router-view>
                  </div>
                  <div class="col-lg-2"></div>
                </div>
              </div>
            </div>
          </div>
        </div>
    `
};

// This is "this page component"
Vue.component('squads-page', squadsComponent);