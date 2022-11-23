Vue.component('profile-info', {
    props: {
        userId: {
            required: false,
            default: null,
            type: Number
        }
    },
    data: function () {
        return {
            customHeaders: customHttpHeaders,
            userData: userData,

            profile: undefined,

            editProfileMode: false,

            newName: "",
            newDescription: "",

            saving: false,
            errorSaving: false,
            userLink: {
                isCustom: false,
                url: "",
                customId: "",
                available: true,
                error: false,
                isValid: true
            },
            following: {
                updating: false,
                error: false,
                usersFollowing: undefined,
                usersFollowers: undefined
            },
            likes: {
                showDetails: false
            },
            profileAudioMode: "new" // or "existing"
        }
    },
    computed: {
        usernameValid: function () {
            return this.newName.trim().length > 1 && this.newName.trim().length < 120;
        },
        descriptionValid: function () {
            return this.newDescription?.trim().length < 350 || this.newDescription === null;
        },
        profileImageUrl: function () {
            if (this.profile.profileImageId === null) return "/res/imgs/avatar.svg";
            return `/api/Fetch/ProfileImages/${this.profile.profileImageId}.png`;
        },
        receivedLikesPageUrl: function () {
            return `/perfil/${this.userId}/likes_recibidos`;
        },
        givenLikesPageUrl: function () {
            return `/perfil/${this.userId}/likes_dados`;
        }
    },
    methods: {
        fetchProfile: async function () {
            const response = await fetch(`/api/Fetch/UserProfile`, {
                headers: this.customHeaders,
                method: "post",
                body: JSON.stringify({id: this.userId === null ? this.userData.id : this.userId})
            });
            this.profile = await response.json();
        },
        toggleEditProfile: function () {
            this.newName = this.profile.name;
            this.newDescription = this.profile.descriptionText;
            this.editProfileMode = !this.editProfileMode;
            this.saving = false;
            this.errorSaving = false;
        },
        saveProfile: async function () {
            try {
                this.saving = true;
                const response = await fetch("/api/EditProfile/UpdateProfile", {
                    headers: this.customHeaders,
                    method: "post",
                    body: JSON.stringify({
                        newUsername: this.newName,
                        newDescription: this.newDescription
                    })
                });

                this.saving = false;
                if (response.status !== 200) {
                    this.errorSaving = true;
                    return;
                }

                const parsedResponse = await response.json();
                this.profile.username = parsedResponse.newUsername;
                this.profile.description = parsedResponse.newDescription;

                this.editProfileMode = false;
            } catch (e) {
                console.error(e);
                this.saving = false;
                this.errorSaving = true;
            }
        },
        loadProfileLink: async function () {
            const that = this;
            let response = await fetch(`/api/UserLinks/Get/${this.userData.id}`, {
                method: "GET",
                headers: this.customHeaders
            });

            if (!response.ok) {
                this.userLink.error = true
                return;
            }
            try {
                let parsedResponse = await response.json();
                this.userLink.isCustom = parsedResponse.isCustom;
                const that = this;
                if (this.userLink.isCustom) {
                    this.userLink.customId = parsedResponse.customId;
                    this.userLink.url = "https://isolaatti.com/" + this.userLink.customId;
                    new QRious({
                        element: document.getElementById('user-profile-link-qr'),
                        value: that.userLink.url
                    });
                } else {
                    this.userLink.url = parsedResponse.url;
                    await that.createCustomLink();
                }

            } catch (error) {
                this.userLink.error = true;
            }
        },
        createCustomLink: async function () {
            let response = await fetch("/api/UserLinks/Create", {
                method: "POST",
                headers: this.customHeaders
            });
            if (response.ok) {
                const that = this;
                let parsedResponse = await response.json();
                this.userLink.customId = parsedResponse.id;
                this.userLink.url = "https://isolaatti.com/" + this.userLink.customId;
                new QRious({
                    element: document.getElementById('user-profile-link-qr'),
                    value: that.userLink.url
                });
            }
        },
        modifyCustomLink: async function () {
            const that = this;
            let response = await fetch("api/UserLinks/ChangeUserLink", {
                method: "POST",
                headers: this.customHeaders,
                body: JSON.stringify({
                    data: that.userLink.customId
                })
            });
            if (response.ok) {
                this.userLink.isCustom = true;
                this.userLink.error = false;
                this.userLink.available = true;
                $('#modal-custom-user-link').modal('hide');
            } else if (response.status === 400) {
                this.userLink.error = true;
            } else if (response.status === 401) {
                this.userLink.available = false
            }
        },
        validateCustomLink: function () {
            const regex = new RegExp('^([a-zA-Z0-9 _-]+)$')
            this.userLink.isValid = regex.test(this.userLink.customId)
            if (this.userLink.isValid) {
                this.userLink.url = "https://isolaatti.com/" + this.userLink.customId;
                new QRious({
                    element: document.getElementById('user-profile-link-qr'),
                    value: this.userLink.url
                });
            } else {
                const canvas = document.getElementById('user-profile-link-qr');
                canvas.getContext('2d').clearRect(0, 0, canvas.width, canvas.height);

            }
        },
        followUser: async function () {
            this.following.updating = true;
            const response = await fetch("/api/Following/Follow", {
                headers: this.customHeaders,
                method: "post",
                body: JSON.stringify({
                    id: this.userId
                })
            });

            if (response.ok) {
                this.following.updating = false;
                this.profile.followingThisUser = true;
                this.profile.numberOfFollowers++;
            } else {
                this.following.updating = true;
                this.following.error = true;
            }
        },
        unfollowUser: async function () {
            this.following.updating = true;
            const response = await fetch("/api/Following/Unfollow", {
                headers: this.customHeaders,
                method: "post",
                body: JSON.stringify({
                    id: this.userId
                })
            });

            if (response.ok) {
                this.following.updating = false;
                this.profile.followingThisUser = false;
                this.profile.numberOfFollowers--;
            } else {
                this.following.updating = true;
                this.following.error = true;
            }
        },
        getFollowers: async function () {
            const response = await fetch(`/api/Following/FollowersOf/${this.userId}`, {
                method: "get",
                headers: this.customHeaders
            });
            this.following.usersFollowers = await response.json();
        },
        getFollowings: async function () {
            const response = await fetch(`/api/Following/FollowingsOf/${this.userId}`, {
                method: "get",
                headers: this.customHeaders
            });
            this.following.usersFollowing = await response.json();
        },
        onImageUpdated: function (imageId) {
            this.profile.profileImageId = imageId;
            $('#modal-edit-photo').modal('hide');
        },
        onAudioPosted: async function (audioId) {
            await fetch("/api/EditProfile/UpdateAudioDescription", {
                method: "post",
                headers: this.customHeaders,
                body: JSON.stringify({
                    data: audioId
                })
            });
            this.profile.profileAudioId = audioId;
            $('#modal-edit-audio').modal('hide');
        }
    },
    mounted: async function () {
        await this.fetchProfile();
        await this.loadProfileLink();
    },
    template: `
      <div>
      <aside id="left-bar" class="mt-4 p-2 isolaatti-card" v-if="profile !== undefined">

        <div class="d-flex justify-content-center align-items-center flex-column mb-3" style="position: relative">
          <img :src="profileImageUrl" width="100" height="100" class="profile-pic" id="profile_photo"
               ref="profilePhoto"/>
          <button class="btn btn-sm btn-transparent" id="change-profile-photo-btn" data-toggle="modal"
                  data-target="#modal-edit-photo" v-if="profile.isUserItself">
            <i class="fa-solid fa-pencil"></i>
          </button>
        </div>

        <div class="translucent-white-card">
          <div class="alert alert-danger" v-if="errorSaving">
            Ocurrió un error al guardar los cambios.
          </div>
          <div class="d-flex justify-content-end" v-if="profile.isUserItself">
            <button class="btn btn-transparent btn-sm" :class="{'mr-auto':editProfileMode}" @click="toggleEditProfile">
              <i class="fa-solid fa-pencil" v-if="!editProfileMode"></i>
              <i class="fa-solid fa-xmark" v-else></i>
            </button>
            <button class="btn btn-transparent btn-sm" v-if="editProfileMode" :disabled="saving" @click="saveProfile">
              <i class="fa-solid fa-floppy-disk"></i>
            </button>
          </div>

          <div v-if="!editProfileMode">
            <p class="text-center m-0 overflow-auto text-break username-profile">
              {{ profile.name }}
            </p>
            <p class="text-break text-center">
              {{ profile.descriptionText }}
            </p>
          </div>
          <div v-else>
            <div class="form-group">
              <input class="form-control" v-model="newName" placeholder="Nombre"
                     :class="{'is-invalid': !usernameValid}"/>
              <div class="invalid-feedback">
                El nombre debe tener entre 1 y 120 caracteres.
              </div>
            </div>
            <div class="form-group">
              <textarea class="form-control" v-model="newDescription" placeholder="Descripción"
                        :class="{'is-invalid': !descriptionValid}"></textarea>
              <div class="invalid-feedback">
                La descripción no puede ser de más de 350 caracteres.
              </div>
            </div>
          </div>


          <div v-if="!profile.isUserItself"
               class="d-flex justify-content-center">
            <div class="badge badge-pill badge-dark p-2"
                 v-if="profile.followingThisUser && profile.thisUserIsFollowingMe">
              Se siguen mutuamente
            </div>
            <div class="badge badge-pill badge-dark p-2"
                 v-else-if="profile.thisUserIsFollowingMe">
              Este usuario te sigue
            </div>
            <div class="badge badge-pill badge-dark p-2"
                 v-else-if="profile.followingThisUser">
              Sigues a este usuario
            </div>

          </div>
          <div v-if="!profile.isUserItself">
            <button
                class="btn btn-primary btn-sm mt-2 w-100"
                v-if="!profile.followingThisUser"
                @click="followUser"
                :disabled="following.updating">
              Seguir
            </button>
            <button
                class="btn btn-light btn-sm mt-2 w-100"
                v-else
                @click="unfollowUser"
                :disabled="following.updating">
              Dejar de seguir
            </button>
          </div>


          <nav class="d-flex justify-content-center" v-if="profile.isUserItself">
            <button class="btn btn-light btn-sm w-100 ml-1" data-target="#modal-custom-user-link"
                    data-toggle="modal" title="Enlace personalizado">
              <i class="fa-solid fa-link"></i> Enlace personalizado
            </button>
          </nav>

          <section v-if="profile.isUserItself" class="mt-3">
            <hr>
            <div v-if="profile.descriptionAudioId!==null" class="d-flex flex-column">
              <button type="button" class="btn btn-transparent btn-sm ml-auto mb-1" data-target="#modal-edit-audio"
                      data-toggle="modal">
                <i class="fa-solid fa-pencil"></i>
              </button>
              <audio-attachment :audio-id="profile.descriptionAudioId"></audio-attachment>

            </div>
            <button v-else class="btn btn-light btn-sm w-100" data-target="#modal-edit-audio" data-toggle="modal">
              <i class="fa-solid fa-microphone"></i> Configurar audio de presentación
            </button>
            <hr>
          </section>

          <div class="d-flex flex-wrap mt-2">
            <div class="btn-group btn-group-sm">
              <button class="btn btn-link"
                      data-toggle="modal" href="#modal-followers" @click="getFollowers">
                Seguidores: {{ profile.numberOfFollowers }}
              </button>
              <button class="btn btn-link"
                      data-toggle="modal" href="#modal-following" @click="getFollowings">
                Siguiendo: {{ profile.numberOfFollowing }}
              </button>
            </div>
          </div>

          <div class="card mb-2">
            <ul class="list-group list-group-flush">
              <li class="list-group-item bg-light d-flex justify-content-between align-items-center">
                <span>Likes: {{ profile.numberOfLikes }}</span>
                <button type="button" class="btn btn-transparent btn-sm"
                        @click="likes.showDetails = !likes.showDetails">
                  <i class="fa-solid fa-angle-down"></i>
                </button>
              </li>
              <li class="list-group-item" v-show="likes.showDetails">
                <div><a :href="receivedLikesPageUrl">Likes recibidos: {{ profile.numberOfLikes }} <i
                    class="fa-solid fa-arrow-up-right-from-square"></i></a></div>
                <div><a :href="givenLikesPageUrl">Likes dados: {{ profile.numberOfLikesGiven }} <i
                    class="fa-solid fa-arrow-up-right-from-square"></i></a></div>
              </li>
              <li class="list-group-item bg-light d-flex justify-content-between align-items-center">
                Discusiones: {{ profile.numberOfPosts }}
              </li>
            </ul>
          </div>
          <div style="height: 10px"></div>
        </div>
      </aside>

      <div id="modal-followers" class="modal">
        <div class="modal-dialog modal-dialog-centered modal-dialog-scrollable">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">Seguidores</h5>
              <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                <span aria-hidden="true">&times;</span>
              </button>
            </div>
            <div class="modal-body">
              <users-grid :users="following.usersFollowers" v-if="following.usersFollowers !== undefined"></users-grid>
            </div>
          </div>
        </div>
      </div>

      <div id="modal-following" class="modal">
        <div class="modal-dialog modal-dialog-centered modal-dialog-scrollable">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">Siguiendo</h5>
              <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                <span aria-hidden="true">&times;</span>
              </button>
            </div>
            <div class="modal-body">
              <users-grid :users="following.usersFollowing" v-if="following.usersFollowing !== undefined"></users-grid>
            </div>
          </div>
        </div>
      </div>

      <div class="modal" id="modal-custom-user-link">
        <div class="modal-dialog modal-dialog-scrollable modal-dialog-centered">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title"><i class="fa-solid fa-link"></i> Enlace personalizado</h5>
              <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                &times;
              </button>
            </div>
            <div class="modal-body">
              <div class="d-flex justify-content-center mb-2">
                <canvas width="140" height="140" id="user-profile-link-qr"></canvas>
              </div>
              <div class="alert alert-danger" v-if="userLink.error">
                Ocurrió un error en el servidor al tratar de obtener tu enlace.
              </div>
              <div class="alert alert-danger" v-if="!userLink.available">
                El nombre no está disponible, prueba con otro
              </div>
              <div class="alert alert-info" v-if="!userLink.isCustom">
                Acabamos de crear un enlace para ti. Puedes dejarlo así o modificarlo.
              </div>
              <div class="alert alert-danger" v-if="!userLink.isValid">
                Solo puedes usar caracteres alfanuméricos (A-Z, a-z, 0-9, _ y -)
              </div>
              <div class="input-group mb-2">
                <div class="input-group-prepend">
                  <div class="input-group-text">https://isolaatti.com/</div>
                </div>
                <input type="text" class="form-control" id="inlineFormInputGroup" v-model="userLink.customId"
                       v-on:input="validateCustomLink">
              </div>
            </div>
            <div class="modal-footer">
              <button class="btn btn-sm btn-transparent" data-dismiss="modal" aria-label="Close">
                Cancelar
              </button>
              <button class="btn btn-sm btn-primary" v-on:click="modifyCustomLink" :disabled="!userLink.isValid">
                Aceptar
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Modal to select what to edit (photo or description)-->
      <div class="modal" id="modal-edit-photo" v-if="profile?.isUserItself">
        <div class="modal-dialog modal-dialog-scrollable modal-dialog-centered">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title"><i class="far fa-edit"></i> Cambiar foto de perfil</h5>
              <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                &times;
              </button>
            </div>
            <div class="modal-body">
              <profile-image-maker @imageUpdated="onImageUpdated"></profile-image-maker>
            </div>
          </div>
        </div>
      </div>

      <div class="modal fade" id="modal-edit-audio" v-if="profile?.isUserItself">
        <div class="modal-dialog modal-dialog-scrollable modal-dialog-centered">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title"><i class="fa-solid fa-microphone"></i> Audio de presentación</h5>
              <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                &times;
              </button>
            </div>
            <div class="modal-body">
              <div class="isolaatti-card">
                <p class="mb-0">Actualmente tienes un audio colocado.</p>
                <button class="btn btn-sm btn-danger w-100" type="button">Quitar</button>
              </div>
              <div class="isolaatti-card mt-2">
                <p>Este audio aparecerá destacado en tu perfil.</p>
                <div class="w-100 d-flex">
                  <div class="ml-auto mr-auto btn-group btn-group-sm mb-1">
                    <button class="btn btn-light" type="button" @click="profileAudioMode='new'"
                            :disabled="profileAudioMode==='new'">
                      Nuevo
                    </button>
                    <button class="btn btn-light" type="button" @click="profileAudioMode='existing'"
                            :disabled="profileAudioMode==='existing'">
                      Existente
                    </button>
                  </div>
                </div>

                <audio-recorder @audio-posted="onAudioPosted" v-if="profileAudioMode==='new'"></audio-recorder>
                <audios-list :user-id="userId" v-else-if="profileAudioMode==='existing'"></audios-list>
              </div>
            </div>
          </div>
        </div>
      </div>

      </div>
    `
})