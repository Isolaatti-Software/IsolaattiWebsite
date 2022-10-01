Vue.component('profile-images', {
    props: {
        userId: {
            required: true,
            type: Number
        }
    },
    data: function () {
        return {
            customHeaders: customHttpHeaders,
            images: []
        }
    },
    methods: {
        getImages: async function () {
            this.images = await (await fetch(`/api/Fetch/ProfileImages/OfUser/${this.userId}`, {
                method: "GET",
                headers: this.customHeaders
            })).json();
        },
        openImageInNewTab: function (imageId) {
            window.location = `/imagen/${imageId}`
        }
    },
    mounted: async function () {
        await this.getImages();
    },
    template: `
      <section>
      <h5 class="mt-2 text-center"><i class="fas fa-camera" aria-hidden="true"></i> Fotos de perfil</h5>
      <p class="m-2 text-center" v-if="images.length === 0"> No hay contenido que mostrar <i
          class="fa-solid fa-face-sad-cry"></i></p>
      <div class="grid-3-columns mt-2">
        <div class="d-flex justify-content-center w-100 hover-image-container" v-for="image in images"
             @click="openImageInNewTab(image.imageId)">
          <img :src="image.relativeUrl" class="w-100"/>
        </div>
      </div>
      </section>
    `
})