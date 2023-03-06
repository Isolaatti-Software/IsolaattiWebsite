const squadsBelong = {
    data: function() {
        return {
            customHeaders: customHttpHeaders,
            loading: true,
            mySquads: []
        }
    },
    methods: {
        fetchMySquads: async function() {
            const response = await fetch("/api/Squads/SquadsBelong", {
                headers: this.customHeaders
            });
            this.loading = false;
            this.mySquads = await response.json();
        },
        squadUrl: function(squadId) {
            return `/squads/${squadId}`;
        },
        imageUrl: function(imageId) {
            return imageId ? `/api/images/image/${imageId}?mode=reduced` : "/res/imgs/groups.png";
        }
    },
    mounted: async function() {
        await this.fetchMySquads();
    },
    template: `
    <section class="isolaatti-card">
        <h5>Squads donde eres miembro</h5>
        <p>Squads donde eres miembro, no administrador.</p>
        <div class="alert alert-info" v-if="mySquads.length === 0 && !loading">No eres miembro de ningún squad.</div>
        <div class="d-flex justify-content-center w-100">
          <div class="spinner-border" v-if="loading"></div>
        </div>
        <div class="list-group list-group-flush">
          <a :href="squadUrl(squad.id)" class="list-group-item list-group-item-action" v-for="squad in mySquads">
            <img :src="imageUrl(squad.imageId)" class="user-avatar"/>
            {{squad.name}}<br>
            <small>id: {{squad.id}}</small>
          </a>
        </div>
    </section>
    `
}

Vue.component('squads-belong', squadsBelong);