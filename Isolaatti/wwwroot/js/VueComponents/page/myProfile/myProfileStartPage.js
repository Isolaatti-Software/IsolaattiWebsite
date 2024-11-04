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
          <div style="min-height: 500px">
            <posts-list :user-id="userData.id" v-if="currentSection==='discussions'" class="max-640"></posts-list>
          </div>
</div>
    `
}