const profileStartPage = {
    props: {
        userId: {
            required: true,
            type: Number
        }
    },
    data: function() {
        return {
            currentSection: "discussions" // other are "audios", and "profile-pictures"
        }
    },
    template: `
    <section class="mt-3 d-flex flex-column" id="your-posts-deposit">

          <div class="sticky-top bg-white isolaatti-card p-3 w-100 d-flex justify-content-center mt-3 max-640">

            <div class="btn-group btn-group-sm">
              <button class="btn" :class="[currentSection==='discussions' ? 'btn-primary' : 'btn-light']"
                      v-on:click="currentSection='discussions'">
                <i class="far fa-newspaper"></i> Discusiones
              </button>
              <button class="btn" :class="[currentSection==='audios' ? 'btn-primary' : 'btn-light']"
                      v-on:click="currentSection='audios'">
                <i class="fa-solid fa-ear-listen"></i> Audios
              </button>
              <button class="btn" :class="[currentSection==='profile-pictures' ? 'btn-primary' : 'btn-light']"
                      v-on:click="currentSection='profile-pictures'">
                <i class="fa-solid fa-image"></i> Imagenes
              </button>
            </div>
          </div>
          <posts-list :user-id="userId" v-if="currentSection==='discussions'" class="max-640"></posts-list>
          <audios-list :user-id="userId" v-if="currentSection==='audios'" class="max-640"></audios-list>
          <profile-images :user-id="userId" v-if="currentSection==='profile-pictures'" class="max-640" :user-iself="false"></profile-images>
        </section>
    `
}