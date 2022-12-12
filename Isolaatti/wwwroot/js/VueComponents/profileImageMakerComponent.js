

Vue.component('profile-image-maker', {
    props: {
        // Pass this property in case you want to show squad images instead user's images
        squadId: {
            required: false,
            default: null
        }
    },
    data: function () {
        return {
            customHeaders: customHttpHeaders,
            filename: "Elegir archivo...",
            imageForPreview: new Image(),
            canvasContext: undefined,
            loadingComponent: true,
            uploading: false,
            ableToUpload: false,
            
            userData: userData,
            mode: "upload" // or existing
        }
    },
    methods: {
        onFileLoaded: function (event) {
            const file = event.target.files[0];
            this.filename = file.name;
            const that = this;
            const fileReader = new FileReader();
            fileReader.readAsDataURL(file);
            fileReader.onload = function () {
                that.imageForPreview.src = fileReader.result;
            }
        },
        uploadImage: async function () {
            this.uploading = true;
            const that = this;
            document.getElementById("previewProfilePicture").toBlob(async function (blob) {

                const formData = new FormData();
                formData.append("file", blob);
                const request = new XMLHttpRequest()
                request.open("POST", "/api/EditProfile/UpdatePhoto");
                request.setRequestHeader("sessionToken", that.customHeaders.get("sessionToken"))
                request.onload = function () {
                    if (request.status === 200) {
                        that.uploading = false;
                        that.$emit('imageUpdated', JSON.parse(request.responseText));
                    }
                }
                request.send(formData);

            });
        },
        updateImage: async function(imageId) {
            const url = `/api/EditProfile/SetProfilePhoto?imageId=${imageId}`;
            const response = await fetch(url, {method: "post", headers: this.customHeaders});
            if(response.ok)
                this.$emit('imageUpdated',imageId)
        }
    },
    mounted: function () {
        this.canvasContext = document.getElementById("previewProfilePicture").getContext("2d");
        this.loadingComponent = false;
        const that = this;
        this.imageForPreview.addEventListener("load", function() {
            that.ableToUpload = true;

            // horizontal
            if (that.imageForPreview.width > that.imageForPreview.height) {
                let constant = that.imageForPreview.height / 120;
                let margin = (120 - (that.imageForPreview.width / constant)) / 2;
                that.canvasContext.drawImage(that.imageForPreview, margin, 0, that.imageForPreview.width / constant, that.imageForPreview.height / constant);
            }

            // vertical
            else if (that.imageForPreview.width < that.imageForPreview.height) {
                let constant = that.imageForPreview.width / 120;
                let margin = (120 - (that.imageForPreview.height / constant)) / 2;
                that.canvasContext.drawImage(that.imageForPreview, 0, margin, that.imageForPreview.width / constant, that.imageForPreview.height / constant);
            } else {
                that.canvasContext.drawImage(that.imageForPreview, 0, 0, 120, 120);
            }
        })
    },
    template: `
    <div>
    <div class="btn-group w-100">
        <button class="btn" 
            :class="[mode==='upload' ? 'btn-primary' : 'btn-light']"
            @click="mode='upload'">
            Subir
        </button>
        <button class="btn" 
            :class="[mode==='existing' ? 'btn-primary' : 'btn-light']"
            @click="mode='existing'">
            Existente
        </button>
    </div>
    <div v-show="mode==='upload'">
        <section v-show="loadingComponent">
            Cargando...
        </section>
        <section v-show="!loadingComponent">
          <div class="d-flex justify-content-center mb-2">
            <canvas width="120" height="120" id="previewProfilePicture"></canvas>
          </div>
          <div class="custom-file">
            <input type="file" class="custom-file-input" accept="image/*" @input="onFileLoaded" id="profilePictureLoadFormElement">
            <label class="custom-file-label text-ellipsis" for="profilePictureLoadFormElement">{{filename}}</label>
          </div>
          <div class="d-flex justify-content-end mt-2">
            <span v-if="uploading" class="mr-auto">Subiendo...</span>
            <button type="button" class="btn btn-primary" @click="uploadImage" :disabled="!ableToUpload || uploading">Subir</button>
          </div>
        </section>
    </div>
    <profile-images v-show="mode==='existing'" 
                    :user-id="userData.id" 
                    :is-for-select="true"
                    @accepted="updateImage"></profile-images>
    </div>
    `
})