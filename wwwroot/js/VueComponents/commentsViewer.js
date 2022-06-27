/*
* Isolaatti project, 2022
* Erik Cavazos
* Comments viewer component
*/
Vue.component("comments-viewer", {
    props: {
        postId: {
            type: Number,
            required: true
        },
        isUnderPost: {
            type: Boolean,
            default: false
        }
    },
    data: function () {
        return {
            customHeaders: customHttpHeaders,
            comments: [],
            loading: false,
            numberOfComments: 0
        }
    },
    computed: {
        openThreadLink: function () {
            return this.preview ? "#" : `/pub/${this.postId}`;
        }
    },
    methods: {
        fetchComments: function (event) {
            this.loading = true;
            if (event !== undefined) {
                event.target.disabled = true;
            }
            fetch(`/api/Fetch/Post/${this.postId}/Comments/10/${this.comments.length < 1 ? "" : this.comments[this.comments.length - 1].id}`, {headers: this.customHeaders}).then(result => {
                result.json().then(res => {
                    this.comments = this.comments.concat(res);
                    this.loading = false;
                    if (event !== undefined) {
                        event.target.disabled = false;
                    }
                    this.lastId = res.lastId;
                })
            })
        },
        fetchNumberOfComments: function () {
            const that = this;
            fetch(`/api/Posting/Post/${this.postId}/CommentsNumber`, {
                method: "GET",
                headers: this.customHeaders
            }).then(res => res.json()).then(function (parsedRes) {
                that.numberOfComments = parsedRes;
            });
        },
        onCommented: function (comment) {
            this.comments = [comment].concat(this.comments)
        },
        onCommentRemoved: function (id) {
            const index = this.comments.findIndex(c => c.id === id);
            if (index === -1) return;

            this.comments.splice(index, 1);
        }
    },
    mounted: function () {
        const that = this;
        this.fetchComments();
        this.fetchNumberOfComments();
        globalEventEmmiter.$on("commentEdited", function (comment) {
            const index = that.comments.findIndex(c => c.id === comment.id);
            if (index === -1) return;

            that.comments.splice(index, 1, comment);
        });
    },
    template: `
      <div class="d-flex flex-column comments-section pt-2">
      <h5>Comentarios</h5>
      <new-comment :post-to-comment="postId" @commented="onCommented"></new-comment>
      <h5 v-if="comments.length===0 && !isUnderPost" class="m-4 text-center"><i class="fas fa-sad-tear"></i> No hay
        comentarios que mostrar</h5>
      <comment v-for="comment in comments" :comment="comment" class="w-auto" :key="comments.id"
               @commentDeleted="onCommentRemoved"></comment>
      <a :href="openThreadLink" v-if="comments.length < numberOfComments && isUnderPost" class="text-center">Ver
        discusión</a>
      <a href="#" v-on:click="fetchComments" class="text-center mt-2"
         v-if="!isUnderPost && comments.length < numberOfComments">Cargar más</a>
      </div>
    `
})