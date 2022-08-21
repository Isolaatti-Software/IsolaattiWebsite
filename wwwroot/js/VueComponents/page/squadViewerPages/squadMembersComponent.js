const squadMembersComponent = {
    props:{
        squadId: {
            required: true,
            type: String
        }
    },
    data: function() {
        return {
            customHeaders: customHttpHeaders,
            members: []
        }
    },
    methods: {
        fetchMembers: async function() {
            const response = await fetch(`/api/Squads/${this.squadId}/Members`, {
                headers: this.customHeaders
            });
            this.members = await response.json();
        }  
    },
    mounted: async function() {
        await this.fetchMembers();
    },
    template: `
    <section>
      <h5>Invitaciones</h5>
      <users-grid :users="members"></users-grid>
      <h5>Solicitudes</h5>
      <users-grid :users="members"></users-grid>
      <h5>Administrador</h5>
      <h5>Miembros</h5>
      <users-grid :users="members"></users-grid>
    </section>
    `
}

Vue.component('squad-members', squadMembersComponent);