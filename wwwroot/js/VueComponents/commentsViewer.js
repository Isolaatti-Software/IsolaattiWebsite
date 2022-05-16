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
        },
        numberOfComments: {
            type: Number,
            default: 0
        }
    },
    data: function () {
        return {
            customHeaders: customHttpHeaders,
            comments: [],
            loading: false
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
            fetch(`/api/Fetch/Post/${this.postId}/Comments/4/${this.comments.length < 1 ? "" : this.comments[this.comments.length - 1].id}`, {headers: this.customHeaders}).then(result => {
                result.json().then(res => {
                    this.comments = this.comments.concat(res);
                    this.loading = false;
                    if (event !== undefined) {
                        event.target.disabled = false;
                    }
                    this.lastId = res.lastId;
                })
            })
        }
    },
    mounted: function () {
        this.fetchComments();
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