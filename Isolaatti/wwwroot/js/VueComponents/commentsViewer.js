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
    watch: {
        comments: function (old, newValue) {
            const that = this;
            setTimeout(function () {
                that.scrollToBottom();
            }, 200);
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
        onCommentRemoved: function (id) {
            const index = this.comments.findIndex(c => c.id === id);
            if (index === -1) return;

            this.comments.splice(index, 1);
        },
        scrollToBottom: function () {
            let appDiv = document.getElementById("app-main");
            let appHeight = appDiv.scrollHeight;
            console.log(appHeight);
            appDiv.scrollTo(0, appHeight);
        }
    },
    mounted: async function () {
        const that = this;
        this.fetchComments();

        events.$on("commentEdited", function (comment) {
            const index = that.comments.findIndex(c => c.id === comment.id);
            if (index === -1) return;

            that.comments.splice(index, 1, comment);
        });
        
        socket.on(`new_comment`, function(comingComment) {
            console.log(comingComment);
            that.comments.push(comingComment);
        });
        
        socket.on("comment-removed", that.onCommentRemoved);
    },
    template: `
      <div class="d-flex flex-column comments-section">
      <h5 v-if="comments.length===0 && !isUnderPost" class="m-4 text-center"><i class="fas fa-sad-tear"></i> No hay
        comentarios que mostrar</h5>
      <comment v-for="comment in comments" :comment="comment" class="w-auto" :key="comment.comment.id"
               @commentDeleted="onCommentRemoved"></comment>
      <a :href="openThreadLink" v-if="comments.length < numberOfComments && isUnderPost" class="text-center">Ver
        discusión</a>
      <a href="#" v-on:click="fetchComments" class="text-center mt-2"
         v-if="!isUnderPost && comments.length < numberOfComments">Cargar más</a>
      <new-comment :post-to-comment="postId" class="mt-2"></new-comment>
      </div>
      
    `
})