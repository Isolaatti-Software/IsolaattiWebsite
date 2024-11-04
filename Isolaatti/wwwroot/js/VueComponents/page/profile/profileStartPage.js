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
      
          <div style="min-height: 500px">
            <posts-list :user-id="userId" v-if="currentSection==='discussions'" class="max-640"></posts-list>
          </div>
        </section>
    `
}