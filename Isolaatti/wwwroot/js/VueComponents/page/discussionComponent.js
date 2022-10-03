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
        }
    },
    mounted: async function () {
        await this.fetchPost();
        socket.emit("subscribe-scope", {
            type: "post",
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
                               :is-full-page="true">
                </post-template>
          
              </div>
              <div :class="{'col-md-6': layout.showCommentsRight}">
                <div class="d-flex flex-column ml-4">
                  <comments-viewer :post-id="postId" :is-under-post="false"></comments-viewer>
                </div>
              </div>
            </div>
          </div>
        </section>
    `
}
Vue.component('discussion-page', discussionComponent)