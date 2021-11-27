Vue.component('post-template',{
    props: ['post','audio-url','paused','is-modal'],
    data: function() {
        return {
            userData: userData,
        }
    },
    computed: {
        profileLink: function() {
            return `/Perfil?id=${this.post.userId}`
        },
        reportLink: function() {
            return `/Reports/ReportPostOrComment?postId=${this.post.id}`;
        },
        openThreadLink: function() {
            return `/Hilo/${this.post.id}`;
        },
        editPostLink: function() {
            return `/EditorPro?edit=True&postId=${this.post.id}`
        }
    },
    methods: {
        compileMarkdown: function(raw) {
            return marked.parse(raw);
        },
        getPostStyle: function(themeDefinitionJson) {
            if(themeDefinitionJson === null)
                return "";
            const theme = JSON.parse(themeDefinitionJson);
            function returnColorsAsString(array) {
                let res = "";

                for(let i=0; i < array.length - 1; i++) {
                    res += array[i] + ", "
                }

                res += array[array.length - 1];

                return res;
            }

            let backgroundProperty;

            // if a gradient of any kind is selected it will generate the corresponding background,
            // otherwise it will return the solid color
            if(theme.gradient === "true"){
                backgroundProperty = theme.background.type ===
                "linear" ?
                    `linear-gradient(${theme.background.direction}deg, ${returnColorsAsString(theme.background.colors)})` :
                    `radial-gradient(${returnColorsAsString(theme.background.colors)})`;
            } else {
                backgroundProperty = theme.backgroundColor;
            }

            return `color: ${theme.fontColor};
                background: ${backgroundProperty};
                border: ${theme.border.size}px ${theme.border.type} ${theme.border.color};
                border-radius: ${theme.border.radius}px;`;
        },
        getUserImageUrl: function(userId) {
            return `/api/Fetch/GetUserProfileImage?userId=${userId}`
        }
    },
    template: `
      <div class="d-flex mb-2 flex-column p-2 post" :style="getPostStyle(post.themeJson)">
      <div class="d-flex justify-content-between align-items-center">
        <div class="d-flex">
          <img class="user-avatar" :src="getUserImageUrl(post.userId)">
          <div class="d-flex flex-column ml-2">
            <span class="user-name"><a :href="profileLink">{{ post.userName }}</a> </span>
            <div class="d-flex privacy-icon-container">
              <div v-if="post.privacy === 1">
                <i class="fas fa-user" title="Private" aria-hidden="true"></i><span class="sr-only">Privado</span>
              </div>
              <div v-if="post.privacy === 2">
                <i class="fas fa-user-friends" title="People on Isolaatti" aria-hidden="true"></i><span class="sr-only">Usuarios de Isolaatti</span>
              </div>
              <div v-if="post.privacy === 3">
                <i class="fas fa-globe" title="All the world" aria-hidden="true"></i><span class="sr-only">Todos</span>
              </div>
              <span>{{ new Date(post.date).toUTCString() }}</span>
            </div>
          </div>
        </div>
        <div class="dropdown dropleft">
          <button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true">
            <i class="fas fa-ellipsis-h"></i>
          </button>
          <div class="dropdown-menu">
            <a :href="editPostLink" class="dropdown-item" v-if="post.userId===this.userData.id">Editar</a>
            <a href="#modal-share-post" v-on:click="$emit('input',openThreadLink)" class="dropdown-item"
               data-toggle="modal">Compartir</a>
            <a href="#" class="dropdown-item" v-if="post.userId===this.userData.id" v-on:click="$emit('delete',1)">Eliminar</a>
            <a :href="reportLink" class="dropdown-item" target="_blank" v-if="userData.id!==-1">Reportar</a>
          </div>
        </div>
      </div>

      <div class="d-flex mt-1" v-if="post.audioAttachedUrl!=null">
        <button type="button" class="btn btn-primary btn-sm" v-on:click="$emit('play-audio')">
          <i class="fas fa-play" v-if="post.audioAttachedUrl !== audioUrl || paused"></i>
          <i class="fas fa-pause" v-else></i>
        </button>
      </div>
      <div class="mt-2 post-content" v-html="compileMarkdown(post.textContent)"></div>
      <div class="d-flex justify-content-end">
        <div class="btn-group btn-group-sm" v-if="userData.id!=-1">
          <a class="btn btn-dark btn-sm" href="#thread-viewer" data-toggle="modal" v-if="!isModal" v-on:click="$emit('view-thread')">
            <i class="fas fa-comments" aria-hidden="true"></i> {{ post.numberOfComments }}
          </a>
          <button v-if="!post.liked"  v-on:click="$emit('like',0.1)" class="btn btn-dark btn-sm" type="button">
            <i class="fas fa-thumbs-up" aria-hidden="true"></i> {{ post.numberOfLikes }}
          </button>
          <button v-if="post.liked" v-on:click="$emit('un-like',0.1)" class="btn btn-primary btn-sm" type="button">
            <i class="fas fa-thumbs-up" aria-hidden="true"></i> {{ post.numberOfLikes }}
          </button>
        </div>
        <div class="btn-group btn-group-sm" v-else>
          <a class="btn btn-dark" :href="openThreadLink"><i class="fas fa-comments"></i> {{post.numberOfComments}}</a>
        </div>
      </div>
      </div>
    `
})