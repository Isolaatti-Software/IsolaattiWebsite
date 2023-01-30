﻿const yourSquadsComponent = {
    data: function() {
        return {
            customHeaders: customHttpHeaders,
            loading: true,
            mySquads: []
        }
    },
    methods: {
        fetchMySquads: async function() {
            const response = await fetch("/api/Squads/MySquads", {
                headers: this.customHeaders
            });
            this.loading = false;
            this.mySquads = await response.json();
        },
        squadUrl: function(squadId) {
            return `/squads/${squadId}`;
        }
    },
    mounted: async function() {
        await this.fetchMySquads();
    },
    template: `
    <section class="isolaatti-card">
      <h5>Tus Squads</h5>
      <p>Squads donde tú eres el administrador.</p>
      <div class="alert alert-info" v-if="mySquads.length === 0 && !loading">No eres administrador de ningún squad.</div>
      <div class="d-flex justify-content-center w-100">
        <div class="spinner-border" v-if="loading"></div>
      </div>
      <div class="list-group list-group-flush">
        <a :href="squadUrl(squad.id)" class="list-group-item list-group-item-action" v-for="squad in mySquads">
          {{squad.name}}<br>
          <small>id: {{squad.id}}</small>
        </a>
      </div>
    </section>
    `
}

Vue.component('your-squads', yourSquadsComponent);