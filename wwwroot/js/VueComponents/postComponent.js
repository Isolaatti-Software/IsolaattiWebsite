Vue.component('post-template',{
    props: {
        post: Object,
        isFullPage: false,
        preview: {
            type: Boolean,
            default: false
        }
    },
    data: function () {
        return {
            userData: userData,
            cutContent: true,
            customHeaders: customHttpHeaders,
            thisTime: Date.now(),
            editable: false,
            renderPost: undefined,
            deleteDialog: false,
            viewCommenter: false
        }
    },
    computed: {
        profileLink: function () {
            return `/perfil/${this.post.userId}`
        },
        reportLink: function () {
            return this.preview ? "#" : `/Reports/ReportPostOrComment?postId=${this.post.id}`;
        },
        openThreadLink: function () {
            return this.preview ? "#" : `/pub/${this.post.id}`;
        },
        editPostLink: function () {
            return this.preview ? "#" : `/editor?edit=True&postId=${this.post.id}`
        },
        containerCssClass: function () {
            return this.cutContent ? "d-flex flex-column p-2 post post-cut-height" : "d-flex flex-column p-2 mt-3 post"
        },
        rootContainerCss: function () {
            return this.editable || this.deleteDialog ? "background-color: #f8f9fa; padding:0.2rem" : "";
        },
        squadUrl: function() {
            if(this.renderPost.squadId === undefined) {
                return "";
            }
            
            return `/squads/${this.renderPost.squadId}`;
        }
    },
    watch: {
        post: {
            immediate: true,
            deep: true,
            handler: function (value, old) {
                this.renderPost = value;
            }
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
        },
        like: async function (event) {
            if (this.preview) return;

            const postId = this.renderPost.id;
            const requestData = {id: postId}
            const globalThis = this;
            event.target.disabled = true;
            const response = await fetch("/api/Likes/LikePost", {
                method: "POST",
                headers: this.customHeaders,
                body: JSON.stringify(requestData),
            });

            response.json().then(function (res) {
                globalThis.renderPost = res.postData;
                event.target.disabled = false;
            });
        },
        unlike: async function (event) {
            if (this.preview) return;

            const postId = this.renderPost.id;
            const requestData = {id: postId}
            const globalThis = this;
            event.target.disabled = true;
            const response = await fetch("/api/Likes/UnLikePost", {
                method: "POST",
                headers: this.customHeaders,
                body: JSON.stringify(requestData),
            });

            response.json().then(function (res) {
                globalThis.renderPost = res.postData;
                event.target.disabled = false;
            });
        },
        updateFromModified: function (feedPost) {
            this.renderPost = feedPost.postData;
            this.editable = false;
        },
        deletePost: function () {
            const that = this;
            fetch("/api/Posting/Delete", {
                method: "POST",
                headers: this.customHeaders,
                body: JSON.stringify({
                    id: this.renderPost.id
                })
            }).then(function () {
                events.$emit("postDeleted", that.renderPost.id);
            });
        },
        openDiscussion: function () {
            window.location = this.openThreadLink;
        }
    },
    template: `
      <div class="w-100 mt-2" v-if="renderPost !== undefined" :style="rootContainerCss">
      <section v-if="editable" class="d-flex justify-content-end p-1">
        <button @click="editable=false" class="btn btn-danger btn-sm">Cancelar edición</button>
      </section>
      <section v-if="deleteDialog" class="w-100 mt-2 d-flex justify-content-end align-items-center p-1">
        <span class="mr-auto">¿Eliminar esta discusión?</span>
        <button class="btn btn-light mr-1" @click="deleteDialog=false">No</button>
        <button class="btn btn-primary" @click="deletePost">Sí</button>
      </section>
      <article class="d-flex flex-column w-100" v-if="!editable">
        <div :class="containerCssClass">
          <div class="d-flex justify-content-between align-items-center">
            <div class="d-flex">
              <img class="user-avatar" :src="getUserImageUrl(renderPost.userId)">
              <div class="d-flex flex-column ml-2">
                <span class="user-name"><a :href="profileLink">{{ renderPost.username }}</a> </span>
                <span class="small" v-if="renderPost.squadId!==undefined">
                  <a :href="squadUrl">{{ renderPost.squadName }}</a>
                </span>
                <div class="d-flex privacy-icon-container">
                  
                  <span>{{ new Date(renderPost.timeStamp).toUTCString() }}</span>
                </div>
              </div>
            </div>
            <div class="dropdown dropleft" v-if="!preview">
              <button class="btn btn-transparent btn-sm" data-toggle="dropdown" aria-haspopup="true">
                <i class="fas fa-ellipsis-h"></i>
              </button>
              <div class="dropdown-menu">
                <a href="#" class="dropdown-item" v-if="renderPost.userId===this.userData.id" @click="editable=true">Editar</a>
                <a href="#modal-share-post" v-on:click="$emit('input',openThreadLink)" class="dropdown-item"
                   data-toggle="modal">Compartir</a>
                <a href="#" class="dropdown-item" v-if="renderPost.userId===this.userData.id"
                   v-on:click="deleteDialog=true">Eliminar</a>
                <a :href="reportLink" class="dropdown-item" target="_blank" v-if="userData.id!==-1">Reportar</a>
              </div>
            </div>
          </div>

          <audio-attachment :audio-id="renderPost.audioId" v-if="renderPost.audioId!==null"></audio-attachment>
          <div class="mt-2 post-content" v-html="compileMarkdown(renderPost.content)" ref="postContentContainer" @click="openDiscussion"></div>
          <div class="d-flex justify-content-center">
            <button class="btn btn-primary btn-sm" v-on:click="showFullPost" v-if="cutContent">Mostrar todo</button>
          </div>
          <div class="d-flex justify-content-end">
            <button class="btn btn-transparent mr-auto btn-sm" data-toggle="modal" data-target="#modal-post-info" 
                    @click="$emit('details', post)"><i class="fa-solid fa-info"></i> </button>
            <div class="btn-group btn-group-sm" v-if="userData.id!==-1">

              <button class="btn btn-transparent" @click="viewCommenter = !viewCommenter" :class="{'text-primary-lighter':viewCommenter}">
                <i class="fas fa-comments"></i>
                {{ renderPost.numberOfComments }}
              </button>
              <button v-if="!renderPost.liked" v-on:click="like($event)" class="btn btn-transparent btn-sm" type="button">
                <i class="fas fa-thumbs-up" aria-hidden="true"></i> {{ renderPost.numberOfLikes }}
              </button>
              <button v-if="renderPost.liked" v-on:click="unlike($event)" class="text-primary-lighter btn btn-transparent  btn-sm"
                      type="button">
                <i class="fas fa-thumbs-up" aria-hidden="true"></i> {{ renderPost.numberOfLikes }}
              </button>
            </div>
          </div>
        </div>
        <div v-if="!isFullPage && !preview && viewCommenter" class="d-flex flex-column">
          <comments-viewer :post-id="post.id" :is-under-post="true"
                           :number-of-comments="post.numberOfComments"></comments-viewer>
        </div>

      </article>
      <new-discussion v-else :mode="'modify'" :post-to-modify-id="post.id"
                      @modified="updateFromModified"></new-discussion>
      </div>
    `,
    mounted: function () {
        const that = this;
        Prism.highlightAllUnder(this.$el);
        this.$nextTick(function () {
            this.cutContent = this.$refs.postContentContainer.scrollHeight > this.$refs.postContentContainer.clientHeight;
        });
    }
})