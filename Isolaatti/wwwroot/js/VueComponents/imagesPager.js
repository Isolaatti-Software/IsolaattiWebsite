Vue.component('images-pager', {
    props: {
        imagesUrls: {
            type: Array,
            required: true
        }
    },
    data: function() {
        return {
            currentIndex: 0
        }
    },
    computed: {
        showPrev: function() {
            const prevIndex = this.currentIndex - 1
            return prevIndex >= 0 && prevIndex < this.imagesUrls.length;
        },
        showNext: function() {
            const nextIndex = this.currentIndex + 1;
            return nextIndex >= 0 && nextIndex < this.imagesUrls.length;
        }
        
    },
    methods: {
        nextPage: function() {
            this.currentIndex++;
        },
        prevPage: function() {
            this.currentIndex--;
        }
    },
    template: `
    <div class="image-pager">
      <div class="position-absolute w-100 h-100 d-flex justify-content-between m-0 align-items-center">
        <button class="btn btn-light ml-1" :disabled="!showPrev" @click="prevPage"><i class="fa-solid fa-chevron-left"></i></button>
        <button class="btn btn-light mr-1" :disabled="!showNext" @click="nextPage"><i class="fa-solid fa-chevron-right"></i></button>
      </div>
      <div class="w-100 d-flex flex-nowrap overflow-hidden">
        <img :src="imagesUrls[currentIndex]"  />
      </div>
    </div>
    `
})