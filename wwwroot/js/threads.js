// Threads page

function fetchPost() {
    const that = this;
    fetch(`/api/Fetch/Post/${this.threadId}`, {headers: this.customHeaders}).then(result => {
        result.json().then(post => {
            that.post = post;
            that.loading = false;
        })
    })
}


function deleteComment(id) {
    if (!confirm("¿De verdad deseas eliminar esta publicación?")) {
        return;
    }

    fetch("/api/Posting/Comment/Delete", {
        method: "POST",
        headers: this.customHeaders,
        body: JSON.stringify({id: id})
    }).then(res => {
        if (res.ok) {
            let commentId = this.comments.findIndex(value => value.id === id);
            if (commentId === -1) return;
            this.comments.splice(commentId, 1);
        }
    });
}

let vueInstance = new Vue({
    el: "#vue-container",
    data: {
        customHeaders: customHttpHeaders,
        threadId: threadToReadId,
        userId: userData.id,
        post: undefined,
        comments: []
    },
    methods: {
        fetchPost: fetchPost,
        addAComment: function (comment) {
            const temp = this.comments;
            this.comments = [comment].concat(temp);
        },
        deleteComment: deleteComment
    },
    mounted: function () {
        let globalThis = this;
        this.$nextTick(function () {
            globalThis.fetchPost();
        });
    }
})