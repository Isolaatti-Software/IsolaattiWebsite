Vue.component('squad-members', {
    props: {
        squadId: {
            type: String,
            required: true
        }
    },
    data: function() {
        return {
            customHeaders: customHttpHeaders,
            members: [],
            admins: []
        }
    },
    methods: {
        fetchMembers: async function() {
            const response = await fetch(`/api/Squads/${this.squadId}/Members`,{
                method: "GET",
                headers: this.customHeaders
            });
            const parsedResponse = await response.json();
            this.members = parsedResponse.members;
            this.admins = [parsedResponse.admin];
        },
        pictureUrl: function(picId) {
            return `/api/Fetch/ProfileImages/${picId}.png`;
        }
    },
    mounted: async function() {
        await this.fetchMembers();
    },
    template: `
    <section class="isolaatti-card">
      <h5>Administrador</h5>
      <users-grid :users="admins"></users-grid>
      <h5>Miembros</h5>
      <users-grid :users="members"></users-grid>
    </section>
    `
});
