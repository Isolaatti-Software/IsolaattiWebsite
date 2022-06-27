Vue.component('post-template',{
    // ['post','paused','is-modal', 'theme', "audioUrl", "preview"]
    props: {
        post: Object,
        isFullPage: false,
        preview: {
            type: Boolean,
            default: false
        },
        theme: {
            type: Object,
            default: {
                fontColor: "#000",
                backgroundColor: "#FFFFFF",
                gradient: false,
                border: {
                    color: "#FFFFFF",
                    type: "solid",
                    size: 0,
                    radius: 5
                },
                background: {
                    type: "linear",
                    colors: ["#FFFFFF", "#30098EE5"],
                    direction: 0
                }
            }
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
            renderTheme: undefined,
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
        }
    },
    watch: {
        post: {
            immediate: true,
            deep: true,
            handler: function (value, old) {
                this.renderPost = value;
            }
        },
        theme: {
            immediate: true,
            deep: true,
            handler: function (value, old) {
                this.renderTheme = value;
            }
        }
    },
    methods: {
        compileMarkdown: function (raw) {
            return DOMPurify.sanitize(marked.parse(raw));
        },
        getPostStyle: function (theme) {
            if (theme === null)
                return "";

            function returnColorsAsString(array) {
                let res = "";

                for (let i = 0; i < array.length - 1; i++) {
                    res += array[i] + ", "
                }

                res += array[array.length - 1];

                return res;
            }

            let backgroundProperty;

            // if a gradient of any kind is selected it will generate the corresponding background,
            // otherwise it will return the solid color
            if (theme.gradient) {
                backgroundProperty = theme.background.type ===
                "linear" ?
                    `linear-gradient(${theme.background.direction}deg, ${returnColorsAsString(theme.background.colors)})` :
                    `radial-gradient(${returnColorsAsString(theme.background.colors)})`;
            } else {
                backgroundProperty = theme.backgroundColor;
            }

            return `color: ${theme.fontColor ?? "#000"};
                background: ${backgroundProperty};
                border: ${theme.border.size ?? ""}px ${theme.border.type ?? ""} ${theme.border.color ?? ""};
                border-radius: ${theme.border.radius ?? ""}px;`;
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
            this.renderTheme = feedPost.theme;
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
                globalEventEmmiter.$emit("postDeleted", that.renderPost.id);
            });
        }
    },
    template: `
      <div class="w-100 mt-2" v-if="renderPost !== undefined && renderTheme !== undefined" :style="rootContainerCss">
      <section v-if="editable" class="d-flex justify-content-end p-1">
        <button @click="editable=false" class="btn btn-danger btn-sm">Cancelar edición</button>
      </section>
      <section v-if="deleteDialog" class="w-100 mt-2 d-flex justify-content-end align-items-center p-1">
        <span class="mr-auto">¿Eliminar esta discusión?</span>
        <button class="btn btn-light mr-1" @click="deleteDialog=false">No</button>
        <button class="btn btn-primary" @click="deletePost">Sí</button>
      </section>
      <article class="d-flex flex-column w-100" v-if="!editable">
        <div :class="containerCssClass" :style="getPostStyle(renderTheme)">
          <div class="d-flex justify-content-between align-items-center">
            <div class="d-flex">
              <img class="user-avatar" :src="getUserImageUrl(renderPost.userId)">
              <div class="d-flex flex-column ml-2">
                <span class="user-name"><a :href="profileLink">{{ renderPost.username }}</a> </span>
                <div class="d-flex privacy-icon-container">
                  <div v-if="renderPost.privacy === 1">
                    <i class="fas fa-user" title="Private" aria-hidden="true"></i><span class="sr-only">Privado</span>
                  </div>
                  <div v-if="renderPost.privacy === 2">
                    <i class="fas fa-user-friends" title="People on Isolaatti" aria-hidden="true"></i><span
                      class="sr-only">Usuarios de Isolaatti</span>
                  </div>
                  <div v-if="renderPost.privacy === 3">
                    <i class="fas fa-globe" title="All the world" aria-hidden="true"></i><span
                      class="sr-only">Todos</span>
                  </div>
                  <span>{{ new Date(renderPost.timeStamp).toUTCString() }}</span>
                </div>
              </div>
            </div>
            <div class="dropdown dropleft" v-if="!preview">
              <button class="btn btn-light btn-sm" data-toggle="dropdown" aria-haspopup="true">
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
          <div class="mt-2 post-content" v-html="compileMarkdown(renderPost.content)" ref="postContentContainer"></div>
          <div class="d-flex justify-content-center">
            <button class="btn btn-primary btn-sm" v-on:click="showFullPost" v-if="cutContent">Mostrar todo</button>
          </div>
          <div class="d-flex justify-content-end">
            <a class="btn btn-light mr-auto btn-sm" :href="openThreadLink"><i class="fas fa-external-link-alt"></i> </a>
            <div class="btn-group btn-group-sm" v-if="userData.id!==-1">

              <button class="btn btn-light" @click="viewCommenter = !viewCommenter" :disabled="viewCommenter">
                <i class="fas fa-comments"></i>
                {{ renderPost.numberOfComments }}
              </button>
              <button v-if="!renderPost.liked" v-on:click="like($event)" class="btn btn-light btn-sm" type="button">
                <i class="fas fa-thumbs-up" aria-hidden="true"></i> {{ renderPost.numberOfLikes }}
              </button>
              <button v-if="renderPost.liked" v-on:click="unlike($event)" class="btn btn-light btn-sm btn-liked"
                      type="button">
                <i class="fas fa-thumbs-up" aria-hidden="true"></i> {{ renderPost.numberOfLikes }}
              </button>
            </div>
          </div>
        </div>
        <div v-if="!isFullPage && !preview && viewCommenter" class="d-flex flex-column">
          <div class="d-flex justify-content-end mt-1">
            <button class="btn btn-light btn-sm close" @click="viewCommenter = !viewCommenter">&times;</button>
          </div>
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
        this.$nextTick(function () {
            this.cutContent = this.$refs.postContentContainer.scrollHeight > this.$refs.postContentContainer.clientHeight;
        });
    }
})