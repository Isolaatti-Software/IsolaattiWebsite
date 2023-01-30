const squadPeopleComponent = {
    props:{
        squadId: {
            required: true,
            type: String
        }
    },
    data: function() {
        return {
            currentScreen: "members" // or "requests" or "members" or "invitations"
        }
    },
    watch: {
        $route: {
            immediate: true,
            deep: true,
            handler: function(to, from) {
                this.currentScreen = to.query.opcion
            }
        }
    },
    methods: {
        onOptionSelected: function (e) {
            switch (e.target.value) {
                case "0": this.$router.push({ path: '/miembros', query: { opcion: 'members' } }); break;
                case "1": this.$router.push({ path: '/miembros', query: { opcion: 'invitations' } }); break;
                case "2": this.$router.push({ path: '/miembros', query: { opcion: 'requests' } }); break;
            }
        }
    },
    template: `
    <div>
      <section>
        <select class="custom-select" @input="onOptionSelected">
            <option selected value="0">Miembros</option>
            <option value="1">Invitaciones</option>
            <option value="2">Solicitudes</option>
        </select>
      </section>
      <div class="mt-2">
        <squad-invitations :squad-id="squadId" v-if="currentScreen==='invitations'"></squad-invitations>
        <squad-requests :squad-id="squadId" v-if="currentScreen==='requests'"></squad-requests>
        <squad-members :squad-id="squadId" v-if="currentScreen==='members'"></squad-members>
      </div>
    </div>
    `
}

Vue.component('squad-people', squadPeopleComponent);