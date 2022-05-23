Vue.component('new-comment', {
    props: {
        mode: {
            type: String,
            default: "new",
            required: false
        },
        postToComment: {
            type: Number,
            required: true
        },
        commentToEdit: {
            type: Number,
            required: false
        }
    },
    data: function () {
        return {
            customHeaders: customHttpHeaders,
            userData: userData,
            audioMode: "none",
            posting: false,
            comment: {
                id: -1,
                content: "",
                authorId: userData.id,
                authorName: userData.name,
                postId: -1,
                targetUserId: 1,
                privacy: 1,
                audioId: null,
                timeStamp: new Date()
            }
        }
    },
    computed: {
        ableToPostDiscussion: function () {
            return this.comment.content.length >= 1;
        },
        uniqueDomIdForPreviewModal: function () {
            return `modal-preview-post-${this.comment.id}`;
        }
    },
    methods: {
        postComment: function () {
            const that = this;
            this.posting = true;

            let url = `/api/Posting/Post/${this.postToComment}/Comment`;

            if (this.mode === "modify") {
                url = `/api/Comment/${this.commentToEdit}/Edit`;
            }

            fetch(url, {
                method: "POST",
                headers: this.customHeaders,
                body: JSON.stringify({
                    content: this.comment.content,
                    audioId: this.comment.audioId,
                    privacy: this.comment.privacy
                })
            }).then(res => res.json()).then(function (postedComment) {
                that.posting = false;
                that.comment = {
                    id: -1,
                    content: "",
                    authorId: userData.id,
                    authorName: userData.name,
                    postId: -1,
                    targetUserId: 1,
                    privacy: 1,
                    audioId: null,
                    timeStamp: new Date()
                };
                if (that.mode === "modify") {
                    globalEventEmmiter.$emit("commentEdited", postedComment);
                    that.$emit("commentEdited");
                } else {
                    that.$emit("commented", postedComment);
                }

            });
        },
        audioPosted: function (id) {
            this.comment.audioId = id;
            this.audioMode = "none";
        },
        removeAudio: function () {
            this.comment.audioId = null;
        },
        setAudio: function (audioId) {
            this.comment.audioId = audioId;
        }
    },
    mounted: function () {
        if (this.mode === "modify") {
            const that = this;
            fetch(`/api/Fetch/Comments/${this.commentToEdit}`, {
                method: "GET",
                headers: this.customHeaders
            }).then(res => res.json()).then(function (comment) {
                that.comment = comment;
            });
        }
    },
    template: `
      <section>
      <div class="btn-group btn-group-sm mb-1">
        <button class="btn btn-light" v-on:click="audioMode='newAudio'"
                :disabled="audioMode==='newAudio'">
          <i class="fa-solid fa-microphone"></i>
        </button>
        <button class="btn btn-light" v-on:click="audioMode='existingAudio'"
                :disabled="audioMode==='existingAudio'">
          <i class="fa-solid fa-file-audio"></i>
        </button>
      </div>

      <div class="btn-group btn-group-sm mb-1">
        <button class="btn btn-light" :data-target="'#' + uniqueDomIdForPreviewModal" data-toggle="modal">
          <i class="fa-solid fa-eye"></i> Vista previa
        </button>
      </div>


      <div v-if="audioMode==='newAudio'">
        <div class="d-flex justify-content-end">
          <button class="btn btn-light btn-sm close" v-on:click="audioMode='none'">&times;</button>
        </div>

        <audio-recorder class="mt-2" :is-discussion="true" v-on:audio-posted="audioPosted"></audio-recorder>
      </div>
      <div v-if="audioMode==='existingAudio'">
        <div class="d-flex justify-content-end">
          <span class="mr-auto">Audio existente</span>
          <button class="btn btn-light btn-sm close" v-on:click="audioMode='none'">&times;</button>
        </div>
        <audios-list-select v-on:audio-selected="setAudio"></audios-list-select>
      </div>
      <audio-attachment class="mt-2"
                        :audio-id="comment.audioId"
                        :can-remove="true"
                        v-if="comment.audioId!==null"
                        v-on:remove="removeAudio"></audio-attachment>

      <textarea class="mt-2 isolaatti-discussion-text" v-model="comment.content"
                placeholder="Escribe aqui el contenido para iniciar la discusión. Markdown es compatible."></textarea>

      <div class="d-flex justify-content-end mt-2">
        <button class="btn btn-primary" :disabled="!ableToPostDiscussion" v-on:click="postComment" v-if="!posting">
          {{ mode === "modify" ? "Guardar" : "Publicar" }}
        </button>
        <div v-else class="d-flex align-items-center mt-1">
          <div class="spinner-border mr-1" role="status">
            <span class="sr-only">Publicando...</span>
          </div>
        </div>
      </div>


      <div class="modal" :id="uniqueDomIdForPreviewModal">
        <div class="modal-dialog modal-dialog-centered">
          <div class="modal-content">
            <div class="modal-header">
              <div class="modal-title">Vista previa</div>
            </div>
            <div class="modal-body">
              <comment :comment="comment"></comment>
            </div>
          </div>
        </div>
      </div>
      </section>
    `
})