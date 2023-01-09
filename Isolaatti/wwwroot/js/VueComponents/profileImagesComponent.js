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
        },
        userIself: {
          required: false,
          type: Boolean,
          default: true
        }
    },
    data: function () {
        return {
            customHeaders: customHttpHeaders,
            images: [],
            imageSelected: undefined,
            imagesSelected: [],
            imageOnViewer: undefined
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
        imageOnClick: function (image) {
            if(!this.isForSelect) {
                if(this.imagesSelected.length > 0) {
                    this.showOptions(undefined, image.id);
                } else {
                    this.imageOnViewer = image;
                    $("#modal-image-viewer").modal("show");
                }
                
            } else {
                this.imageSelected = image.id;
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
        relativeUrl: function(imageId, mode="reduced") {
            return `/api/images/image/${imageId}?mode=${mode}`;
        },
        onUploaded: function(image) {
            this.images.push(image);
            $('#modal-upload-photo').modal('hide');
        }
    },
    mounted: async function () {
        await this.getImages();
    },
    template: `
      <section>
      <div class="d-flex justify-content-end" v-if="userIself">
        <button class="btn btn-sm btn-light" title="Nueva imagen" data-target="#modal-upload-photo" data-toggle="modal">
          <i class="fa-solid fa-plus"></i>
        </button>
      </div>
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
            @click="imageOnClick(image)"
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

      <div class="modal" id="modal-upload-photo">
        <div class="modal-dialog modal-dialog-scrollable modal-dialog-centered">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title"><i class="fa-solid fa-image"></i> Subir imagen</h5>
              <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                &times;
              </button>
            </div>
            <div class="modal-body">
              <image-uploader :profile="false" :squad-id="squadId" @uploaded="onUploaded"/>
            </div>
          </div>
        </div>
      </div>
      
      <div class="modal" id="modal-image-viewer">
        <div class="modal-dialog modal-dialog-scrollable modal-dialog-centered modal-xl">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title"><i class="fa-solid fa-image"></i> Imagen</h5>
              <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                &times;
              </button>
            </div>
            <div class="modal-body">
              <div class="container-fluid h-100" v-if="imageOnViewer !== undefined">
                <div class="row">
                  <div class="col-md-6">
                    <img :src="relativeUrl(imageOnViewer.id, 'original')" class="h-100 w-100"/>
                  </div>
                  <div class="col-md-6">
                    <p>{{imageOnViewer.name}}</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      </section>
    `
})