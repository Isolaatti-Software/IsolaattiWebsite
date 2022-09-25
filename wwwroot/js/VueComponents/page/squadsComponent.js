const squadsComponent = {
    data: function() {
        return {
            path: ""
        }
    },
    methods: {

    },
    mounted: function() {

    },
    template: `
    <div class="row m-0">
        <div class="col-xl-3 col-lg-4">
            <aside class="mt-4 p-2 isolaatti-card">
                <div class="translucent-white-card">
                    <h3 class="m-2"><i class="fa-solid fa-people-group"></i> Squads</h3>
                    <div class="d-flex flex-column mt-3">
                        <button @click="$router.push('/crear')" class="btn btn-sm btn-primary mb-2"><i class="fa-solid fa-plus"></i> Nuevo squad</button>
                        <div class="dropdown-divider"></div>
                        <button @click="$router.push('/')" class="btn btn-sm btn-light mb-2">Squads donde eres miembro</button>
                        <button @click="$router.push('/tuyos')" class="btn btn-sm btn-light mb-2">Tus Squads</button>
                        <button @click="$router.push('/solicitudes_union')" class="btn btn-sm btn-light mb-2">Solicitudes</button>
                        <button @click="$router.push('/invitaciones')" class="btn btn-sm btn-light">Invitaciones</button>
                    </div>
                </div>
            </aside>
        </div>
        <div class="col-xl-9 col-lg-8">
            <router-view class="mt-4"></router-view>
        </div>
        
    </div>
    `
};

const createSquadComponent = {
    data: function() {
        return {
            customHeaders: customHttpHeaders,
            // states
            submitting: false,
            makingSquad: false,
            makingInvitations: false,
            
            name: "",
            shortDescription: "",
            extendedDescription: "",
            privacy: 0, // or private
            justCreatedSquadId: undefined
        }
    },
    computed: {
        
    },
    methods: {
        createSquad: async function() {
            this.submitting = true;
            this.makingSquad = true;
            const squadCreationResponse = await fetch('/api/Squads/Create', {
                method: "post",
                headers: this.customHeaders,
                body: JSON.stringify({
                    name: this.name,
                    description: this.shortDescription,
                    extendedDescription: this.extendedDescription,
                    privacy: this.privacy
                })
            });
            this.makingSquad = false;
            const squadCreationParsedResponse = await squadCreationResponse.json();
            this.makingInvitations = true;
            this.justCreatedSquadId = squadCreationParsedResponse.squad.id;

        },
        onInvitationsCreated: function() {
            this.makingInvitations = false;
            this.submitting = false;
            window.location = `/squads/${this.justCreatedSquadId}`;
        }
    },
    template: `
<div>
    <div class="isolaatti-card">
        <h5 class="mb-3"><i class="fa-solid fa-plus"></i> Crear Squad</h5>
        <div class="d-flex flex-column">
            <div class="form-group">
                <label for="name">Nombre</label>
                <input type="text" class="form-control" id="name" v-model="name">
            </div>
            <div class="form-group">
                <label for="short-description">Descripción corta</label>
                <input type="text" class="form-control" id="short-description" v-model="shortDescription">
            </div>
            <div class="form-group">
                <label for="description">Descripción extendida</label>
                <textarea type="text" class="form-control" 
                id="description" placeholder="Markdown es compatible" rows="10" v-model="extendedDescription"></textarea>
            </div>
            <div class="form-group">
                <label for="privacy-selector">Privacidad</label>
                <select class="custom-select" id="privacy-selector" v-model="privacy">
                    <option :value="1">Todos pueden solicitar entrar</option>
                    <option :value="0">Solo por invitación</option>
                </select>
            </div>
            <squad-invitation-creator-user-searcher 
                :auto-send="true" 
                :squad-id="justCreatedSquadId"
                @created="onInvitationsCreated"
            ></squad-invitation-creator-user-searcher>
            <div class="alert alert-info" v-if="makingSquad">
                Creando squad...
            </div>
            <div class="alert alert-info" v-if="makingInvitations">
                Haciendo invitaciones...
            </div>
            <div class="d-flex justify-content-end">
                <button type="button" class="btn btn-primary" @click="createSquad" :disabled="submitting">Crear</button>
            </div>
        </div>

    </div>

    
</div>
    `
}

const squadsFeed = {
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
            {{squad.name}}<br>
            <small>id: {{squad.id}}</small>
          </a>
        </div>
    </section>
    `
}

const yourSquadsComponent = {
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

const requestsComponent = {
    data: function() {
        return {
            customHeaders: customHttpHeaders,
            joinRequests: [],
            loading: true,
            error: false,
            editionMode: false,
            selectedInvitationId: undefined,
            
            overallMode: "forMe" //or fromMe
        }
    },
    methods: {
        fetchInvitations: async function() {
            const response = await fetch(`/api/Users/Me/Squads/MyInvitations`, {
                headers: this.customHeaders
            });
            try {
                this.invitations = (await response.json());
                this.loading = false;
            } catch (e) {
                this.error = true;
            }
        },
        selectInvitation: function(id) {
            if(this.selectedInvitationId === id) {
                this.selectedInvitationId = undefined;
            } else {
                this.selectedInvitationId = id;
            }

        },
        setEditionMode: function() {
            this.editionMode = !this.editionMode;
        },
        profileLink: function(profileId) {
            return `/perfil/${profileId}`;
        },
        profileImageLink: function(profileId) {
            return `/api/Fetch/GetUserProfileImage?userId=${profileId}`
        }
    },
    mounted: async function() {
        await this.fetchInvitations();
    },
    template: `
      <section class="isolaatti-card">
      <h5>Solicitudes</h5>
      <div class="btn-group w-100">
        <button class="btn">
          Para tí
        </button>
        <button class="btn">
          De tí
        </button>
      </div>
      
      <template v-if="overallMode==='forMe'">
        
      </template>
      <template v-else>
        
      </template>
      <div v-if="loading" class="d-flex justify-content-center mt-2">
        <div class="spinner-border" role="status">
          <span class="sr-only">Cargando más contenido...</span>
        </div>
      </div>
      
      </section>
    `
}


const invitationsComponent = {
    data: function() {
        return {
            customHeaders: customHttpHeaders,
            invitations: [],
            loading: true,
            error: false,
            editionMode: false,
            selectedInvitationId: undefined,
            overallMode: "forMe" //or fromMe
        }
    },
    methods: {
        fetchInvitationsForMe: async function() {
            this.loading = true;
            const response = await fetch(`/api/Users/Me/Squads/InvitationsForMe`, {
                headers: this.customHeaders
            });
            try {
                this.invitations = (await response.json());
            } catch (e) {
                this.error = true;
            }
            this.loading = false;
        },
        fetchInvitationsFromMe: async function() {
            const response = await fetch(`/api/Users/Me/Squads/MyInvitations`, {
                headers: this.customHeaders
            });
            try {
                this.invitations = (await response.json());
            } catch(e) {
                this.error = true;
            }
            this.loading = false;
        },
        onInvitationUpdate: function(invitation) {
            const index = this.invitations.findIndex(inv => inv.invitation.id === invitation.id);
            const temp = this.invitations;
            temp[index].invitation = invitation;
            this.invitations = temp;
        }
    },
    watch:{
        overallMode: {
            handler: async function(val, oldVal) {
                if(val === oldVal) {
                    return;
                }
                this.invitations = [];
                switch(val) {
                    case "forMe": await this.fetchInvitationsForMe();
                    break;
                    case "fromMe": await this.fetchInvitationsFromMe();
                    break;
                }
            },
            immediate: true
        }
    },
    mounted: async function() {
        
    },
    template: `
    <section class="isolaatti-card">
      <h5>Invitaciones</h5>
      <div class="btn-group w-100">
        <button class="btn" :class="{'btn-primary':overallMode==='forMe'}" @click="overallMode='forMe'">
          Para tí
        </button>
        <button class="btn" :class="{'btn-primary':overallMode==='fromMe'}" @click="overallMode='fromMe'">
          De tí
        </button>
      </div>
      <squad-invitations-list :items="invitations" @invitation-update="onInvitationUpdate"></squad-invitations-list>
      <div v-if="loading" class="d-flex justify-content-center mt-2">
        <div class="spinner-border" role="status">
          <span class="sr-only">Cargando más contenido...</span>
        </div>
      </div>
      
    </section>
    `
}

// This is "this page component"
Vue.component('squads-page', squadsComponent);

// Components for "subpages"
Vue.component('create-squad', createSquadComponent);
Vue.component('your-squads', yourSquadsComponent);
Vue.component('squads-feed', squadsFeed);
Vue.component('squads-requests', requestsComponent);
Vue.component('squads-invitations', invitationsComponent);


