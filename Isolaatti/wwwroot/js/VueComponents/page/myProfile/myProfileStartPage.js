const myProfileStartPage = {
    data: function() {
        return {
            customHeaders: customHttpHeaders,
            userData: userData,
            currentSection: "discussions" // other are "audios", and "profile-pictures"
        }
    },
    template: `
    <div>
              <new-post
              v-on:new-discussion=""
              v-on:new-audio="">
          </new-post>
          <div class="sticky-top bg-white isolaatti-card p-3 d-flex justify-content-center mt-3 max-640">
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
          <div style="min-height: 500px">
            <posts-list :user-id="userData.id" v-if="currentSection==='discussions'" class="max-640"></posts-list>
            <audios-list :user-id="userData.id" v-if="currentSection==='audios'" class="max-640"></audios-list>
            <profile-images :user-id="userData.id" v-if="currentSection==='profile-pictures'"
                            class="max-640"></profile-images>
          </div>
</div>
    `
}