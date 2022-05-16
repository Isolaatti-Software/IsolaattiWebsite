// Threads page

function fetchPost() {
    fetch(`/api/Fetch/Post/${this.threadId}`, {headers: this.customHeaders}).then(result => {
        result.json().then(post => {
            this.post = post;
            this.loading = false;
        })
    })
}

function fetchComments() {
    fetch(`/api/Fetch/Post/${this.threadId}/Comments`, {
        headers: this.customHeaders
    }).then(res => {
        res.json().then(comments => {
            this.comments = comments;
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
        comments: [],
        commentTextarea: "",
        selectionStart: 0,
        selectionEnd: 0,
        audioPlayer: new Audio(),
        audioUrl: "",
        playing: false,
        paused: false,
        audio: {
            recording: false,
            url: "",
            timeInSeconds: 0,
            recorded: false,
            consolidated: false,
            recorder: {
                mediaRecorder: null,
                mediaStream: null,
                audioData: [],
                audioPlayer: new Audio(),
                resultantBlob: null
            }
        },
        postLinkToShare: "",
        submitting: false,
        uploadingData: {
            uploading: false,
            progress: 0
        }
    },
    computed: {
        makeCommentButtonDisabled: function () {
            return this.commentTextarea === "" || this.commentTextarea === null || this.uploadingData.uploading || this.submitting
        },
        addAudioButtonText: function () {
            return (this.audio.consolidated) ? "Audio grabado" : "Grabar audio";
        },
        uploadingProgressBarStyleCss: function () {
            return `width: ${this.uploadingData.progress}%;`
        },
        progressStyle: function () {
            return `width:${(this.audio.timeInSeconds * 100) / 120}%`
        },
        clockFormatTime: function () {
            let truncatedSecs = Math.round(this.audio.timeInSeconds);
            let minutes = truncatedSecs / 60;
            let seconds = truncatedSecs % 60;
            if (seconds < 10) {
                seconds = `0${seconds}`
            }
            return `${Math.trunc(minutes)}:${seconds}`;
        }
    },
    methods: {
        makeComment: async function () {
            const globalThis = this;

            let requestBody = {
                content: this.commentTextarea,
                audioUrl: null,
                privacy: 1
            }

            const url = `/api/Posting/Post/${this.threadId}/Comment`

            async function sendCommentRequest() {
                globalThis.submitting = true;
                const response = await fetch(url, {
                    headers: globalThis.customHeaders,
                    method: "POST",
                    body: JSON.stringify(requestBody)
                });

                response.json().then(comment => {
                    globalThis.commentTextarea = "";
                    globalThis.addAComment(comment);
                    globalThis.submitting = false;
                });
            }

            await sendCommentRequest();
        },

        fetchPost: fetchPost,
        getPostStyle: function (themeDefinitionJson) {
            if (themeDefinitionJson === null)
                return "";
            const theme = JSON.parse(themeDefinitionJson);
            return `color: ${theme.fontColor};
                        background-color: ${theme.backgroundColor};
                        border: ${theme.border.size} ${theme.border.type} ${theme.border.color}`;
        },
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


        thisVueInstance = this;
    }
})