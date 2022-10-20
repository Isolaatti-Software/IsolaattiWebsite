Vue.component('profile-images', {
    props: {
        userId: {
            required: true,
            type: Number
        },
        isForSelect: {
            required: false,
            type: Boolean,
            default: false
        }
    },
    data: function () {
        return {
            customHeaders: customHttpHeaders,
            images: [],
            imageSelected: undefined
        }
    },
    methods: {
        getImages: async function () {
            this.images = await (await fetch(`/api/Fetch/ProfileImages/OfUser/${this.userId}`, {
                method: "GET",
                headers: this.customHeaders
            })).json();
        },
        imageOnClick: function (imageId) {
            if(!this.isForSelect) {
                window.location = `/imagen/${imageId}`
            } else {
                this.imageSelected = imageId;
            }
        },
        showOptions: function(e) {
            e.preventDefault();
            console.log("Opciones");
        }
    },
    mounted: async function () {
        await this.getImages();
    },
    template: `
      <section>
      <p class="m-2 text-center" v-if="images.length === 0"> No hay contenido que mostrar <i
          class="fa-solid fa-face-sad-cry"></i></p>
      <div class="grid-3-columns mt-2">
        <div class="d-flex justify-content-center w-100 hover-image-container" 
            :class="{'primary-border-2px':imageSelected===image.imageId}"
            v-for="image in images"
            @click="imageOnClick(image.imageId)"
            @contextmenu="showOptions">
          <img :src="image.relativeUrl" class="w-100"/>
        </div>
      </div>
      <div v-if="isForSelect" class="d-flex mt-2 justify-content-end">
        <button class="btn btn-primary" :disabled="imageSelected===undefined">Aceptar</button>
      </div>
      </section>
    `
})