Vue.component('squad-invitation-creator-user-searcher', {
    props: {
        squadId: {
            type: String,
            required: false
        },
        autoSend: {
            type: Boolean,
            required: true
        }
    },
    data: function() {
        return {
            customHeaders: customHttpHeaders,
            invitations: [],
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
    watch: {
        squadId: {
            handler: async function(oldValue, newValue) {
                if(this.autoSend){
                    const response = await this.makeInvitations();
                    if(response.ok) {
                        this.$emit("created");
                    }
                }
            }
        }
    },
    methods: {
        search: _.debounce(async function(){
            const response = await fetch(`/api/Search/Quick?q=${encodeURIComponent(this.userSearchQuery)}&onlyProfile=True&contextType=squad&contextValue=${this.squadId}`, {
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
        makeInvitations: async function() {
            await fetch(`/api/Squads/${this.squadId}/InviteMany`, {
                method: "post",
                headers: this.customHeaders,
                body: JSON.stringify({
                    message: this.invitationMessage,
                    userIds: this.invitationsUserIds
                })
            });
            this.$emit("created");
        }
    },
    mounted: function() {
        
    },
    template: `
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
                <div class="d-flex justify-content-end mt-2 mb-1" v-if="!autoSend">
                  <button class="btn btn-primary" :disabled="invitations.length < 1" @click="makeInvitations">
                    Enviar
                  </button>
                </div>
                <div class="list-group-flush list-group mt-1" v-if="userSearchResult!==undefined">
                    <button class="list-group-item list-group-item-action" v-for="result in filteredSearchResult" 
                        @click="addToInvitations(result)">#{{result.resourceId}} {{result.contentPreview}}</button>
                </div>
            </div>
    `
})