async function likePost(post, event) {
    const postId = post.id;
    const requestData = {id: postId}
    const globalThis = this;
    event.target.disabled = true;
    const response = await fetch("/api/Likes/LikePost", {
        method: "POST",
        headers: this.customHeaders,
        body: JSON.stringify(requestData),
    });

    response.json().then(function (post) {
        let index = vueContainer.posts.findIndex(post => post.postData.id === postId);
        Vue.set(vueContainer.posts, index, post);
        event.target.disabled = false;
    });
}

async function unLikePost(post, event) {
    const postId = post.id;
    const requestData = {id: postId}
    const globalThis = this;
    event.target.disabled = true;
    const response = await fetch("/api/Likes/UnLikePost", {
        method: "POST",
        headers: this.customHeaders,
        body: JSON.stringify(requestData),
    });

    response.json().then(function (post) {
        let index = vueContainer.posts.findIndex(post => post.postData.id === postId);
        Vue.set(vueContainer.posts, index, post);
        event.target.disabled = false;
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
            if (!confirm("¿De verdad deseas eliminar esta publicación?")) {
                return;
            }

            fetch("/api/Posting/Delete", {
                method: "POST",
                headers: this.customHeaders,
                body: JSON.stringify({id: postId})
            }).then(res => {
                if (res.ok) {
                    let postIndex = this.posts.findIndex(value => value.postData.id === postId);
                    this.posts.splice(postIndex, 1);
                }
            });
        },
        viewComments: function(post) {
            this.commentsViewer.postId = post.id;
            this.getComments();
        },
        getComments: function() {
            fetch(`api/Fetch/Post/${this.commentsViewer.postId}/Comments`, {
                headers: this.customHeaders
            }).then(res => {
                res.json().then(comments => {
                    this.commentsViewer.comments = comments;
                })
            })
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