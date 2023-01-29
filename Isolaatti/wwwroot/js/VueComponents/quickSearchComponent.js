Vue.component("quick-search", {
    data: function () {
        return {
            customHeaders: customHttpHeaders,
            query: "",
            loading: false,
            result: undefined,
            resultIsVisible: false
        }
    },
    methods: {
        onInputFocus: function(e) {
            this.resultIsVisible = true;
        },
        search: _.debounce(async function () {
            this.result = undefined;
            this.loading = true
            const response = await fetch(`/api/Search/Quick?q=${encodeURIComponent(this.query)}`, {
                headers: this.customHeaders
            });
            this.result = await response.json();
            this.loading = false;
        }, 300),
        link: function (result) {
            switch (result.resultType) {
                case 0: // post
                    return `/pub/${result.resourceId}`;
                case 1: // user
                    return `/perfil/${result.resourceId}`;
                case 4: // image
                    return `/imagen/${result.resourceId}`;
            }
        },
        userProfileLink: function(userId) {
            return `/perfil/${userId}`
        },
        compileMarkdown: function(rawMarkdown) {
            return DOMPurify.sanitize(marked.parse(rawMarkdown));
        },
        squadUrl: function(squadId) {
            return `/squads/${squadId}`;
        }
    },
    mounted: function() {
        const that = this;
        events.$on('backdrop-clicked', function() {
            that.resultIsVisible = false;
        })
    },
    template: `
      <section class="search-box">
      <div class="d-flex justify-content-center">
        <input class="form-control" 
            :class="resultIsVisible ? 'w-100' : 'search-box-input'" 
            @focus="onInputFocus" 
            placeholder="Búsqueda rápida" 
            @input="search" 
            v-model="query"/>
      </div>
      <div class="search-box-results" :class="resultIsVisible ? '' : 'd-none'">
      <div v-if="query === ''" class="m-3">
            <p class="text-center">Comienza a escribir para realizar una búsqueda en toda la plataforma...</p>
        </div>
        <div v-else class="p-2">
            <div v-if="result !== undefined">
                <div v-if="result.profiles.length>0">
                    <h5 class="m-2">Perfiles</h5>
                    <users-grid :users="result.profiles"></users-grid>
                    <hr/>
                </div>
                <div v-if="result.squads.length>0">
                    <h5 class="m-2">Squads</h5>
                    <div class="list-group list-group-flush">
                        <a :href="squadUrl(squad.id)" class="list-group-item list-group-item-action" v-for="squad in result.squads">
                          {{squad.name}}<br>
                          <small>id: {{squad.id}}</small>
                        </a>
                    </div>
                    <hr/>
                </div>
                <div v-if="result.audios.length>0">
                    <h5>Audios</h5>
                </div>
                
                <post-template v-for="post in result.posts" :post="post"
                     v-bind:preview="false"
                     v-bind:key="post.post.id"></post-template>
            </div>
            <div v-if="loading" class="w-100 d-flex h-100 justify-content-center align-items-center">
                <div class="spinner-border" role="status">
                    <span class="sr-only">Cargando...</span>
                </div>
            </div>
            
        </div>
</div>
      </section>
    `
});