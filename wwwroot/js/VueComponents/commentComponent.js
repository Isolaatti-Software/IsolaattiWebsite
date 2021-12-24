Vue.component('comment', {
    props: ['comment', 'audio-url', 'paused'],
    data: function () {
        return {
            userData: userData,
            cutContent: true
        }
    },
    computed: {
        profileLink: function () {
            return `/perfil/${this.comment.whoWrote}`
        },
        reportLink: function () {
            return `/Reports/ReportPostOrComment?commentId=${this.comment.id}`;
        },
        containerCssClass: function () {
            return this.cutContent ? "post d-flex mb-2 flex-column p-2 post-cut-height" : "post d-flex mb-2 flex-column p-2"
        },
        titleAtr: function () {
            return `id del comentario: ${this.comment.id}`
        }
    },
    methods: {
        compileMarkdown: function (raw) {
            return marked.parse(raw);
        },
        getUserImageUrl: function (userId) {
            return `/api/Fetch/GetUserProfileImage?userId=${userId}`
        },
        showFullPost: function () {
            this.cutContent = false;
        }
    },
    template: `
      <div :class="containerCssClass" :title="titleAtr">
      <div class="d-flex justify-content-between align-items-center">
        <div class="d-flex">
          <img class="user-avatar" :src="getUserImageUrl(comment.whoWrote)">
          <div class="d-flex flex-column ml-2">
            <span class="user-name"><a :href="profileLink">{{ comment.whoWroteName }}</a> </span>
            <span>{{ new Date(comment.date).toUTCString() }}</span>
          </div>
        </div>
        <div class="dropdown dropleft" v-if="userData.id!==-1">
          <button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true">
            <i class="fas fa-ellipsis-h"></i>
          </button>
          <div class="dropdown-menu">
            <a href="#" class="dropdown-item" v-if="comment.whoWrote===this.userData.id">Editar</a>
            <a href="#" class="dropdown-item" v-if="comment.whoWrote===this.userData.id"
               v-on:click="$emit('delete-comment')">Eliminar</a>
            <a :href="reportLink" class="dropdown-item" target="_blank">Reportar</a>
          </div>
        </div>
      </div>

      <div class="d-flex mt-1" v-if="comment.audioUrl!=null">
        <button type="button" class="btn btn-primary btn-sm" v-on:click="$emit('play-audio')">
          <i class="fas fa-play" v-if="comment.audioUrl !== audioUrl || paused"></i>
          <i class="fas fa-pause" v-else></i>
        </button>
      </div>
      <div class="mt-2 post-content" v-html="compileMarkdown(comment.textContent)" ref="commentContentContainer"></div>
      <div class="d-flex justify-content-center">
        <button class="btn btn-primary " v-on:click="showFullPost" v-if="cutContent">Mostrar todo</button>
      </div>
      </div>
    `,
    mounted: function () {
        this.cutContent = this.$refs.commentContentContainer.scrollHeight > this.$refs.commentContentContainer.clientHeight;
    }
})