Vue.component("squad-members-page", {
    props: {
        squadId: {
            type: String,
            required: true
        }
    },
    data: function() {
        return {
            customHeaders: customHttpHeaders,
            ready: false,
            selectedUser: undefined,
            squadData: undefined,
            members: [],
            initialState: undefined,
            moreMembers: false
        }
    },
    methods: {
        loadDetails: function(user) {
            this.selectedUser = user;
        },
        profileImageLink: function(imageId) {
            if(imageId === null || imageId === "") {
                return "/res/imgs/avatar.svg";
            }
            return `/api/images/image/${imageId}?mode=reduced`
        },
        userProfileLink: function(userId) {
            return `/perfil/${userId}`
        },
        fetchMembers: async function(refresh) {
            let url = `/api/Squads/${this.squadId}/Members`;
            if(refresh) {
                this.members = [];
            }
            if(this.members.length >= 20) {
                url += `?lastId=${this.members[this.members.length - 1].id}`
            }
            const response = await fetch(url,{
                method: "get",
                headers: this.customHeaders
            });

            if(response.ok){
                const incomingMembers = await response.json();
                this.members = this.members.concat(incomingMembers)
                this.moreMembers = incomingMembers.length === 20;
            }
        },
        fetchState: async function() {
            const response = await fetch(`/api/Squads/${this.squadId}/MyState`, {
                method: "get",
                headers: this.customHeaders
            });
            if(response.ok) {
                this.initialState = await response.json();
            }
        }
    },
    mounted: async function() {
        await this.fetchMembers();
        await this.fetchState();
        this.ready = true;
    },
    template: `
      <div>
      <h5>Miembros</h5>
      <div class="row" v-if="ready">
        <div class="col-lg-6">
          <users-grid :users="members" v-on:itemClick="loadDetails"/>
          <div class="d-flex justify-content-center">
            <button class="btn btn-outline-primary" v-if="moreMembers" @click="fetchMembers(false)">Cargar más</button>
          </div>
        </div>
        <div class="col-lg-6">
          <p v-if="selectedUser === undefined">Selecciona un miembro para ver detalles.</p>
          <squad-user-profile v-else :initial-state="initialState" :selected-user="selectedUser" :squad-id="squadId"/>
        </div>
      </div>
      </div>
    `
})