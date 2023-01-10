Vue.component("quick-search", {
    data: function () {
        return {
            customHeaders: customHttpHeaders,
            query: "",
            results: []
        }
    },
    methods: {
        search: _.debounce(async function () {
            const response = await fetch(`/api/Search/Quick?q=${encodeURIComponent(this.query)}`, {
                headers: this.customHeaders
            });
            this.results = await response.json();
        }, 300),
        link: function (result) {
            switch (result.resultType) {
                case 0: // post
                    return `/pub/${result.resourceId}`;
                case 1: // user
                    return `/perfil/${result.resourceId}`;
            }
        }
    },
    template: `
      <section>
      <div class="d-flex">
        <button class="btn btn-light" data-toggle="modal" data-target="#search-modal">&times;</button>
        <input class="form-control" placeholder="Búsqueda rápida" @input="search" v-model="query"/>
      </div>
      <div class="list-group list-group-flush mt-3">
        <a class="list-group-item list-group-item-action" :href="link(result)" v-for="result in results">
          {{ result.contentPreview }}
        </a>
      </div>
      </section>
    `
});