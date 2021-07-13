Vue.component('comment', {
    props: ['comment', 'audio-url', 'paused'],
    data: function () {
        return {
            userData: userData,
            sessionToken: sessionToken
        }
    },
    computed: {
        profileLink: function () {
            return `/Profile?id=${this.comment.whoWrote}`
        },
        reportLink: function () {
            return `/Reports/ReportPostOrComment?commentId=${this.comment.id}`;
        }
    },
    methods: {
        compileMarkdown: function (raw) {
            return marked(raw);
        },
        getUserImageUrl: function (userId) {
            return `/api/Fetch/GetUserProfileImage?userId=${userId}&sessionToken=${this.sessionToken}`
        }
    },
    template: `
      <div class="post d-flex mb-2 flex-column p-2">
      <div class="d-flex justify-content-between align-items-center">
        <div class="d-flex">
          <img class="user-avatar" :src="getUserImageUrl(comment.whoWrote)">
          <div class="d-flex flex-column ml-2">
            <span class="user-name"><a :href="profileLink">{{ comment.whoWroteName }}</a> </span>
          </div>
        </div>
        <div class="dropdown dropleft">
          <button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true">
            <i class="fas fa-ellipsis-h"></i>
          </button>
          <div class="dropdown-menu">
            <a href="#" class="dropdown-item" v-if="comment.whoWrote===this.userData.id">Edit</a>
            <a href="#" class="dropdown-item" v-if="comment.whoWrote===this.userData.id">Delete</a>
            <a :href="reportLink" class="dropdown-item" target="_blank">Report</a>
          </div>
        </div>
      </div>

      <div class="d-flex mt-1" v-if="comment.audioUrl!=null">
        <button type="button" class="btn btn-primary btn-sm" v-on:click="$emit('play-audio')">
          <i class="fas fa-play" v-if="comment.audioUrl !== audioUrl || paused"></i>
          <i class="fas fa-pause" v-else></i>
        </button>
      </div>
      <div class="mt-2 post-content" v-html="compileMarkdown(comment.textContent)"></div>
      </div>
    `
})