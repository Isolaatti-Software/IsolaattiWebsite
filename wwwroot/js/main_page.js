async function likePost(post) {
    const postId = post.id;
    const requestData = {id: postId}
    const globalThis = this;
    const response = await fetch("/api/Likes/LikePost", {
        method: "POST",
        headers: this.customHeaders,
        body: JSON.stringify(requestData),
    });

    response.json().then(function (post) {
        let index = vueContainer.posts.findIndex(post => post.postData.id === postId);
        Vue.set(vueContainer.posts, index, post);
    });
}

async function unLikePost(post) {
    const postId = post.id;
    const requestData = {id: postId}
    const globalThis = this;
    const response = await fetch("/api/Likes/UnLikePost", {
        method: "POST",
        headers: this.customHeaders,
        body: JSON.stringify(requestData),
    });

    response.json().then(function (post) {
        let index = vueContainer.posts.findIndex(post => post.postData.id === postId);
        Vue.set(vueContainer.posts, index, post);
    });
}

let vueContainer = new Vue({
    el: "#vue-container",
    data: {
        customHeaders: customHttpHeaders,
        userData: userData,
        lastPostGotten: 0,
        posts: [],
        loading: true,
        noMoreContent: false,
        audioPlayer: new Audio(),
        audioUrl: "",
        playing: false,
        paused: false,
        postLinkToShare: "",
        commentsViewer: {
            postId: 0,
            comments: []
        }
    },
    computed: {
        openThreadLink: function() {
            return `/pub/${this.commentsViewer.postId}`;
        }
    },
    methods: {
        fetchFeed: async function () {
            const globalThis = this;
            const response = await fetch(`/api/Feed/${this.lastPostGotten}/10`, {
                method: "GET",
                headers: this.customHeaders
            });
            response.json().then(feed => {
                globalThis.posts = globalThis.posts.concat(feed.posts)
                globalThis.lastPostGotten = feed.lastPostId;
                globalThis.noMoreContent = !feed.moreContent;
                globalThis.loading = false;
            })
        },
        likePost: likePost,
        unLikePost: unLikePost,
        refresh: function () {
            this.posts = [];
            this.lastPostGotten = 0;
            this.noMoreContent = false;
            this.loading = true;
            this.fetchFeed();
        },
        compileMarkdown: function (raw) {
            return marked(raw);
        },
        playAudio: function(url) {
            if(this.audioUrl !== url) {
                this.audioPlayer.pause();
                this.audioUrl = url
                this.audioPlayer.src = url;
                this.audioPlayer.play();
                this.paused = false;
            } else {
                if(!this.audioPlayer.paused) {
                    this.audioPlayer.pause()
                    this.paused = true;
                    this.playing = false;
                } else {
                    this.audioPlayer.play();
                    this.playing = true;
                    this.paused = false;
                }
            }
            this.playing = true;
        },
        getPostStyle: function(themeDefinitionJson) {
            if(themeDefinitionJson === null)
                return "";
            const theme = JSON.parse(themeDefinitionJson);
            return `color: ${theme.fontColor};
                background-color: ${theme.backgroundColor};
                border: ${theme.border.size} ${theme.border.type} ${theme.border.color}`;
        },
        copyToClipboard: function(relativeUrl) {
            let absoluteUrl = `${window.location.protocol}//${window.location.host}${relativeUrl}`;
            navigator.clipboard.writeText(absoluteUrl).then(function() {
                alert("Se copió el texto al portapapeles");
            });
        },
        deletePost: function(postId) {

        },
        viewComments: function(post) {
            this.commentsViewer.postId = post.id;
            this.getComments();
        },
        getComments: function() {

        }
    },
    mounted: function() {
        this.$nextTick(function() {
            this.fetchFeed();
            let globalThis = this;
            this.audioPlayer.onended = function() {
                globalThis.audioUrl = "";
            }
        });
    }
});