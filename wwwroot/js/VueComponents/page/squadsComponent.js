const squadsComponent = {
    data: function() {
        return {
            path: ""
        }
    },
    methods: {
        navigate: function(path) {
            window.location.hash = "#" + path;
            this.path = path;
        }
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
                        <button class="btn btn-sm btn-light">Solicitudes</button>
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
            invitations: [], // user info objects
            invitationMessage: "Hola, te invito a mi Squad",
            userSearchResult: undefined,
            userSearchQuery: ""
        }
    },
    computed: {
        filteredSearchResult: function(){
            if(this.userSearchResult === undefined)
                return [];
            return this.userSearchResult.filter(el => !this.invitationsUserIds.includes(Number(el.resourceId)));
        },
        invitationsUserIds: function(){
            return this.invitations.map(el => Number(el.resourceId));
        }
    },
    methods: {
        search: _.debounce(async function(){
            const response = await fetch(`/api/Search/Quick?q=${encodeURIComponent(this.userSearchQuery)}&onlyProfile=True`, {
                method: "get",
                headers: this.customHeaders
            });
            this.userSearchResult = await response.json();
        }, 300),
        addToInvitations: function(invitation) {
            this.invitations.push(invitation);
        },
        removeFromInvitations: function(invitation) {
            const index = this.invitations.findIndex(el => el.resourceId === invitation.resourceId);
            this.invitations.splice(index,1);
        },
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
            const squadInvitationsReponse = await fetch(`/api/Squads/${squadCreationParsedResponse.squad.id}/InviteMany`, {
                method: "post",
                headers: this.customHeaders,
                body: JSON.stringify({
                    message: this.invitationMessage,
                    userIds: this.invitationsUserIds
                })
            });
            const parsedSquadInvitationsResponse = await squadInvitationsReponse.json();
            this.makingInvitations = false;
            this.submitting = false;
            window.location = `/squads/${squadCreationParsedResponse.squad.id}`;
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
            <div class="form-group">
                <h6>Invitar</h6>
                <div class="d-flex flex-column m-2">
                    <div class="form-group" v-if="invitations.length > 0">
                        <label>Mensaje de invitación</label>
                        <textarea class="form-control" v-model="invitationMessage"></textarea>
                    </div>
                    <div class="rounded p-2 bg-primary text-light pr-3 m-1" v-for="invitedUser in invitations"> 
                        <button type="button" class="btn btn-sm btn-primary" @click="removeFromInvitations(invitedUser)">&times;</button> 
                        <span>{{invitedUser.contentPreview}}</span>
                    </div> 
                </div>
                <input type="text" class="form-control"  v-model="userSearchQuery" @input="search"
                placeholder="Comienza a buscar por nombre..."/>
                <div class="list-group-flush list-group mt-1" v-if="userSearchResult!==undefined">
                    <button class="list-group-item list-group-item-action" v-for="result in filteredSearchResult" 
                        @click="addToInvitations(result)">#{{result.resourceId}} {{result.contentPreview}}</button>
                </div>
            </div>
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
    template: `
    <p>Squads feed component works!</p>
    `
}

const yourSquadsComponent = {
    template: `
    <p>Your squads component works!</p>
    `
}

// This is "this page component"
Vue.component('squads-page', squadsComponent);

// Components for "subpages"
Vue.component('create-squad', createSquadComponent);
Vue.component('your-squads', yourSquadsComponent);
Vue.component('squads-feed', squadsFeed);


