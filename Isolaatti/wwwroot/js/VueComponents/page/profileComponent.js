Vue.component('profile', {
    props: {
        userId: {
            required: true,
            type: Number
        }
    },
    data: function () {
        return {
            customHeaders: customHttpHeaders,
            currentSection: "discussions" // other are "audios", and "profile-pictures"
        }
    },
    template: `
      <div class="row  m-0">


      <div class="col-xl-3 col-lg-4">
        <profile-info :user-id="userId"></profile-info>
      </div>
      <div class="col-xl-9 col-lg-8">
        <section class="mt-3 d-flex flex-column" id="your-posts-deposit">

          <div class="sticky-top bg-white isolaatti-card p-3 w-100 d-flex justify-content-center mt-3 max-640">

            <div class="btn-group">
              <button class="btn btn-light" :disabled="currentSection==='discussions'"
                      v-on:click="currentSection='discussions'">
                <i class="far fa-newspaper"></i> Discusiones
              </button>
              <button class="btn btn-light" :disabled="currentSection==='audios'"
                      v-on:click="currentSection='audios'">
                <i class="fa-solid fa-ear-listen"></i> Audios
              </button>
              <button class="btn btn-light" :disabled="currentSection==='profile-pictures'"
                      v-on:click="currentSection='profile-pictures'">
                <i class="fa-solid fa-image"></i> Fotos de perfil
              </button>
            </div>
          </div>
          <posts-list :user-id="userId" v-if="currentSection==='discussions'" class="max-640"></posts-list>
          <audios-list :user-id="userId" v-if="currentSection==='audios'" class="max-640"></audios-list>
          <profile-images :user-id="userId" v-if="currentSection==='profile-pictures'" class="max-640"></profile-images>
        </section>
      </div>
      </div>
      
      
    `
})