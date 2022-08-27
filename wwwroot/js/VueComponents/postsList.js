/*
* Isolaatti project, 2022
* Erik Cavazos
* Posts list component
*/
Vue.component("posts-list", {
    props: {
        userId: {
            type: Number,
            required: true
        }
    },
    data: function () {
        return {
            customHeaders: customHttpHeaders,
            posts: [],
            moreContent: false,
            loading: false,
            lastId: -1,
            filterData: {
                privacy: {
                    private: true,
                    isolaatti: true,
                    public: true
                },
                content: "all"
            },
            sortingData: {
                ascending: "0"
            },
            postDetails: {
                post: undefined,
                usersWhoLiked: []
            }
        }
    },
    methods: {
        fetchPosts: function (lastId, event) {
            this.loading = true;
            if (event !== undefined) {
                event.target.disabled = true;
            }
            fetch(`/api/Fetch/PostsOfUser/${this.userId}/8/${lastId}?olderFirst=${this.sortingData.ascending === "1" ? "True" : "False"}`, {headers: this.customHeaders}).then(result => {
                result.json().then(res => {
                    this.posts = this.posts.concat(res.feed);
                    this.moreContent = res.moreContent;
                    this.loading = false;
                    if (event !== undefined) {
                        event.target.disabled = false;
                    }
                    this.lastId = res.lastId;
                })
            })
        },
        reloadPosts: function (event) {
            this.posts = [];
            this.fetchPosts(-1, event);
        },
        concatPost: function (post) {
            this.posts = [post].concat(this.posts);
        },
        removePost: function (postId) {
            const index = this.posts.findIndex(p => p.postData.id === postId);
            if (index === -1) {
                return;
            }
            this.posts.splice(index, 1);
        },
        putPostDetails: async function (post) {
            this.postDetails.post = post;


            const response = await fetch(`/api/Fetch/Post/${post.id}/LikedBy`, {
                headers: this.customHeaders
            });
            this.postDetails.usersWhoLiked = await response.json();
        },
        userProfileLink: function (userId) {
            return `/perfil/${userId}`;
        },
        profilePictureUrl: function (imageId) {
            if (imageId === null) {
                return "/res/imgs/user-solid.png";
            }
            return `/api/Fetch/ProfileImages/${imageId}.png`;
        }
    },
    mounted: function () {
        this.fetchPosts(-1);
        events.$on("posted", this.concatPost);
        events.$on("postDeleted", this.removePost);
    },
    template: `
      <section class="d-flex flex-column pt-1 mt-2 mb-3 align-items-center w-100">
      <h5><i class="far fa-newspaper"></i> Discusiones</h5>
      <div class="d-flex justify-content-end w-100">
        <div class="btn-group">
          <button type="button" class="btn btn-secondary" title="Filtrar"
                  data-target="#modal-filter-posts" data-toggle="modal">
            <i class="fas fa-filter"></i>
          </button>
          <button type="button" class="btn btn-secondary" title="Ordenar"
                  data-target="#modal-sort-posts" data-toggle="modal">
            <i class="fas fa-sort"></i>
          </button>
        </div>
      </div>
      <p class="m-2 text-center" v-if="posts.length === 0 && !loading"> No hay contenido que mostrar <i
          class="fa-solid fa-face-sad-cry"></i></p>
      <post-template v-for="post in posts" :post="post.postData"
                     v-bind:theme="post.theme"
                     v-bind:preview="false"
                     v-bind:key="post.postData.id" v-on:delete="" @details="putPostDetails">
      </post-template>
      <div v-if="loading" class="d-flex justify-content-center mt-2">
        <div class="spinner-border" role="status">
          <span class="sr-only">Cargando más contenido...</span>
        </div>
      </div>
      <div v-if="moreContent" class="d-flex flex-column align-items-center mt-2">
        <button class="btn btn-primary" v-on:click="fetchPosts(lastId, $event)">Cargar más</button>
      </div>
      <!-- Modal filter posts -->
      <div class="modal fade" id="modal-filter-posts">
        <div class="modal-dialog modal-dialog-centered">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">Filtrar</h5>
              <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                <span aria-hidden="true">&times;</span>
              </button>
            </div>
            <div class="modal-body">
              <h6>Contenido</h6>
              <select class="custom-select" v-model="filterData.content">
                <option value="all">Todo</option>
                <option value="withAudio">Solo con audio</option>
                <option value="withoutAudio">Solo sin audio</option>
              </select>

              <h6 class="mt-3">Privacidad</h6>
              <div class="form-check form-check-inline">
                <input class="form-check-input" type="checkbox" id="privacy-filter-option-private"
                       v-model="filterData.privacy.private">
                <label class="form-check-label" for="privacy-filter-option-private">Privado</label>
              </div>
              <div class="form-check form-check-inline">
                <input class="form-check-input" type="checkbox" id="privacy-filter-option-isolaatti"
                       v-model="filterData.privacy.isolaatti">
                <label class="form-check-label" for="privacy-filter-option-isolaatti">Usuarios de Isolaatti</label>
              </div>
              <div class="form-check form-check-inline">
                <input class="form-check-input" type="checkbox" id="privacy-filter-option-everybody"
                       v-model="filterData.privacy.public">
                <label class="form-check-label" for="privacy-filter-option-everybody">Todos</label>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Modal sort posts -->
      <div class="modal fade" id="modal-sort-posts">
        <div class="modal-dialog modal-dialog-centered">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">Ordenar</h5>
              <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                <span aria-hidden="true">&times;</span>
              </button>
            </div>
            <div class="modal-body">
              <select class="custom-select" v-model="sortingData.ascending" v-on:change="reloadPosts">
                <option value="0">Más nuevo primero</option>
                <option value="1">Más viejo primero</option>
              </select>
            </div>
          </div>
        </div>
      </div>
      
      <div class="modal" id="modal-post-info">
        <div class="modal-dialog modal-dialog-centered">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">Detalles</h5>
              <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                <span aria-hidden="true">&times;</span>
              </button>
            </div>
            <div class="modal-body">
              <h6>Personas que dieron like</h6>
              <div class="list-group list-group-flush">
                <a class="list-group-item list-group-item-action" v-for="user in postDetails.usersWhoLiked" :href="userProfileLink(user.id)">
                  <img class="user-avatar" :src="profilePictureUrl(user.profileImageId)">
                  <span>{{user.name}}</span>
                </a>
              </div>
            </div>
          </div>
        </div>
      </div>
      </section>

    `
})