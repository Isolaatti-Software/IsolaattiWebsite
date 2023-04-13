Vue.component("squad-admins-page", {
    props: {
        users: {
            type: Array,
            required: true
        }
    },
    data: function() {
        return {
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
        },
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
          <div v-else>
            <img :src="profileImageLink(selectedUser.imageId)" class="profile-pic"/>
            <h5>{{selectedUser.name}}</h5>
            <div class="d-flex">
              <button class="btn btn-primary btn-sm">Quitar administrador</button>
              <button class="btn btn-danger btn-sm ml-1">Sacar del squad</button>
            </div>
            <h5 class="mt-1">Permisos</h5>
            <p>Este administrador puede llevar a cabo lo siguiente:</p>
            <div class="d-flex flex-column">
              <div class="custom-control custom-switch">
                <input type="checkbox" class="custom-control-input" id="customSwitch1">
                <label class="custom-control-label" for="customSwitch1">Modificar nombre del squad</label>
              </div>
              <div class="custom-control custom-switch">
                <input type="checkbox" class="custom-control-input" id="customSwitch2">
                <label class="custom-control-label" for="customSwitch2">Modificar descripción y descripción extendida del squad</label>
              </div>
              <div class="custom-control custom-switch">
                <input type="checkbox" class="custom-control-input" id="customSwitch3">
                <label class="custom-control-label" for="customSwitch3">Modificar audio del squad</label>
              </div>
              <div class="custom-control custom-switch">
                <input type="checkbox" class="custom-control-input" id="customSwitch4">
                <label class="custom-control-label" for="customSwitch4">Modificar imagen del squad</label>
              </div>
            </div>
            <div class="d-flex justify-content-end mt-3">
              <button class="btn btn-primary btn-sm">Guardar</button>
            </div>
          </div>
        </div>
      </div>
      </div>
    `
})