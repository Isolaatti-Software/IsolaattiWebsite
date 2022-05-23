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
        justAddedComments: {
            type: Array,
            default: function () {
                return []
            }
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
        mixedComments: function () {
            return this.justAddedComments.concat(this.comments);
        },
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
        globalEventEmmiter.$on("commentDeleted", function (id) {
            const index = that.comments.findIndex(c => c.id === id);
            if (index === -1) return;

            that.comments.splice(index, 1);
        });
    },
    template: `
      <div :class="mixedComments.length > 0 ? 'd-flex flex-column comments-section pt-2' : 'd-flex flex-column'">
      <h5 v-if="mixedComments.length===0 && !isUnderPost" class="m-4 text-center"><i class="fas fa-sad-tear"></i> No hay
        comentarios que mostrar</h5>
      <comment v-for="comment in mixedComments" :comment="comment" class="w-auto" :key="comments.id"></comment>
      <a :href="openThreadLink" v-if="comments.length < numberOfComments && isUnderPost" class="text-center">Ver
        discusión</a>
      <a href="#" v-on:click="fetchComments" class="text-center mt-2"
         v-if="!isUnderPost && comments.length < numberOfComments">Cargar más</a>
      </div>
    `
})