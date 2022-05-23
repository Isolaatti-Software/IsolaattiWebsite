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
        refresh: function () {
            this.posts = [];
            this.lastPostGotten = 0;
            this.noMoreContent = false;
            this.loading = true;
            this.fetchFeed();
        },
        concatPost: function (post) {
            this.posts = [post].concat(this.posts);
        },
        removePost: function (postId) {
            const index = this.posts.findIndex(p => p.postData.id === postId);
            if (index === -1) {
                return;
            }
            this.posts.splice(index, 1);
        }

    },
    mounted: function() {
        this.$nextTick(function () {
            this.fetchFeed();
            let globalThis = this;
            this.audioPlayer.onended = function () {
                globalThis.audioUrl = "";
            };
            globalEventEmmiter.$on("posted", this.concatPost);
            globalEventEmmiter.$on("postDeleted", this.removePost);
        });
    }
});


new Vue({
    el: "#rightbar"
});