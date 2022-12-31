

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
            userData: userData,
            mode: "upload" // or existing
        }
    },
    methods: {
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
        },
        onUploaded: function(image) {
            this.$emit("imageUpdated", image.id)
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
    <image-uploader v-if="mode==='upload'" :profile="profile" :squad-id="squadId" @uploaded="onUploaded"/>
    <profile-images v-else-if="mode==='existing'" 
                    :user-id="userData.id" 
                    :is-for-select="true"
                    :squad-id="squadId"
                    @accepted="updateImage"/>
    </div>
    `
})