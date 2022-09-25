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
            post: undefined
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
      <div class="row  m-0">
      <div class="col-md-6">
        <post-template v-if="post!==undefined"
                       :post="post.postData"
                       :theme="post.theme"
                       :key="postId"
                       :is-full-page="true">
        </post-template>
        
      </div>
      <div class="col-md-6">
        <div class="d-flex flex-column ml-4">
          <h5 class="m-2">Comentarios</h5>
          <comments-viewer :post-id="postId" :is-under-post="false"></comments-viewer>
        </div>
      </div>
      </div>
    `
}
Vue.component('discussion-page', discussionComponent)