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
        },
        numberOfComments: {
            type: Number,
            required: true
        }
    },
    data: function () {
        return {
            customHeaders: customHttpHeaders,
            clientId: clientId,
            comments: [],
            loading: false,
            moreContent: false
        }
    },
    computed: {
        openThreadLink: function () {
            return this.preview ? "#" : `/pub/${this.postId}`;
        }
    },
    methods: {
        fetchComments: async function (event) {
            this.loading = true;
            const that = this;
            if (event !== undefined) {
                event.target.disabled = true;
            }
            const response = await fetch(
                `/api/Fetch/Post/${this.postId}/Comments?take=10${this.comments.length < 1 ? "" : "&lastId=" + this.comments[this.comments.length - 1].comment.id}`, 
                {
                    headers: this.customHeaders
                });
            
            const commentsDto = await response.json();

            this.comments = this.comments.concat(commentsDto.data);
            this.loading = false;
            this.moreContent = commentsDto.moreContent;
            if (event !== undefined) {
                event.target.disabled = false;
            }
        },
        onCommentRemoved: function(id) {
            const index = this.comments.findIndex(c => c.comment.id === id);
            if (index === -1) return;

            this.comments.splice(index, 1);
        },
        onCommentEdited: function(comment) {
            const index = this.comments.findIndex(c => c.comment.id === comment.comment.id);
            if (index === -1) return;

            this.comments.splice(index, 1, comment);
        },
        onCommentAdded: function(comingComment) {
            this.comments.push(comingComment);
        }
    },
    mounted: async function () {
        const that = this;
        await this.fetchComments();

        // Local events
        events.$on("comment-edited", this.onCommentEdited);
        events.$on("comment-removed", this.onCommentRemoved);
        events.$on("comment-added", this.onCommentAdded);
    },
    template: `
      <div class="d-flex flex-column comments-section">
      <h5 v-if="comments.length===0 && !isUnderPost" class="m-4 text-center"><i class="fas fa-sad-tear"></i> No hay
        comentarios que mostrar</h5>
      <comment v-for="comment in comments" :comment="comment" class="w-auto" :key="comment.comment.id"
               @commentDeleted="onCommentRemoved" @updated="onCommentEdited"></comment>
      <a :href="openThreadLink" v-if="comments.length < numberOfComments && isUnderPost" class="text-center">Ver
        discusión</a>
      <a href="#" v-on:click="fetchComments" class="text-center mt-2"
         v-if="!isUnderPost && comments.length < numberOfComments">Cargar más</a>
      <new-comment :post-to-comment="postId" class="mt-2" @commented="onCommentAdded"></new-comment>
      </div>
      
    `
})