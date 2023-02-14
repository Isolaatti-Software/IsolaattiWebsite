Vue.component('new-post', {
    data: function() {
        return {
            mode: undefined, // or audio
            example: {
                template: "<p>Hola</p>"
            }
        }
    },
    mounted: function() {
        const that = this;
    },
    template: `
      <div class="max-640 mt-4 p-3 isolaatti-card" style="">
      <div class="card-body">
        <h5 class="card-title"><i class="fa-solid fa-plus"></i> Nuevo</h5>
        
        <div class="p-3 d-flex justify-content-center mt-3 w-100">
          <div class="btn-group btn-group-sm w-100">
            <button class="btn" :class="[mode==='discussion' ? 'btn-primary' : 'btn-outline-primary']"
                    v-on:click="mode='discussion'">
              <i class="far fa-newspaper"></i> Discusión
            </button>
            <button class="btn" :class="[mode==='article' ? 'btn-primary' : 'btn-outline-primary']"
                    v-on:click="mode='article'">
              <i class="fa-solid fa-pen"></i> Artículo
            </button>
            <button class="btn" :class="[mode==='audio' ? 'btn-primary' : 'btn-outline-primary']"
                    v-on:click="mode='audio'">
              <i class="fa-solid fa-ear-listen"></i> Audio
            </button>
          </div>
        </div>
        
        <div class="max-640 mt-1">
            <div class="d-flex justify-content-end mb-1" v-if="mode !== undefined">
              <button class="btn btn-light btn-sm" @click="mode=undefined">&times;</button>
            </div>
            <new-discussion v-if="mode==='discussion'"></new-discussion>
            <audio-recorder v-else-if="mode==='audio'"></audio-recorder>
            <div v-else-if="mode==='article'">Pronto podrás escribir articulos</div>
        </div>
      </div>
      </div>
    `
})