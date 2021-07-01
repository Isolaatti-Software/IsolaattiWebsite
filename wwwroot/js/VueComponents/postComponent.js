Vue.component('post-template',{
    props: ['post','audio-url','paused'],
    data: function() {
        return {
            userData: userData
        }
    },
    computed: {
        profileLink: function() {
            return `/Profile?id=${this.post.userId}`
        },
        reportLink: function() {
            return `/Reports/ReportPostOrComment?postId=${this.post.id}`;
        },
        openThreadLink: function() {
            return `/Threads/${this.post.id}`;
        },
        editPostLink: function() {
            return `/PostEditor?edit=True&postId=${this.post.id}`
        }
    },
    methods: {
        compileMarkdown: function(raw) {
            return marked(raw);
        },
        getPostStyle: function(themeDefinitionJson) {
            if(themeDefinitionJson === null)
                return "";
            const theme = JSON.parse(themeDefinitionJson);
            return `color: ${theme.fontColor};
                background-color: ${theme.backgroundColor};
                border: ${theme.border.size}px ${theme.border.type} ${theme.border.color};
                border-radius: ${theme.border.radius}px;`;
        }
    },
    template: `
      <div class="d-flex mb-2 flex-column p-2 post" :style="getPostStyle(post.themeJson)">
      <div class="d-flex justify-content-between align-items-center">
        <span class="user-name"><a :href="profileLink">{{ post.userName }}</a> </span>
        <div class="dropdown dropleft">
          <button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true">
            <i class="fas fa-ellipsis-h"></i>
          </button>
          <div class="dropdown-menu">
            <a :href="editPostLink" class="dropdown-item" v-if="post.userId===this.userData.id">Edit</a>
            <a href="#modal-share-post" v-on:click="$emit('input',openThreadLink)" class="dropdown-item" data-toggle="modal">Share</a>
            <a href="#" class="dropdown-item" v-if="post.userId===this.userData.id" v-on:click="$emit('delete',1)">Delete</a>
            <a :href="reportLink" class="dropdown-item" target="_blank">Report</a>
          </div>
        </div>
      </div>
      <div class="d-flex privacy-icon-container">
        <div v-if="post.privacy === 1">
          <i class="fas fa-user" title="Private" aria-hidden="true"></i><span class="sr-only">Only you</span>
        </div>
        <div v-if="post.privacy === 2">
          <i class="fas fa-user-friends" title="People on Isolaatti" aria-hidden="true"></i><span class="sr-only">People on Isolaatti</span>
        </div>
        <div v-if="post.privacy === 3">
          <i class="fas fa-globe" title="All the world" aria-hidden="true"></i><span class="sr-only">Everyone</span>
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
        <div class="btn-group btn-group-sm">
          <a class="btn btn-dark btn-sm" :href="openThreadLink">
            <i class="fas fa-comments" aria-hidden="true"></i> {{ post.numberOfComments }}
          </a>
          <button v-if="!post.liked"  v-on:click="$emit('like',0.1)" class="btn btn-dark btn-sm" type="button">
            <i class="fas fa-thumbs-up" aria-hidden="true"></i> {{ post.numberOfLikes }}
          </button>
          <button v-if="post.liked" v-on:click="$emit('un-like',0.1)" class="btn btn-primary btn-sm" type="button">
            <i class="fas fa-thumbs-up" aria-hidden="true"></i> {{ post.numberOfLikes }}
          </button>
        </div>
      </div>
      </div>
    `
})