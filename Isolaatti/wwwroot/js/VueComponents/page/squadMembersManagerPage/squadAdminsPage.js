Vue.component("squad-admins-page", {
    props: {
        squadId: {
            type: String,
            required: true
        },
        users: {
            type: Array,
            required: true
        },
        initialState: {
            type: Object,
            required: true
        }
    },
    data: function() {
        return {
            customHeaders: customHttpHeaders,
            selectedUser: undefined
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
        }
    },
    template: `
      <div>
      <h5>Administradores actuales</h5>
      <div class="row">
        <div class="col-lg-6">
          <users-grid :users="users" v-on:itemClick="loadDetails"/>
        </div>
        <div class="col-lg-6">
          <p v-if="selectedUser === undefined">Selecciona un miembro para ver detalles.</p>
          <squad-user-profile v-else :initial-state="initialState" :selected-user="selectedUser" :squad-id="squadId"/>
        </div>
      </div>
      <!-- Remove administrator modal-->
      <div class="modal" id="modal-remove-admin-confirmation">
        <div class="modal-dialog modal-dialog-centered">
          <remove-admin-from-squad-modal-content v-if="selectedUser !== undefined" 
                                                 :selected-user="selectedUser" 
                                                 :squad-id="squadId"/>
        </div>
      </div>
      <!-- Remove user from squad modal-->
      <div class="modal" id="modal-remove-user-from-squad-confirmation">
        <div class="modal-dialog modal-dialog-centered">
          <remove-squad-user-modal-content v-if="selectedUser !== undefined" 
                                           :selected-user="selectedUser"
                                           :squad-id="squadId"/>
        </div>
      </div>
      </div>
    `
})