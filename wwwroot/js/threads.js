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
        likePost: async function (post) {
            const requestData = {id: post.id}
            const globalThis = this;
            const response = await fetch("/api/Likes/LikePost", {
                method: "POST",
                headers: this.customHeaders,
                body: JSON.stringify(requestData),
            });

            response.json().then(function (post) {
                globalThis.post = post;
            });
        },
        unLikePost: async function (post) {
            const requestData = {id: post.id}
            const globalThis = this;
            const response = await fetch("/api/Likes/UnLikePost", {
                method: "POST",
                headers: this.customHeaders,
                body: JSON.stringify(requestData),
            });

            response.json().then(function (post) {
                globalThis.post = post;
            });
        },
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
                    globalThis.resetRecording();
                    globalThis.addAComment(comment);
                    globalThis.submitting = false;
                });
            }

            // there is an audio to upload
            if (this.audio.consolidated) {
                let storageRef = storage.ref();
                let date = new Date();

                let uploadTask = storageRef
                    .child(`audio_comments/${userData.id}/${date.toDateString()}-${date.getHours()}-${date.getMinutes()}-${date.getSeconds()}_audio.webm`)
                    .put(this.audio.recorder.resultantBlob);

                uploadTask.on(
                    'state_changed', (snapshot) => {
                        globalThis.uploadingData.progress = (snapshot.bytesTransferred / snapshot.totalBytes) * 100;
                        globalThis.uploadingData.uploading = snapshot.state === firebase.storage.TaskState.RUNNING;
                    },
                    (error) => {

                    },
                    () => {
                        uploadTask.snapshot.ref.getDownloadURL().then(function (url) {
                            requestBody.audioUrl = url;
                            sendCommentRequest();
                        })
                    });
            } else {
                await sendCommentRequest();
            }

        },
        compileMarkdown: function (raw) {
            return marked(raw);
        },
        copyToClipboard: function (relativeUrl) {
            let absoluteUrl = `${window.location.protocol}//${window.location.host}${relativeUrl}`;
            navigator.clipboard.writeText(absoluteUrl).then(function () {
                alert("Se copió el texto al portapapeles");
            });
        },
        deletePost: function (postId) {
            if (!confirm("¿De verdad deseas eliminar esta publicación?")) {
                return;
            }
            let globalThis = this;
            let form = new FormData();
            form.append("sessionToken", sessionToken);
            form.append("postId", postId);

            let request = new XMLHttpRequest();
            request.open("POST", "/api/EditPost/Delete");
            request.onload = function () {
                if (request.status === 200) {
                    window.location = "/";
                }
            }
            request.send(form);
        },
        playAudio: function (url) {
            if (this.audioUrl !== url) {
                this.audioPlayer.pause();
                this.audioUrl = url
                this.audioPlayer.src = url;
                this.audioPlayer.play();
                this.paused = false;
            } else {
                if (!this.audioPlayer.paused) {
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
        insertHeading: function (headingN) {
            let markdown = "";
            let textarea = vueInstance.$refs.commentTextareaRef;
            switch (headingN) {
                case 1:
                    markdown = "# Header 1";
                    break;
                case 2:
                    markdown = "## Header 2";
                    break;
                case 3:
                    markdown = "### Header 3";
                    break;
                case 4:
                    markdown = "#### Header 4";
                    break;
                case 5:
                    markdown = "##### Header 5";
                    break;
                case 6:
                    markdown = "###### Header 6";
                    break;
            }

            if (textarea.selectionStart === textarea.selectionEnd) {
                let textBefore = "";
                let textAfter = "";
                for (let i = 0; i < textarea.selectionStart; i++) {
                    textBefore += this.commentTextarea.charAt(i);
                }
                for (let i = textarea.selectionStart; i < this.commentTextarea.length; i++) {
                    textAfter += this.commentTextarea.charAt(i);
                }
                if (textBefore.length !== 0) {
                    this.commentTextarea = textBefore + "\n" + markdown + textAfter;
                    textarea.selectionStart = textBefore.length + markdown.length + 1;
                    textarea.selectionEnd = textBefore.length + markdown.length + 1;
                    console.log(textBefore.length + markdown.length + 1);
                } else {
                    this.commentTextarea = textBefore + markdown + textAfter;
                    textarea.selectionStart = textBefore.length + markdown.length;
                    textarea.selectionEnd = textBefore.length + markdown.length;
                    console.log(textBefore.length + markdown.length);
                }
                textarea.focus();
            } else {

            }
        },
        surroundTextWith: function (tag) {
            let markdownSurrounder = "";
            let textarea = vueInstance.$refs.commentTextareaRef;
            switch (tag) {
                case "bold":
                    markdownSurrounder = "**";
                    break;
                case "italics":
                    markdownSurrounder = "*";
                    break;
            }

            console.log(textarea);


            if (textarea.selectionStart === textarea.selectionEnd) {
                let textBefore = "";
                let textAfter = "";
                for (let i = 0; i < textarea.selectionStart; i++) {
                    textBefore += this.commentTextarea.charAt(i);
                }
                for (let i = textarea.selectionStart; i < this.commentTextarea.length; i++) {
                    textAfter += this.commentTextarea.charAt(i);
                }

                // cursor is not inside any pair of "*"
                if (!(textBefore.charAt(textBefore.length - 1) === "*" && textAfter.charAt(0) === "*")) {
                    if (textBefore.charAt(textBefore.length - 1) === "*") {
                        textBefore += " ";
                    }

                    this.commentTextarea = textBefore + markdownSurrounder + markdownSurrounder + textAfter;
                    this.selectionStart = textBefore.length + markdownSurrounder.length;
                    this.selectionEnd = textBefore.length + markdownSurrounder.length;

                } else {
                    if (tag === "italics") {
                        // this means that it is has only bold, so it can add another * for italics
                        if (textBefore.substring(textBefore.length - 4) !== "***"
                            && textBefore.substring(textBefore.length - 3) === "**") {
                            this.commentTextarea = textBefore + markdownSurrounder + markdownSurrounder + textAfter;
                            textarea.selectionStart = textBefore.length + markdownSurrounder.length;
                            textarea.selectionEnd = textBefore.length + markdownSurrounder.length;
                            console.log(textBefore.length + markdownSurrounder.length);
                        }
                    } else {
                        if (textBefore.substring(textBefore.length - 4) !== "***"
                            && textBefore.substring(textBefore.length - 3) !== "**") {
                            this.commentTextarea = textBefore + markdownSurrounder + markdownSurrounder + textAfter;
                            this.selectionStart = textBefore.length + markdownSurrounder.length;
                            this.selectionEnd = textBefore.length + markdownSurrounder.length;
                            console.log(textBefore.length + markdownSurrounder.length);
                        }
                    }
                }
                textarea.focus();


            } else {
                let textBefore = "";
                let textAfter = "";
                let selectedText = "";
                for (let i = 0; i < textarea.selectionStart; i++) {
                    textBefore += this.commentTextarea.charAt(i);
                }
                for (let i = textarea.selectionEnd; i < this.commentTextarea.length; i++) {
                    textAfter += this.commentTextarea.charAt(i);
                }
                for (let i = textarea.selectionStart; i < textarea.selectionEnd; i++) {
                    selectedText += this.commentTextarea.charAt(i);
                }
                if (textBefore.substring(textBefore.length - (markdownSurrounder + 1)) !== markdownSurrounder) {
                    this.commentTextarea = textBefore + markdownSurrounder + selectedText + markdownSurrounder + textAfter;
                }
            }
        },
        fetchPost: fetchPost,
        getComments: fetchComments,
        requestMicrophone: function () {
            let audioStreamPromise = navigator.mediaDevices.getUserMedia({audio: true, video: false})

            audioStreamPromise.then((stream) => {
                this.audio.recorder.mediaStream = stream;
            });
        },
        startRecording: function () {
            this.audio.consolidated = false;
            this.audio.timeInSeconds = 0;
            this.audio.recorder.mediaRecorder = new MediaRecorder(this.audio.recorder.mediaStream, {
                mimeType: 'audio/webm; codecs=opus',
                bitsPerSecond: 128000
            });
            this.audio.recorder.mediaRecorder.start();
            let globalThis = this;

            // start interval that will be updating the progress bar.
            // stop this interval when recording ends
            let interval = setInterval(function () {
                globalThis.audio.timeInSeconds += 1;
            }, 1000);

            // This timer is what limits the duration up to 2 minutes (120 seconds)
            let timer = setTimeout(function () {
                globalThis.stopRecording();
                clearInterval(interval);
            }, 120 * 1000);

            this.audio.recorder.mediaRecorder.ondataavailable = function (e) {
                globalThis.audio.recorder.audioData.push(e.data);
                console.log("Data pushed...");
            }
            this.audio.recorder.mediaRecorder.onstop = function () {
                clearTimeout(timer);
                clearInterval(interval);
                globalThis.audio.recorder.resultantBlob =
                    new Blob(globalThis.audio.recorder.audioData, {'type': 'audio/webm; codecs=opus'});
            }
            this.audio.recording = true;
        },
        stopRecording: function () {
            this.audio.recorder.mediaRecorder.stop();
            this.audio.recording = false;
            this.audio.recorded = true;
        },
        playRecord: function () {
            this.audio.recorder.audioPlayer.src = window.URL.createObjectURL(this.audio.recorder.resultantBlob);
            this.audio.recorder.audioPlayer.volume = 1;
            this.audio.recorder.audioPlayer.play().then(function () {
                console.log("Playing recorded audio");
            }).catch(function (error) {
                console.error("Can't play recorded audio: " + error);
            });
        },
        resetRecording: function () {
            this.audio.recording = false;
            this.audio.recorded = false;
            this.audio.recorder.resultantBlob = null;
            this.audio.recorder.audioData = [];
            this.audio.recorder.audioPlayer.pause();
            this.audio.consolidated = false;
            this.audio.timeInSeconds = 0;
            this.uploadingData.progress = 0;
            this.uploadingData.uploading = false;
        },
        consolidate: function () {
            this.audio.consolidated = true;
            $('#add-audio-modal').modal('hide');
        },
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
            globalThis.getComments();
        });

        this.audioPlayer.onended = function () {
            globalThis.audioUrl = "";
        }

        thisVueInstance = this;
    }
})