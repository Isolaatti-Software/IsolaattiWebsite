Vue.component('comment', {
    props: ['comment'],
    data: function () {
        return {
            userData: userData,
            cutContent: true
        }
    },
    computed: {
        profileLink: function () {
            return `/perfil/${this.comment.authorId}`
        },
        reportLink: function () {
            return `/Reports/ReportPostOrComment?commentId=${this.comment.id}`;
        },
        containerCssClass: function () {
            return this.cutContent ? "post d-flex mt-2 flex-column p-2 post-cut-height" : "post d-flex mt-2 flex-column p-2"
        },
        titleAtr: function () {
            return `id del comentario: ${this.comment.id}`
        }
    },
    methods: {
        compileMarkdown: function (raw) {
            return DOMPurify.sanitize(marked.parse(raw));
        },
        getUserImageUrl: function (userId) {
            return `/api/Fetch/GetUserProfileImage?userId=${userId}`
        },
        showFullPost: function () {
            this.cutContent = false;
        }
    },
    template: `
      <article class="d-flex align-items-center">
      <div class="comments-start-line"></div>
      <div :class="containerCssClass" :title="titleAtr">
        <div class="d-flex justify-content-between align-items-center">
          <div class="d-flex">
            <img class="user-avatar" :src="getUserImageUrl(comment.authorId)">
            <div class="d-flex flex-column ml-2">
              <span class="user-name"><a :href="profileLink">{{ comment.authorName }}</a> </span>
              <span>{{ new Date(comment.timeStamp).toUTCString() }}</span>
            </div>
          </div>
          <div class="dropdown dropleft" v-if="userData.id!==-1">
            <button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true">
              <i class="fas fa-ellipsis-h"></i>
            </button>
            <div class="dropdown-menu">
              <a href="#" class="dropdown-item" v-if="comment.authorId===this.userData.id">Editar</a>
              <a href="#" class="dropdown-item" v-if="comment.authorId===this.userData.id"
                 v-on:click="$emit('delete-comment')">Eliminar</a>
              <a :href="reportLink" class="dropdown-item" target="_blank">Reportar</a>
            </div>
          </div>
        </div>

        <div class="mt-2 post-content" v-html="compileMarkdown(comment.content)" ref="commentContentContainer"></div>
        <div class="d-flex justify-content-center">
          <button class="btn btn-primary " v-on:click="showFullPost" v-if="cutContent">Mostrar todo</button>
        </div>
      </div>
      </article>
    `,
    mounted: function () {
        this.cutContent = this.$refs.commentContentContainer.scrollHeight > this.$refs.commentContentContainer.clientHeight;
    }
})