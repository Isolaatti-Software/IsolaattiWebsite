Vue.component("audio-attachment", {
    props: {
        audioObject: {
            type: Object,
            required: false,
            default: null
        },
        audioId: {
            type: String,
            required: false
        },
        canRemove: {
            type: Boolean,
            default: false,
            required: false
        }
    },
    data: function () {
        return {
            name: "",
            userName: "",
            userId: -1,
            audioNotFound: false,
            customHeaders: customHttpHeaders,
            userData: userData,
            playbackStatus: {
                selected: false,
                playing: false,
                paused: false
            },
            showDialogBackground: false,
            changingName: false,
            deleting: false,
            newName: "",
            performingDeletion: false,
            errorDeleting: false,
            performingNameChange: false,
            trackingRequestSent: false
        }
    },
    methods: {
        sendTrackingRequest: async function() {
            const response = await fetch(`/tracking/register?audioId=${this.audioObject.id}`,{
                headers: this.customHeaders,
                method: "GET"
            });
            this.trackingRequestSent = response.ok;
        },
        play: async function () {
            await audioService.playPause(this.id);
            if(!this.trackingRequestSent) {
                await this.sendTrackingRequest();
            }
        },
        showChangeNameDialog: function() {
            this.changingName = true;
            this.newName = this.name;
        },
        hideChangeNameDialog: function() {
            this.changingName = false;
            this.newName = "";
        },
        showDeleteAudioDialog: function() {
            this.showDialogBackground = true;
            this.deleting = true;
        },
        hideDeleteAudioDialog: function() {
            this.showDialogBackground = false;
            this.deleting = false;
        },
        changeAudioName: async function() {
            this.performingNameChange = true
            const response = await fetch(`/api/Audios/${this.id}/Rename`, {
                method: "post",
                headers: this.customHeaders,
                body: JSON.stringify({
                    data: this.newName
                })
            });
            this.performingNameChange = false
            if(response.ok) {
                this.name = this.newName;
                
            }
            this.hideChangeNameDialog();
            
            
            
            this.$emit("nameChanged", this.id);
            this.hideChangeNameDialog();
        },
        deleteAudio: async function() {
            this.performingDeletion = true;
            const response = await fetch(`/api/Audios/${this.id}/Delete`,{
                method: "post",
                headers: this.customHeaders
            });
            if(response.ok){
                this.$emit("audioDeleted", this.id);
            } else {
                this.performingDeletion = false;
                
            }
            
            this.deleting = false;
        }
    },
    computed: {
        profileRelativeUrl: function () {
            return `/perfil/${this.userId}`;
        },
        id: function(){
            if(this.audioId === undefined) {
                return this.audioObject.id;
            }
            return this.audioId;
        },
        newNameIsInvalid: function() {
            return this.newName.length < 1;
        }
    },
    mounted: function () {
        const that = this;
        
        this.$nextTick(function () {
            // Only feteches data when the data is not passed as property. Data is usually passed when
            // this component is in a list, but not passed when it is on a discussion
            if(that.audioObject === null) {
                fetch(`/api/Audios/${that.id}`, {
                    method: "GET",
                    headers: customHttpHeaders
                }).then(res => {
                    if(res.ok){
                        res.json().then(audioMetadata => {
                            that.name = audioMetadata.name;
                            that.userName = audioMetadata.userName;
                            that.userId = audioMetadata.userId;
                        })
                    } else {
                        that.audioNotFound = true;
                    }
                })
            } else {
                that.name = that.audioObject.name;
                that.userName = that.audioObject.userName;
                that.userId = that.audioObject.userId
            }
        })

        // I need to listen to these events to update UI when another 
        // audio on the whole system starts playing
        events.$on("audios.state", function (state, id) {
            if(state === "ended" && id === that.id) {
                that.playbackStatus.playing = false;
                that.playbackStatus.paused = false;
            }
            
            if(state === "playing") {
                if(id === that.id) {
                    that.playbackStatus.selected = true;
                    that.playbackStatus.playing = true;
                    that.playbackStatus.paused = false;
                } else {
                    that.playbackStatus.selected = false;
                    that.playbackStatus.playing = false;
                    that.playbackStatus.paused = false;
                }
            }
            
            if(state === "paused") {
                if(id === that.id) {
                    that.playbackStatus.selected = true;
                    that.playbackStatus.playing = false;
                    that.playbackStatus.paused = true;
                } else {
                    that.playbackStatus.selected = false;
                    that.playbackStatus.playing = false;
                    that.playbackStatus.paused = false;
                }
            }
        });
    },
    template: `
        <div :class="{'item-in-place-dialog-background': showDialogBackground }">
            <section v-if="deleting" class="w-100">
                <div v-if="errorDeleting" class="mt-2 d-flex justify-content-end align-items-center p-1">
                    <span >Ocurrió un error al eliminar el audio.</span>
                    <button class="btn btn-light">OK</button>
                </div>
                <div v-else class="mt-2 d-flex justify-content-end align-items-center p-1">
                    <span class="mr-auto" >¿Eliminar este audio?</span>
                
                    <button class="btn btn-light mr-1" @click="hideDeleteAudioDialog" :disabled="performingDeletion">No</button>
                    <button class="btn btn-primary" @click="deleteAudio" :disabled="performingDeletion">Sí</button>
                </div>
                
                
            </section>
            <div class="audio-attachment-component">
            <div class="btn-group-sm" v-if="playbackStatus.url !== null && !audioNotFound">
        
                <button type="button" class="btn btn-light btn-sm" v-on:click="play" v-if="playbackStatus.playing">
                    <i class="fas fa-pause"></i>
                </button>
                <button type="button" class="btn btn-light btn-sm" v-on:click="play" v-else>
                    <i class="fas fa-play"></i>
                </button>
            </div>
            <div class="d-flex flex-column overflow-hidden w-100 align-items-start">
                <div class="d-flex w-100" v-if="changingName">
                    <input type="text" class="form-control w-100" v-model="newName"/>
                    <button class="btn btn-primary ml-1" @click="changeAudioName" :disabled="newNameIsInvalid || performingNameChange"><i class="fa-solid fa-floppy-disk"></i></button>
                    <button class="btn" @click="hideChangeNameDialog"><i class="fa-solid fa-xmark"></i></button>
                </div>
                
                <div v-else>
                    <p v-if="audioNotFound">Audio eliminado</p>
                    <p v-if="!audioNotFound" class="m-0 text-ellipsis btn-link btn p-0" :class="{'text-primary font-weight-bolder':playbackStatus.selected, 'text-black-50':!playbackStatus.selected}" data-dismiss="modal" v-on:click="$emit('select')">{{name}}</p>
                    <p v-if="!audioNotFound" class="text-black-50 m-0"><a :href="profileRelativeUrl">{{userName}}</a></p>
                </div>
            </div>
            <button type="button" class="close ml-auto" v-if="canRemove" v-on:click="$emit('remove')">&times;</button>
      
            <div class="dropdown ml-auto" v-else-if="!audioNotFound">
                <button type="button" class="btn" data-toggle="dropdown" aria-haspopup="true"><i class="fas fa-ellipsis-h" aria-hidden="true"></i></button>
                <div class="dropdown-menu">
                    <a href="#" class="dropdown-item" @click="showChangeNameDialog" v-if="userData.id === userId">Cambiar nombre</a>
                    <a href="#" class="dropdown-item" @click="showDeleteAudioDialog" v-if="userData.id === userId">Eliminar</a>
                    <a href="#" class="dropdown-item">Reportar</a>
                </div>
            </div>
            </div>
        </div>
      
    `
})