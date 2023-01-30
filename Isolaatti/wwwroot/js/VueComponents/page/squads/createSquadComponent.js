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
Vue.component('create-squad', createSquadComponent);