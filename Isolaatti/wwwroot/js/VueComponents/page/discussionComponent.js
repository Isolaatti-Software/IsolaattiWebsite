const discussionComponent = {
    props: {
        postId: {
            type: Number,
                required: true
        }
    },
    data: function () {
        return {
            customHeaders: customHttpHeaders,
            userId: userData.id,
            post: undefined,
            
            layout: {
                isFluid: false,
                showCommentsRight: true
            },

            postDetails: {
                post: undefined,
                usersWhoLiked: []
            }
        }
    },
    methods: {
        fetchPost: function () {
            const that = this;
            fetch(`/api/Fetch/Post/${this.postId}`, {headers: this.customHeaders}).then(result => {
                result.json().then(post => {
                    that.post = post;
                    that.loading = false;
                })
            })
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
        await this.fetchPost();
        socket.emit("subscribe-scope", {
            type: "new_comment",
            id: this.postId
        });
    },
    template: `
        <section>
          <div class="container">
            <div class="row">
              <div class="col-12 d-flex justify-content-end p-2 align-items-center">
                <div class="btn-group mr-2">
                  <button class="btn" 
                          :class="{
                                    'btn-primary': !layout.showCommentsRight,
                                    'btn-light': layout.showCommentsRight
                                  }" 
                          @click="layout.showCommentsRight = !layout.showCommentsRight" 
                          title="Mostrar comentarios a la derecha o abajo">
                    <i class="fa-solid fa-arrows-down-to-line"></i>
                  </button>
                  <button class="btn" 
                          :class="{
                                    'btn-primary': layout.isFluid,
                                    'btn-light': !layout.isFluid
                                  }" 
                          @click="layout.isFluid = !layout.isFluid" 
                          title="Mostrar layout normal o expandido">
                    <i class="fa-solid fa-expand"></i>
                  </button>
                </div>
              </div>
            </div>
          </div>
          <div :class="{'container': !layout.isFluid, 'container-fluid': layout.isFluid}">
            
            <div class="m-0" :class="{'row': layout.showCommentsRight}">
              <div :class="{'col-md-6': layout.showCommentsRight}">
                <post-template v-if="post!==undefined"
                               :post="post"
                               :key="postId"
                               @details="putPostDetails"
                               :is-full-page="true">
                </post-template>
          
              </div>
              <div :class="{'col-md-6': layout.showCommentsRight}">
                <div class="d-flex flex-column ml-4">
                  <comments-viewer v-if="post!==undefined" :post-id="postId" :is-under-post="false" :numberOfComments="post.numberOfComments"></comments-viewer>
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
}
Vue.component('discussion-page', discussionComponent)