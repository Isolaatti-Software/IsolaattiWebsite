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
            discussion: {
                audioId: null,
                content: "",
                description: null,
                id: -1,
                liked: false,
                numberOfComments: 0,
                numberOfLikes: 0,
                privacy: 2,
                timeStamp: new Date(),
                title: null,
                userId: userData.id,
                username: userData.name
            },
            theme: {
                fontColor: "#000",
                backgroundColor: "#FFFFFF",
                gradient: false,
                border: {
                    color: "#FFFFFF",
                    type: "solid",
                    size: 0,
                    radius: 5
                },
                background: {
                    type: "linear",
                    colors: ["#FFFFFF", "#30098EE5"],
                    direction: 0
                }
            },
            defaultThemes: [] // this is populated from a hosted json
        }
    },
    computed: {
        ableToPostDiscussion: function () {
            return this.discussion.content.length >= 1;
        },
        uniqueDomIdForPreviewModal: function () {
            return `modal-preview-post-${this.discussion.id}`;
        },
        uniqueDomIdForPrivacyModal: function () {
            return `modal-privacy-post-${this.discussion.id}`;
        },
        uniqueDomIdForThemeModal: function () {
            return `modal-theme-post-${this.discussion.id}`;
        }
    },
    methods: {
        postDiscussion: async function () {
            const that = this;
            this.posting = true;
            let endpointUrl = "/api/Posting/Make";
            let requestBody = {
                privacy: that.discussion.privacy,
                content: that.discussion.content,
                theme: that.theme,
                audioId: that.discussion.audioId
            }
            if (this.mode === "modify") {
                endpointUrl = "/api/Posting/Edit";
                requestBody.postId = that.discussion.id;
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
            this.discussion.content = "";
            this.posting = false;
            if (this.mode !== "modify")
                globalEventEmmiter.$emit("posted", madePost);
            else
                this.$emit("modified", madePost)
        },
        audioPosted: function (id) {
            this.discussion.audioId = id;
            this.audioMode = "none";
        },
        addColor: function () {
            this.theme.background.colors.push("#FFFFFF");
        },
        removeColor: function (index) {
            this.theme.background.colors.splice(index, 1);
        },
        switchColors: function (color1Index, color2Index) {
            // interchange color 1 and color 2
            // I do it this way because Vue reactivity doesn't detect changes on values when they are made directly with index
            let temp = this.theme.background.colors[color1Index];
            this.theme.background.colors.splice(color1Index, 1, this.theme.background.colors[color2Index]);
            this.theme.background.colors.splice(color2Index, 1, temp);
        },
        applyDefaultTheme: function (event) {
            this.theme = this.defaultThemes[event.target.value].data;
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
            // populate default themes
            fetch('/json/defaultThemes.json')
                .then(response => response.json())
                .then(data => this.defaultThemes = data.themes);

            if (this.mode === "modify") {
                fetch(`/api/Fetch/Post/${this.postToModifyId}`, {
                    method: "GET",
                    headers: this.customHeaders
                }).then(response => response.json())
                    .then(data => {
                        this.discussion = data.postData;
                        this.theme = data.theme
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
        <button class="btn btn-light" :data-target="'#' + uniqueDomIdForThemeModal" data-toggle="modal"
                v-if="mode!=='comment'">
          <i class="fa-solid fa-palette"></i> Tema
        </button>
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
                        :audio-id="discussion.audioId"
                        :can-remove="true"
                        v-if="discussion.audioId!==null"
                        v-on:remove="removeAudio"></audio-attachment>

      <textarea class="mt-2 isolaatti-discussion-text" v-model="discussion.content"
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
      <div class="modal" :id="uniqueDomIdForThemeModal">
        <div class="modal-dialog modal-dialog-centered modal-dialog-scrollable">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">
                <i class="fas fa-sliders-h"></i> Personaliza el tema
              </h5>
              <button class="close" data-dismiss="modal">&times;</button>
            </div>
            <div class="modal-body">
              <post-template :post="discussion" :theme="theme" :preview="true"></post-template>
              <h5 class="card-title">Predefinido</h5>
              <div class="form-group">
                <select class="custom-select custom-select-sm" v-on:input="applyDefaultTheme">
                  <option v-for="(theme, index) in defaultThemes" :value="index">{{ theme.name }}</option>
                </select>
              </div>
              <h5 class="card-title">Color de letra</h5>
              <div class="form-group">
                <input type="color" class="form-control form-control-sm" v-model="theme.fontColor">
              </div>

              <h5 class="card-title">Fondo</h5>
              <div class="form-check mb-1">
                <input class="form-check-input" type="checkbox" id="isGradient" v-model="theme.gradient"
                       :true-value="true" :false-value="false">
                <label class="form-check-label" for="isGradient">
                  Gradiente
                </label>
              </div>
              <div v-if="theme.gradient">
                <select class="custom-select" v-model="theme.background.type">
                  <option value="linear">Gradiente lineal</option>
                  <option value="radial">Gradiente radial</option>
                </select>
                <div v-if="theme.background.type==='linear'" class="mt-2">
                  <div class="form-group">
                    <div class="input-group input-group-sm">
                      <input type="number" v-model.number="theme.background.direction"
                             class="form-control form-control-sm"/>
                      <div class="input-group-prepend">
                        <span class="input-group-text">grados</span>
                      </div>
                    </div>
                    <div class="d-flex mt-2" v-for="(color,index) in theme.background.colors">
                      <div class="btn-group">
                        <button type="button" class="btn btn-primary" :disabled="index===0"
                                v-on:click="switchColors(index,index-1)">
                          <i class="fas fa-chevron-up"></i>
                        </button>
                        <button type="button" class="btn btn-primary"
                                :disabled="index===theme.background.colors.length - 1"
                                v-on:click="switchColors(index,index+1)">
                          <i class="fas fa-chevron-down"></i>
                        </button>
                      </div>
                      <input type="color" v-model="theme.background.colors[index]" class="form-control"/>
                      <button type="button" class="btn btn-primary btn-sm ml-1"
                              v-on:click="removeColor(index)"
                              v-bind:disabled="theme.background.colors.length <= 2">
                        <i class="fas fa-minus"></i>
                      </button>
                    </div>

                    <div class="d-flex w-100 justify-content-end">
                      <button class="btn btn-primary btn-sm mt-1 ml-auto" v-on:click="addColor">
                        <i class="fas fa-plus"></i>
                      </button>
                    </div>
                  </div>
                </div>
                <div v-else class="mt-2">
                  <div class="form-group">
                    <div class="d-flex mt-2" v-for="(color,index) in theme.background.colors">
                      <input type="color" v-model="theme.background.colors[index]" class="form-control"/>
                      <button type="button" class="btn btn-primary btn-sm ml-1"
                              v-on:click="removeColor(index)"
                              v-bind:disabled="theme.background.colors.length <= 2">
                        <i class="fas fa-minus"></i>
                      </button>
                    </div>

                    <div class="d-flex w-100 justify-content-end">
                      <button class="btn btn-primary btn-sm mt-1 ml-auto" v-on:click="addColor">
                        <i class="fas fa-plus"></i>
                      </button>
                    </div>
                  </div>
                </div>
              </div>
              <div v-else>
                <div class="form-group">
                  <input type="color" class="form-control form-control-sm" v-model="theme.backgroundColor">
                </div>
              </div>

              <h5 class="card-title">Borde</h5>
              <div class="form-group">
                <label>Color</label>
                <input type="color" class="form-control form-control-sm" v-model="theme.border.color">
              </div>
              <div class="form-group">
                <label>Tipo</label>
                <select class="custom-select custom-select-sm" v-model="theme.border.type">
                  <option value="none">Ninguno</option>
                  <option value="solid">Sólido</option>
                  <option value="dotted">Punteado</option>
                  <option value="dashed">Con guiones</option>
                  <option value="ridge">Cresta</option>
                  <option value="groove">Ranura</option>
                  <option value="inset">Recuadro hacia adentro</option>
                  <option value="outset">Recuadro hacia afuera</option>
                </select>
              </div>
              <div class="form-group">
                <label>Tamaño</label>
                <div class="input-group input-group-sm">
                  <input type="number" class="form-control form-control-sm"
                         v-model.number="theme.border.size"/>
                  <div class="input-group-prepend">
                    <span class="input-group-text">px</span>
                  </div>
                </div>
              </div>
              <div class="form-group">
                <label>Radio (valor máximo recomendado 30px)</label>
                <div class="input-group input-group-sm">
                  <input type="number" class="form-control form-control-sm" max="30" min="0"
                         v-model.number="theme.border.radius">
                  <div class="input-group-prepend">
                    <span class="input-group-text">px</span>
                  </div>
                </div>

              </div>
            </div>
          </div>
        </div>
      </div>
      <div class="modal" :id="uniqueDomIdForPrivacyModal">
        <div class="modal-dialog modal-dialog-centered">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">Privacidad</h5>
              <button class="close" data-dismiss="modal">&times;</button>
            </div>
            <div class="modal-body">
              <select class="custom-select w-100 custom-select-sm" v-model="discussion.privacy"
                      title="Select privacy">
                <option :value="1">Privado</option>
                <option :value="2">Usuarios de Isolaatti</option>
                <option :value="3">Todos</option>
              </select>
            </div>
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
              <post-template :post="discussion" :theme="theme" :preview="true"></post-template>
            </div>
          </div>
        </div>
      </div>
      </section>

    `
})