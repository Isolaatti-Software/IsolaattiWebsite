Vue.component('new-discussion', {
    props: {
        mode: {
            type: String,
            default: "new",
            required: false
        },
        postToModifyId: {
            type: Number,
            required: false
        },
        squadId: {
            type: String,
            required: false,
            default: null
        }
    },
    data: function () {
        return {
            customHeaders: customHttpHeaders,
            userData: userData,
            selectionStart: 0,
            selectionEnd: 0,
            audioMode: "none",
            posting: false,
            showPreview: false,
            discussion: {
                post: {
                    id: -1,
                    textContent: "",
                    userId: userData.id,
                    privacy: 2,
                    date: new Date(),
                    audioId: null,
                    squadId: null,
                    linkedDiscussionId: null,
                    linkedCommentId: null
                },
                numberOfLikes: 0,
                numberOfComments: 0,
                userName: userData.name,
                squadName: null,
                liked: false
            }
        }
    },
    computed: {
        ableToPostDiscussion: function () {
            return this.discussion.post.textContent.length >= 1;
        },
        uniqueDomIdForPreviewModal: function () {
            return `modal-preview-post-${this.discussion.post.id}`;
        }
    },
    methods: {
        postDiscussion: async function () {
            const that = this;
            this.posting = true;
            let endpointUrl = "/api/Posting/Make";
            let requestBody = {
                privacy: that.discussion.post.privacy,
                content: that.discussion.post.textContent,
                audioId: that.discussion.post.audioId,
                squadId: that.squadId
            }
            if (this.mode === "modify") {
                endpointUrl = "/api/Posting/Edit";
                requestBody.postId = that.discussion.post.id;
            }


            let response = await fetch(endpointUrl, {
                method: "POST",
                body: JSON.stringify(requestBody),
                headers: this.customHeaders
            });

            if (!response.ok) {
                that.$emit("error", "Error posting discussion");
                return;
            }

            let madePost = await response.json();
            this.discussion.post.textContent = "";
            this.posting = false;
            if (this.mode !== "modify")
                events.$emit("posted", madePost);
            else
                this.$emit("modified", madePost)
        },
        audioPosted: function (id) {
            this.discussion.audioId = id;
            this.audioMode = "none";
        },
        removeAudio: function () {
            this.discussion.audioId = null;
        },
        setAudio: function (audioId) {
            this.discussion.audioId = audioId;
        }
    },
    mounted: function () {
        this.$nextTick(function () {
            if (this.mode === "modify") {
                fetch(`/api/Fetch/Post/${this.postToModifyId}`, {
                    method: "GET",
                    headers: this.customHeaders
                }).then(response => response.json())
                    .then(data => {
                        this.discussion = data;
                    });
            }
        });
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
        <button class="btn" :class="[showPreview ? 'btn-primary' : 'btn-light']" @click="showPreview=!showPreview">
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
      
      <post-template v-if="showPreview" :post="discussion" :preview="true"></post-template>
      
      <audio-attachment class="mt-2"
                        :audio-id="discussion.post.audioId"
                        :can-remove="true"
                        v-if="discussion.post.audioId!==null"
                        v-on:remove="removeAudio"></audio-attachment>

      <textarea class="mt-2 form-control" v-model="discussion.post.textContent"
                placeholder="Escribe aqui el contenido para iniciar la discusión. Markdown es compatible."></textarea>

      <div class="d-flex justify-content-end mt-2">
        <button class="btn btn-primary" :disabled="!ableToPostDiscussion" v-on:click="postDiscussion" v-if="!posting">
          {{ mode === "modify" ? "Guardar" : "Publicar" }}
        </button>
        <div v-else class="d-flex align-items-center mt-1">
          <div class="spinner-border mr-1" role="status">
            <span class="sr-only">Publicando...</span>
          </div>
        </div>
      </div>
      </section>

    `
})