Vue.component('users-grid', {
    props: {
        users: {
            type: Array,
            required: true
        }
    },
    methods: {
        imageUrl: function(imageId){
            if(imageId === null || imageId === "") {
                return "/res/imgs/avatar.svg";
            }
            return `/api/images/image/${imageId}?mode=reduced`
        },
        navigateToProfile: function (profileId) {
            window.location = `/perfil/${profileId}`;
        },
        onItemClick: function(user) {
            if(this.$listeners["itemClick"]) {
                this.$emit("itemClick", user)
            } else {
                this.navigateToProfile(user.id)
            }
        }
    },
    template: `
      <div>
      <div class="users-grid">
        <div class="user-profile-card" v-for="user in users" @click="onItemClick(user)">
          <img :src="imageUrl(user.imageId)" class="profile-pic"/>
          <p class="text-ellipsis mw-100 m-0">{{ user.name }}</p>
        </div>

      </div>
      <p v-if="users.length < 1">
        No hay contenido que mostrar...
      </p>
      </div>
    `

})