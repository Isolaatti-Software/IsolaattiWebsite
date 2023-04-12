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
            owner: [],
            admins: []
        }
    },
    methods: {
        fetchOwner: async function() {
            const response = await fetch(`/api/Squads/${this.squadId}/Owner`,{
                method: "GET",
                headers: this.customHeaders
            });
            if(response.ok){
                const parsedResponse = await response.json();

                this.owner = [parsedResponse];
            }
            
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
                this.members = await response.json();
            }
            
        },
        fetchAdmins: async function(refresh) {
            let url = `/api/Squads/${this.squadId}/Admins`;
            if(refresh) {
                this.admins = [];
            }
            if(this.admins.length >= 20) {
                url += `?lastId=${this.admins[this.admins.length - 1].id}`
            }
            const response = await fetch(url,{
                method: "get",
                headers: this.customHeaders
            });

            if(response.ok){
                this.admins = await response.json();
            }

        },
        pictureUrl: function(picId) {
            return `/api/Fetch/ProfileImages/${picId}.png`;
        },
        squadMembersManagerPageUrl: function(tab) {
            let url = `/squads/${this.squadId}/gestion_miembros`;
            switch(tab) {
                case "owner": url += "/propietario"; break;
                case "admins": url += "/administradores"; break;
                case "members": url += "/miembros"; break;
            }
            
            return url;
        }
    },
    mounted: async function() {
        await this.fetchOwner();
        await this.fetchMembers(true);
        await this.fetchAdmins(true)
    },
    template: `
    <section class="isolaatti-card">
      <div class="d-flex w-100">
        <h5>Propietario</h5>
        <a class="btn btn-sm btn-link ml-auto" :href="squadMembersManagerPageUrl('owner')">Editar</a>
      </div>
      <users-grid :users="owner"></users-grid>
      <div class="d-flex w-100">
        <h5>Administradores</h5>
        <a class="btn btn-sm btn-link ml-auto" :href="squadMembersManagerPageUrl('admins')">Editar</a>
      </div>
      <users-grid :users="admins"></users-grid>
      <div class="d-flex w-100">
        <h5>Miembros</h5>
        <a class="btn btn-sm btn-link ml-auto" :href="squadMembersManagerPageUrl('members')">Editar</a>
      </div>
      <users-grid :users="members"></users-grid>
    </section>
    `
});