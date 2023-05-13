Vue.component("add-squad-admin-modal-dialog", {
    props: {
        squadId: {
            type: String,
            required: true
        }
    },
    data: function() {
        return {
            customHeaders: customHttpHeaders,
            page: "search", // or confirmation,
            result: {
                users: [],
                lastId: 0
            },
            suggestions: undefined,
            error: undefined,
            errorLoadingSuggestions: false,
            loadingSuggestions: false,
            query: "",
            selectedUser: undefined,
            sendingChangeOwnerRequest: false
        }
    },
    methods: {
        onSearchInput: _.debounce(async function() {
            await this.search(true)
        }, 300),
        search: async function(reload) {
            if(reload) {
                this.result.lastId = 0;
                this.result.users = [];
            }
            const response = await fetch(`/api/Search/${this.squadId}/RankedUserSearch?q=${encodeURIComponent(this.query)}&lastId=${this.result.lastId}`, {
                method: 'get',
                headers: this.customHeaders
            });
            if(!response.ok){
                
                return;
            }
            try {
                const parsed = await response.json()
                this.result.users = this.result.users.concat(parsed.users);
                this.result.lastId = parsed.lastId;
            } catch(e) {

            }

        },
        fetchNextPage: function() {

        },
        loadSuggestions: async function() {
            this.loadingSuggestions = true;
            const response = await fetch(`/api/Search/${this.squadId}/SearchSuggestions?admins=False`, {
                method: "get",
                headers: this.customHeaders
            });
            this.loadingSuggestions = false;
            if(!response.ok){
                this.errorLoadingSuggestions = true;
                return;
            }
            this.errorLoadingSuggestions = false;
            const parsedResponse = await response.json();
            this.suggestions = parsedResponse;
        },
        loadDetails: function(user) {
            console.log(user);
        },
        selectUser: function(user) {
            this.selectedUser = user;
            this.page = "confirmation"
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
        sendAddAdminRequest: async function() {
            this.sendingChangeOwnerRequest = true;
            this.error = undefined;
            const response = await fetch(`/api/squads/${this.squadId}/AddAdmin`,{
                method: "post",
                headers: this.customHeaders,
                body: JSON.stringify({
                    id: this.selectedUser.id
                })
            });
            this.sendingChangeOwnerRequest = false;
            if(response.ok) {
                window.location.reload();
            }
            try {
                this.error = await response.json();
            } catch(e) {
                this.error = {
                    result: "unknown_server_error"
                }
            }
            
        }
    },
    mounted: async function() {
        await this.loadSuggestions();
    },
    template: `
    <div>
      <section v-show="page === 'search'">
        <div class="form-group">
          <label for="search-field">Buscar entre todos los miembros</label>
          <input id="search-field" type="text" class="form-control" v-model="query" @input="onSearchInput"/>
        </div>
        <template v-if="result.users.length > 0">
          <hr/>
          <users-grid :users="result.users.map(u => u.user)" @itemClick="selectUser"/>
          <div class="d-flex justify-content-center">
            <button class="btn btn-primary btn-sm">Cargar más</button>
          </div>
        </template>
        <template v-else-if="suggestions !== undefined">
          <hr/>
          <h5>Sugerencias</h5>
          <users-grid :users="suggestions.users.map(u => u.user)" @itemClick="selectUser"/>
        </template>
      </section>
      <section v-show="page === 'confirmation'">
        <template v-if="selectedUser !== undefined">
          <div class="d-flex">
            <button class="btn btn-sm" @click="page = 'search'">
              <i class="fa-solid fa-arrow-left"></i>
            </button>
          </div>
          <p>Esto es lo que ocurrirá:</p>
          <ul>
            <li><strong>{{selectedUser.name}}</strong> será un administrador. Más adelante podrás asignarle permisos.</li>
          </ul>
          <div class="d-flex flex-column align-items-center mb-3">
            <img class="profile-pic" :src="profileImageLink(selectedUser.imageId)"/>
            <a :href="userProfileLink(selectedUser.id)" target="_blank">{{selectedUser.name}}</a>
          </div>
          <div class="d-flex justify-content-end">
            <button class="btn btn-primary btn-sm w-100" @click="sendAddAdminRequest" :disabled="sendingChangeOwnerRequest">
              <div class="spinner-border text-light spinner-border-sm" role="status" v-if="sendingChangeOwnerRequest">
                <span class="sr-only">Loading...</span>
              </div>
              Continuar
            </button>
          </div>
          <div class="alert alert-danger mt-1" v-if="error !== undefined">
            Ocurrió un error en el servidor "{{ error.result }}"
          </div>
        </template>
        
      </section>
    </div>
    `
})