Vue.component('profile-images', {
    props: {
        userId: {
            required: false,
            type: Number
        },
        isForSelect: {
            required: false,
            type: Boolean,
            default: false
        },
        squadId: {
            required: false,
            type: String,
            default: null
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
            const url = this.squadId !== null ? `/api/images/of_squad/${this.squadId}` : `/api/images/of_user/${this.userId}`
            
            this.images = await (await fetch(url, {
                method: "GET",
                headers: this.customHeaders
            })).json();
        },
        imageOnClick: function (imageId) {
            if(!this.isForSelect) {
                if(this.imagesSelected.length > 0) {
                    this.showOptions(undefined, imageId);
                } else {
                    window.location = `/api/images/image/${imageId}?mode=original`;
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
        },
        relativeUrl: function(imageId) {
            return `/api/images/image/${imageId}?mode=reduced`;
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
            :class="{'primary-border-2px':imageSelected===image.id}"
            v-for="image in images"
            @click="imageOnClick(image.id)"
            @contextmenu="showOptions($event, image.imageId)">
          <img :src="relativeUrl(image.id)" class="w-100" />
          <span class="image-selected" v-show="imagesSelected.includes(image.id)">
            <i class="fa-solid fa-check"></i>
          </span>
        </div>
      </div>
      <div v-if="isForSelect" class="d-flex mt-2 mb-0 justify-content-end position-sticky bg-white p-2" style="bottom: 0">
        <button class="btn btn-primary" :disabled="imageSelected===undefined" @click="$emit('accepted', imageSelected);">Aceptar</button>
      </div>
      </section>
    `
})