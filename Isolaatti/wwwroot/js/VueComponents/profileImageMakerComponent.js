

Vue.component('profile-image-maker', {
    props: {
        // Pass this property in case you want to show squad images instead user's images
        squadId: {
            required: false,
            default: null
        },
        profile: {
            required: true,
            default: false
        }
    },
    data: function () {
        return {
            customHeaders: customHttpHeaders,
            imageFile: null,
            uploading: false,
            ableToUpload: false,
            description: `Mi imagen de ${new Date().toLocaleString()}`,
            imageUrlData: undefined,
            userData: userData,
            mode: "upload" // or existing
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
                    that.$emit('imageUpdated', JSON.parse(request.responseText).id);
                }
            }
            request.send(formData);
        },
        updateImage: async function(imageId) {
            let url = "";
            if(this.squadId === null){
                url =  `/api/EditProfile/SetProfilePhoto?imageId=${imageId}`;
            } else {
                url = `/api/images/set_image_of_squad/${this.squadId}?imageId=${imageId}`;
            }
            const response = await fetch(url, {method: "post", headers: this.customHeaders});
            if(response.ok)
                this.$emit('imageUpdated',imageId)
        }
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
    <div v-if="mode==='upload'">
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
          <button type="button" class="btn btn-primary" @click="uploadImage" :disabled="!ableToUpload || uploading">Subir</button>
        </div>
      </template>
      
    </div>
    <profile-images v-else-if="mode==='existing'" 
                    :user-id="userData.id" 
                    :is-for-select="true"
                    :squad-id="squadId"
                    @accepted="updateImage"></profile-images>
    </div>
    `
})