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
            imageSelected: undefined,
            imagesSelected: []
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
                if(this.imagesSelected.length > 0) {
                    this.showOptions(undefined, imageId);
                } else {
                    window.location = `/imagen/${imageId}`;
                }
                
            } else {
                this.imageSelected = imageId;
            }
        },
        showOptions: function(e, imageId) {
            if(e !== undefined) {
                e.preventDefault();
            }
            console.log(imageId);
            if(this.imagesSelected.includes(imageId)) {
                const index = this.imagesSelected.indexOf(imageId);
                this.imagesSelected.splice(index, 1);
            } else {
                this.imagesSelected.push(imageId);
            }
        }
    },
    mounted: async function () {
        await this.getImages();
    },
    template: `
      <section>
      <div class="d-flex mt-2 justify-content-end" v-if="imagesSelected.length > 0">
        <span class="mr-auto">{{imagesSelected.length}} imagenes seleccionadas</span>
        <button class="btn btn-light"><i class="fa-solid fa-trash"></i></button>
      </div>
      <p class="m-2 text-center" v-if="images.length === 0"> No hay contenido que mostrar <i
          class="fa-solid fa-face-sad-cry"></i></p>
      <div class="grid-3-columns mt-2">
        <div class="position-relative d-flex justify-content-center w-100 hover-image-container" 
            :class="{'primary-border-2px':imageSelected===image.imageId}"
            v-for="image in images"
            @click="imageOnClick(image.imageId)"
            @contextmenu="showOptions($event, image.imageId)">
          <img :src="image.relativeUrl" class="w-100"/>
          <span class="image-selected" v-show="imagesSelected.includes(image.imageId)">
            <i class="fa-solid fa-check"></i>
          </span>
        </div>
      </div>
      <div v-if="isForSelect" class="d-flex mt-2 justify-content-end">
        <button class="btn btn-primary" :disabled="imageSelected===undefined">Aceptar</button>
      </div>
      </section>
    `
})