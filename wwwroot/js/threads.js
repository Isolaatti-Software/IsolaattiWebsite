// Threads page
(function () {
    let form = new FormData();
    form.append("sessionToken", sessionToken);
    form.append("postId", threadToReadId);

    let request = new XMLHttpRequest();
    request.open("POST", "/api/GetPost");
    request.onload = () => {
        if (request.status === 200) {
            //return 
            let vueInstance = new Vue({
                el: "#vue-container",
                data: {
                    userId: userData.id,
                    post: JSON.parse(request.responseText),
                    commentBoxFullSize: false,
                    commentTextarea: "",
                    selectionStart: 0,
                    selectionEnd: 0,
                    comments: [],
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
                    postLinkToShare: ""
                },
                computed: {
                    makeCommentButtonDisabled: function() {
                        return this.commentTextarea === "" || this.commentTextarea === null
                    },
                    addAudioButtonText: function() {
                        return (this.audio.consolidated) ? "Audio recorded" : "Record audio";
                    }
                },
                methods: {
                    likePost: function (post) {
                        let formData = new FormData();
                        formData.append("sessionToken", sessionToken);
                        formData.append("postId", post.id);

                        let request = new XMLHttpRequest();
                        request.open("POST", "/api/Likes/LikePost");
                        request.onreadystatechange = () => {
                            if (request.readyState === XMLHttpRequest.DONE) {
                                if (request.status === 200) {
                                    let modifiedPost = JSON.parse(request.responseText);
                                    this.post = modifiedPost;
                                    // let index = vueContainer.posts.findIndex(post => post.id === modifiedPost.id);
                                    // Vue.set(vueContainer.posts, index, modifiedPost);
                                }
                            }
                        }
                        request.send(formData);
                    },
                    unLikePost: function (post) {
                        let formData = new FormData();
                        formData.append("sessionToken", sessionToken);
                        formData.append("postId", post.id);

                        let request = new XMLHttpRequest();
                        request.open("POST", "/api/Likes/UnLikePost");
                        request.onreadystatechange = () => {
                            if (request.readyState === XMLHttpRequest.DONE) {
                                if (request.status === 200) {
                                    let modifiedPost = JSON.parse(request.responseText);
                                    this.post = modifiedPost;
                                    // let index = vueContainer.posts.findIndex(post => post.id === modifiedPost.id);
                                    // Vue.set(vueContainer.posts, index, modifiedPost);
                                }
                            }
                        }
                        request.send(formData);
                    },
                    makeComment: function () {
                        let globalThis = this;
                        let form = new FormData();
                        form.append("sessionToken", sessionToken);
                        form.append("postId",this.post.id);
                        form.append("content", this.commentTextarea);
                        
                        // there is an audio to upload
                        if(this.audio.consolidated) {
                            let storageRef = storage.ref();
                            let date = new Date();
                            
                            let uploadTask = storageRef
                                .child(`audio_comments/${userData.id}/${date.toDateString()}-${date.getHours()}-${date.getMinutes()}-${date.getSeconds()}_audio.webm`);
                            uploadTask.put(this.audio.recorder.resultantBlob).then(function(snapshot){
                                uploadTask.getDownloadURL().then(function(url){
                                    form.append("audioUrl",url);
                                    sendCommentRequest();
                                });
                            });
                        } else {
                            sendCommentRequest();
                        }
                        
                        function sendCommentRequest() {
                            let request = new XMLHttpRequest();
                            request.open("POST","/api/MakeComment");
                            request.onload = () => {
                                if(request.status === 200) {
                                    console.log("Se hizo el comentario!!");
                                    console.log(JSON.parse(request.responseText));
                                    globalThis.commentTextarea = "";
                                    globalThis.getComments();
                                    if(globalThis.commentBoxFullSize){
                                        globalThis.toggleCommentBoxFullSize();
                                    }
                                }
                            }
                            request.send(form);
                        }
                    },
                    compileMarkdown: function(raw) {
                        return marked(raw);
                    },
                    copyToClipboard: function(relativeUrl) {
                        let absoluteUrl = `${window.location.protocol}//${window.location.host}${relativeUrl}`;
                        navigator.clipboard.writeText(absoluteUrl).then(function() {
                            alert("Se copió el texto al portapapeles");
                        });
                    },
                    deletePost: function(postId) {
                        if(!confirm("Are your sure you want to delete this post?")) {
                            return;
                        }
                        let globalThis = this;
                        let form = new FormData();
                        form.append("sessionToken", sessionToken);
                        form.append("postId", postId);

                        let request = new XMLHttpRequest();
                        request.open("POST", "/api/EditPost/Delete");
                        request.onload = function() {
                            if(request.status === 200) {
                                window.location = "/";
                            }
                        }
                        request.send(form);
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
                    toggleCommentBoxFullSize: function() {
                        let button = this.$refs.buttonToggleComSize;
                        if(!this.commentBoxFullSize) {

                            this.$refs.commentBox.style.animation = "expandCommentBox";
                            this.$refs.commentBox.style.animationDuration = "0.4s"
                            this.$refs.commentBox.style.animationFillMode = "forwards";
                            this.commentBoxFullSize = true;
                            button.innerHTML = '<i class="fas fa-arrow-down"></i>';
                        } else {

                            this.$refs.commentBox.style.animation = "shrinkCommentBox";
                            this.$refs.commentBox.style.animationDuration = "0.4s";
                            this.$refs.commentBox.style.animationFillMode = "forwards";
                            button.innerHTML = '<i class="fas fa-arrow-up"></i>';
                            this.commentBoxFullSize = false;
                        }
                    },
                    insertHeading: function(headingN) {
                        let markdown = "";
                        let textarea = vueInstance.$refs.commentTextareaRef;
                        switch(headingN) {
                            case 1: markdown = "# Header 1"; break;
                            case 2: markdown = "## Header 2"; break;
                            case 3: markdown = "### Header 3"; break;
                            case 4: markdown = "#### Header 4"; break;
                            case 5: markdown = "##### Header 5"; break;
                            case 6: markdown = "###### Header 6"; break;
                        }

                        if(textarea.selectionStart === textarea.selectionEnd) {
                            let textBefore = "";
                            let textAfter = "";
                            for(let i = 0; i < textarea.selectionStart; i++) {
                                textBefore += this.commentTextarea.charAt(i);
                            }
                            for(let i = textarea.selectionStart; i < this.commentTextarea.length; i++ ) {
                                textAfter += this.commentTextarea.charAt(i);
                            }
                            if(textBefore.length !== 0) {
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
                    surroundTextWith: function(tag) {
                        let markdownSurrounder = "";
                        let textarea = vueInstance.$refs.commentTextareaRef;
                        switch(tag) {
                            case "bold": markdownSurrounder = "**"; break;
                            case "italics": markdownSurrounder = "*"; break;
                        }

                        console.log(textarea);
                        

                        if(textarea.selectionStart === textarea.selectionEnd) {
                            let textBefore = "";
                            let textAfter = "";
                            for(let i = 0; i < textarea.selectionStart; i++) {
                                textBefore += this.commentTextarea.charAt(i);
                            }
                            for(let i = textarea.selectionStart; i < this.commentTextarea.length; i++ ) {
                                textAfter += this.commentTextarea.charAt(i);
                            }

                            // cursor is not inside any pair of "*"
                            if(!(textBefore.charAt(textBefore.length - 1) === "*" && textAfter.charAt(0) === "*")) {
                                if(textBefore.charAt(textBefore.length - 1) === "*") {
                                    textBefore += " ";
                                }

                                this.commentTextarea = textBefore + markdownSurrounder + markdownSurrounder + textAfter;
                                this.selectionStart = textBefore.length + markdownSurrounder.length;
                                this.selectionEnd = textBefore.length + markdownSurrounder.length;

                            } else {
                                if(tag === "italics"){
                                    // this means that it is has only bold, so it can add another * for italics
                                    if(textBefore.substring(textBefore.length - 4) !== "***"
                                        && textBefore.substring(textBefore.length - 3) === "**"){
                                        this.commentTextarea = textBefore + markdownSurrounder + markdownSurrounder + textAfter;
                                        textarea.selectionStart = textBefore.length + markdownSurrounder.length;
                                        textarea.selectionEnd = textBefore.length + markdownSurrounder.length;
                                        console.log(textBefore.length + markdownSurrounder.length);
                                    }
                                } else {
                                    if(textBefore.substring(textBefore.length - 4) !== "***"
                                        && textBefore.substring(textBefore.length - 3) !== "**"){
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
                            for(let i = 0; i < textarea.selectionStart; i++) {
                                textBefore += this.commentTextarea.charAt(i);
                            }
                            for(let i = textarea.selectionEnd; i < this.commentTextarea.length; i++ ) {
                                textAfter += this.commentTextarea.charAt(i);
                            }
                            for(let i = textarea.selectionStart; i < textarea.selectionEnd; i++) {
                                selectedText += this.commentTextarea.charAt(i);
                            }
                            if(textBefore.substring(textBefore.length - (markdownSurrounder + 1)) !== markdownSurrounder) {
                                this.commentTextarea = textBefore + markdownSurrounder + selectedText + markdownSurrounder + textAfter;
                            }
                        }
                    },
                    getComments: function() {
                        let form = new FormData();
                        form.append("sessionToken", sessionToken);
                        form.append("postId", this.post.id);
                        
                        let request = new XMLHttpRequest();
                        request.open("POST", "/api/GetPost/Comments");
                        request.onload = () => {
                            if(request.status === 200) {
                                this.comments = JSON.parse(request.responseText);
                                window.scrollTo(0, document.body.scrollHeight);
                            }
                        }
                        request.send(form);
                    },
                    // playAudio: function(url) {
                    //     if(this.audioUrl !== url) {
                    //         this.audioPlayer.pause();
                    //         this.audioUrl = url
                    //         this.audioPlayer.src = url;
                    //         this.audioPlayer.play();
                    //         this.paused = false;
                    //     } else {
                    //         if(!this.audioPlayer.paused) {
                    //             this.audioPlayer.pause()
                    //             this.paused = true;
                    //             this.playing = false;
                    //         } else {
                    //             this.audioPlayer.play();
                    //             this.playing = true;
                    //             this.paused = false;
                    //         }
                    //     }
                    //     this.playing = true;
                    // },
                    requestMicrophone: function() {
                        let audioStreamPromise = navigator.mediaDevices.getUserMedia({audio:true, video:false})

                        audioStreamPromise.then((stream) => {
                            this.audio.recorder.mediaStream = stream;
                        });
                    },
                    startRecording: function() {
                        this.audio.consolidated = false;
                        this.audio.timeInSeconds = 0;
                        this.audio.recorder.mediaRecorder = new MediaRecorder(this.audio.recorder.mediaStream,{
                            mimeType: 'audio/webm; codecs=opus',
                            bitsPerSecond: 128000
                        });
                        this.audio.recorder.mediaRecorder.start();
                        let globalThis = this;

                        // start interval that will be updating the progress bar.
                        // stop this interval when recording ends
                        let interval = setInterval(function() {
                            globalThis.audio.timeInSeconds += 1;
                        },1000);

                        // This timer is what limits the duration up to 2 minutes (120 seconds)
                        let timer = setTimeout(function(){
                            globalThis.stopRecording();
                            clearInterval(interval);
                        }, 120*1000);

                        this.audio.recorder.mediaRecorder.ondataavailable = function(e) {
                            globalThis.audio.recorder.audioData.push(e.data);
                            console.log("Data pushed...");
                        }
                        this.audio.recorder.mediaRecorder.onstop = function() {
                            clearTimeout(timer);
                            clearInterval(interval);
                            globalThis.audio.recorder.resultantBlob =
                                new Blob(globalThis.audio.recorder.audioData, {'type':'audio/webm; codecs=opus'});
                        }
                        this.audio.recording = true;
                    },
                    stopRecording: function() {
                        this.audio.recorder.mediaRecorder.stop();
                        this.audio.recording = false;
                        this.audio.recorded = true;
                    },
                    playRecord: function() {
                        this.audio.recorder.audioPlayer.src = window.URL.createObjectURL(this.audio.recorder.resultantBlob);
                        this.audio.recorder.audioPlayer.volume = 1;
                        this.audio.recorder.audioPlayer.play().then(function() {
                            console.log("Playing recorded audio");
                        }).catch(function(error) {
                            console.error("Can't play recorded audio: " + error);
                        });
                    },
                    resetRecording: function() {
                        this.audio.recording = false;
                        this.audio.recorded = false;
                        this.audio.recorder.resultantBlob = null;
                        this.audio.recorder.audioData = [];
                        this.audio.recorder.audioPlayer.pause();
                        this.audio.consolidated = false;
                        this.audio.timeInSeconds = 0;
                    },
                    consolidate: function() {
                        this.audio.consolidated = true;
                        $('#add-audio-modal').modal('hide');
                    },
                    getPostStyle: function(themeDefinitionJson) {
                        if(themeDefinitionJson === null)
                            return "";
                        const theme = JSON.parse(themeDefinitionJson);
                        return `color: ${theme.fontColor};
                        background-color: ${theme.backgroundColor};
                        border: ${theme.border.size} ${theme.border.type} ${theme.border.color}`;
                    }
                },
                mounted: function() {
                    this.$nextTick(function() {
                        this.getComments();
                    });
                    let globalThis = this;
                    this.audioPlayer.onended = function() {
                        globalThis.audioUrl = "";
                    }
                }
            })
        }
    }
    request.send(form);
})();