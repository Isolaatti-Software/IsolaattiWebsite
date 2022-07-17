Vue.component('feed', {
    data: function () {
        return {
            customHeaders: customHttpHeaders,
            userData: userData,
            lastPostGotten: 0,
            posts: [],
            loading: true,
            noMoreContent: false,
            postDetails: {
                post: undefined,
                usersWhoLiked: []
            }
        }
    },
    methods: {
        fetchFeed: async function () {
            const globalThis = this;
            const response = await fetch(`/api/Feed/${this.lastPostGotten}/10`, {
                method: "GET",
                headers: this.customHeaders
            });
            response.json().then(feed => {
                globalThis.posts = globalThis.posts.concat(feed.posts)
                globalThis.lastPostGotten = feed.lastPostId;
                globalThis.noMoreContent = !feed.moreContent;
                globalThis.loading = false;
            })
        },
        refresh: function () {
            this.posts = [];
            this.lastPostGotten = 0;
            this.noMoreContent = false;
            this.loading = true;
            this.fetchFeed();
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
    mounted: async function () {
        await this.fetchFeed();
        globalEventEmmiter.$on("posted", this.concatPost);
        globalEventEmmiter.$on("postDeleted", this.removePost);
    },
    template: `
      <section id="posts-deposit" class="d-flex flex-column pt-1 mt-2 mb-3 align-items-center posts-deposit">
      <h5 class="mt-2"><i class="far fa-newspaper"></i> Actividad de las personas que sigues</h5>
      <post-template v-for="post in posts" :post="post.postData"
                     v-bind:theme="post.theme"
                     v-bind:key="post.postData.id" @details="putPostDetails">
      </post-template>
      <div v-if="loading" class="d-flex justify-content-center mt-2">
        <div class="spinner-border" role="status">
          <span class="sr-only">Cargando más contenido...</span>
        </div>
      </div>
      <div v-if="noMoreContent" class="d-flex flex-column align-items-center mt-2">
        <p>Ya viste toda la actividad por ahora, refresca en un momento...</p>
        <button class="btn btn-light" v-on:click="refresh()">
          <i class="fas fa-redo"></i>
        </button>
      </div>
      <div v-else>
        <button class="btn btn-light" v-on:click="fetchFeed()">Cargar más</button>
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