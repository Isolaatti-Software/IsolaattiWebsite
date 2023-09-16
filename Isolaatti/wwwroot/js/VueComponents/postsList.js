/*
* Isolaatti project, 2022
* Erik Cavazos
* Posts list component
*/
Vue.component("posts-list", {
    props: {
        userId: {
            type: Number,
            required: false
        },
        squadId: {
            type: String,
            required: false
        }
    },
    data: function () {
        return {
            customHeaders: customHttpHeaders,
            userData: userData,
            posts: [],
            moreContent: false,
            loading: false,
            lastId: -1,
            showingFilter: false,
            sortingData: {
                ascending: "0"
            },
            filter: {
                includeAudio: "both", // onlyAudio, onlyNoAudio
                includeFromSquads: "both", // onlyFromSquads, onlyNotFromSquads
                dateRange: {
                    enabled: false,
                    from: "1900-01-01",
                    to: "3000-12-31"
                }
            },
            postDetails: {
                post: undefined,
                usersWhoLiked: []
            }
        }
    },
    methods: {
        fetchPosts: async function (event) {
            this.loading = true;
            if (event !== undefined) {
                event.target.disabled = true;
            }
            if(this.squadId !== undefined) {
                const response = await fetch(
                        `/api/Squads/${this.squadId}/Posting/Posts?lastId=${this.lastId}&length=10&olderFirst=${this.sortingData.ascending === "1" ? "True" : "False"}`, 
                        {
                            headers: this.customHeaders
                        });
                
                const parsedResponse = await response.json();
                this.posts = this.posts.concat(parsedResponse.data);
                this.moreContent = parsedResponse.moreContent;
                this.loading = false;
                if (event !== undefined) {
                    event.target.disabled = false;
                }
                if(this.posts.length > 0)
                    this.lastId = this.posts[this.posts.length - 1].id;
            } else {
                const response = await fetch(
                    `/api/Fetch/PostsOfUser/${this.userId}?lastId=${this.lastId}&length=25&olderFirst=${this.sortingData.ascending === "1" ? "True" : "False"}&filterJson=${encodeURIComponent(JSON.stringify(this.filter))}`, 
                    {
                        headers: this.customHeaders
                    });
                const parsedResponse = await response.json();

                this.posts = this.posts.concat(parsedResponse.data);
                this.moreContent = parsedResponse.moreContent;
                this.loading = false;
                if (event !== undefined) {
                    event.target.disabled = false;
                }
                if(this.posts.length > 0)
                    this.lastId = this.posts[this.posts.length - 1].post.id;
            }
        },
        reloadPosts: async function (event) {
            this.posts = [];
            this.lastId = -1;
            await this.fetchPosts(event);
        },
        concatPost: function (post) {
            this.posts = [post].concat(this.posts);
        },
        removePost: function (postId) {
            const index = this.posts.findIndex(p => p.post.id === postId);
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
                return "/res/imgs/avatar.svg";
            }
            return `/api/images/image/${imageId}?mode=small`;
        }
    },
    mounted: async function () {
        await this.fetchPosts();
        events.$on("posted", this.concatPost);
        events.$on("postDeleted", this.removePost);
    },
    template: `
      <section class="d-flex flex-column pt-1 mt-2 mb-3 align-items-center w-100">
      
      <div class="d-flex w-100 justify-content-end">
        <button class="btn ml-1" :class="{'btn-primary': showingFilter, 'btn-light': !showingFilter}" @click="showingFilter = !showingFilter"><i class="fa-solid fa-filter"></i></button>
        <div class="dropdown">
          <button type="button" class="btn" data-toggle="dropdown" aria-haspopup="true"><i class="fas fa-ellipsis-h" aria-hidden="true"></i></button>
          <div class="dropdown-menu">
            <a href="#" class="dropdown-item">Borrado por lotes</a>
            <a href="#" class="dropdown-item">Descarga tu información</a>
          </div>
        </div>
      </div>
      <div v-if="showingFilter" class="d-flex flex-column align-items-start w-100 isolaatti-card mt-2">
        <div class="w-100">
          <h6>Ordenamiento</h6>
          <select class="custom-select" v-model="sortingData.ascending">
            <option value="0">Más nuevo primero</option>
            <option value="1">Más viejo primero</option>
          </select>
          <h6 class="mt-1">Audio</h6>
          <select class="custom-select" v-model="filter.includeAudio">
            <option value="both">No filtrar</option>
            <option value="onlyAudio">Solo con audio</option>
            <option value="onlyNoAudio">Solo sin audio</option>
          </select>
          <template v-if="userId===userData.id">
            <h6 class="mt-1">Squad</h6>
            <select class="custom-select" v-model="filter.includeFromSquads">
              <option value="both">No filtrar</option>
              <option value="onlyFromSquads">Solo de squads</option>
              <option value="onlyNotFromSquads">Solo que no sea de squads</option>
            </select>
          </template>
          <h6 class="mt-2">Fecha</h6>
          <div class="form-group">
            <label for="date-filter-check" >Filtrar por fecha</label>
            <input type="checkbox" class="custom-checkbox" id="date-filter-check" v-model="filter.dateRange.enabled">
          </div>
          <div class="d-flex w-100" v-if="filter.dateRange.enabled">
            
            <div class="form-group mr-1 w-50">
              <label for="date_from">Desde</label>
              <input type="date" id="date_from" class="form-control" v-model="filter.dateRange.from">
            </div>
            <div class="form-group ml-1 w-50">
              <label for="date_to">Hasta</label>
              <input type="date" id="date_to" class="form-control" v-model="filter.dateRange.to">
            </div>
          </div>
          <div class="d-flex justify-content-end">
            <button class="btn btn-primary" @click="reloadPosts">Traer</button>
          </div>
        </div>
      </div>
      <p class="m-2 text-center" v-if="posts.length === 0 && !loading"> No hay contenido que mostrar <i
          class="fa-solid fa-face-sad-cry"></i></p>
      <post-template v-for="post in posts" :post="post"
                     v-bind:preview="false"
                     v-bind:key="post.post.id" v-on:delete="" @details="putPostDetails">
      </post-template>
      <div v-if="loading" class="d-flex justify-content-center mt-2">
        <div class="spinner-border" role="status">
          <span class="sr-only">Cargando más contenido...</span>
        </div>
      </div>
      <div v-if="moreContent" class="d-flex flex-column align-items-center mt-2">
        <button class="btn btn-light" v-on:click="fetchPosts($event)">Cargar más</button>
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
              <h6>Personas que aplaudieron a esto</h6>
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