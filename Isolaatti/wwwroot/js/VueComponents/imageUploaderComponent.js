Vue.component('image-uploader',{
    props: {
        squadId: {
            required: false,
            default: null
        },
        profile: {
            required: true,
            default: false
        }  
    },
    data: function() {
        return {
            customHeaders: customHttpHeaders,
            imageUrlData: undefined,
            uploading: false,
            imageFile: null,
            ableToUpload: false,
            description: `Mi imagen de ${new Date().toLocaleString()}`
        }
    },
    computed: {
        filename: function() {
            if(this.imageFile !== null) {
                return this.imageFile.name;
            }
            return "Elegir archivo..."
        }
    },
    methods: {
        onFileLoaded: function (event) {
            this.imageFile = event.target.files[0];
            this.ableToUpload = true;

            const fileReader = new FileReader();

            fileReader.readAsDataURL(this.imageFile);
            const that = this;
            fileReader.onload = function() {
                that.imageUrlData = fileReader.result;
            }
        },
        uploadImage: async function () {
            this.uploading = true;
            const that = this;

            const formData = new FormData();


            formData.append("file", this.imageFile);
            formData.append("name", this.description);
            if(this.squadId != null) {
                formData.append("squadId", this.squadId);
            }
            const request = new XMLHttpRequest()
            const url = this.profile ? "/api/images/create?setAsProfile=True" : "/api/images/create"
            request.open("POST", url);
            request.setRequestHeader("sessionToken", that.customHeaders.get("sessionToken"))
            request.onload = function () {
                if (request.status === 200) {
                    that.uploading = false;
                    that.$emit('uploaded', JSON.parse(request.responseText));
                }
            }
            request.send(formData);
        },
    },
    template: `
        <div>
        <div class="custom-file mt-3">
          <input type="file" class="custom-file-input" accept="image/*" @input="onFileLoaded" id="profilePictureLoadFormElement">
          <label class="custom-file-label text-ellipsis" for="profilePictureLoadFormElement">{{filename}}</label>
        </div>
        <template v-if="imageUrlData !== undefined">
          <div v-if="imageUrlData !== undefined" class="d-flex justify-content-center mt-2">
            <img :src="imageUrlData" height="240" alt="Vista previa de fotografia"/>
          </div>
          <div class="form-group mt-2">
            <label for="image-description">Descripción de la foto</label>
            <textarea id="image-description" class="form-control" v-model="description" :disabled="!ableToUpload"></textarea>
          </div>
          <div class="d-flex justify-content-end mt-2">
            <span v-if="uploading" class="mr-auto">Subiendo...</span>
            <button type="button" class="btn btn-primary" @click="uploadImage" :disabled="!ableToUpload || uploading">Agregar imagen</button>
          </div>
        </template>
        </div>
    `
})