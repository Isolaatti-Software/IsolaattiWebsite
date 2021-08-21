
let markdownInput = document.getElementById("raw-post-markdown");
let audioContext = new AudioContext();

const vue = new Vue({
    el: "#vue-container",
    data: {
        input: "",
        selectionStart: 0,
        selectionEnd: 0,
        privacy: "2",
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
        theme: {
            fontColor: "#000",
            backgroundColor: "#FFFFFF",
            gradient: "false",
            border: {
                color: "#FFFFFF",
                type: "solid",
                size: "0",
                radius: "5px"
            },
            background: {
                type: "linear",
                colors: ["#FFFFFF","#30098EE5"],
                direction: "0"
            }
        },
        editing: editExistingPost,
        existingAudioUrl: null,
        editingPostId: editPostId
    },
    computed: {
        compiledMarkdown: function() {
            return marked(this.input, { sanitize: true});
        },
        postButtonDisabled: function() {
            return this.input === null || this.input === "";
        },
        addAudioButtonText: function() {
            return (this.audio.consolidated) ? "Audio recorded" : "Record audio";
        },
        postThemeCSSStyle: function() {
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
            if(this.theme.gradient === "true"){
                backgroundProperty = this.theme.background.type === 
                "linear" ? 
                    `linear-gradient(${this.theme.background.direction}deg, ${returnColorsAsString(this.theme.background.colors)})` : 
                    `radial-gradient(${returnColorsAsString(this.theme.background.colors)})`;
            } else {
                backgroundProperty = this.theme.backgroundColor;
            }
            
           return `color: ${this.theme.fontColor};
                background: ${backgroundProperty};
                border: ${this.theme.border.size}px ${this.theme.border.type} ${this.theme.border.color};
                border-radius: ${this.theme.border.radius}px;
                max-height: calc(100vh - 150px); 
                overflow: auto; cursor: not-allowed;`;
        }
    },
    methods: {
        update: _.debounce(function(e){
            this.input = e.target.value
        }, 300),
        insertHeading: function(headingN) {
            let markdown = "";
            switch(headingN) {
                case 1: markdown = "# Header 1"; break;
                case 2: markdown = "## Header 2"; break;
                case 3: markdown = "### Header 3"; break;
                case 4: markdown = "#### Header 4"; break;
                case 5: markdown = "##### Header 5"; break;
                case 6: markdown = "###### Header 6"; break;
            }
            
            if(this.$refs.markdownTextarea.selectionStart === this.$refs.markdownTextarea.selectionEnd) {
                let textBefore = "";
                let textAfter = "";
                for(let i = 0; i < this.$refs.markdownTextarea.selectionStart; i++) {
                    textBefore += this.input.charAt(i);
                }
                for(let i = this.$refs.markdownTextarea.selectionStart; i < this.input.length; i++ ) {
                    textAfter += this.input.charAt(i);
                }
                if(textBefore.length !== 0) {
                    this.input = textBefore + "\n" + markdown + textAfter;
                    this.selectionStart = textBefore.length + markdown.length + 1;
                    this.selectionEnd = textBefore.length + markdown.length + 1;
                } else {
                    this.input = textBefore + markdown + textAfter;
                    this.selectionStart = textBefore.length + markdown.length;
                    this.selectionEnd = textBefore.length + markdown.length;
                }
                this.$refs.markdownTextarea.focus();
            } else {
                
            }
            
        },
        surroundTextWith: function(tag) {
            let markdownSurrounder = "";
            switch(tag) {
                case "bold": markdownSurrounder = "**"; break;
                case "italics": markdownSurrounder = "*"; break;
            }
            
            let textarea = this.$refs.markdownTextarea;
            
            if(textarea.selectionStart === textarea.selectionEnd) {
                let textBefore = "";
                let textAfter = "";
                for(let i = 0; i < textarea.selectionStart; i++) {
                    textBefore += this.input.charAt(i);
                }
                for(let i = textarea.selectionStart; i < this.input.length; i++ ) {
                    textAfter += this.input.charAt(i);
                }
                
                // cursor is not inside any pair of "*"
                if(!(textBefore.charAt(textBefore.length - 1) === "*" && textAfter.charAt(0) === "*")) {
                    if(textBefore.charAt(textBefore.length - 1) === "*") {
                        textBefore += " ";
                    }
                    
                    this.input = textBefore + markdownSurrounder + markdownSurrounder + textAfter;
                    this.selectionStart = textBefore.length + markdownSurrounder.length;
                    this.selectionEnd = textBefore.length + markdownSurrounder.length;
                    
                } else {
                    if(tag === "italics"){
                        // this means that it is has only bold, so it can add another * for italics
                        if(textBefore.substring(textBefore.length - 4) !== "***" 
                            && textBefore.substring(textBefore.length - 3) === "**"){
                            this.input = textBefore + markdownSurrounder + markdownSurrounder + textAfter;
                            this.selectionStart = textBefore.length + markdownSurrounder.length;
                            this.selectionEnd = textBefore.length + markdownSurrounder.length;
                        }
                    } else {
                        if(textBefore.substring(textBefore.length - 4) !== "***"
                            && textBefore.substring(textBefore.length - 3) !== "**"){
                            this.input = textBefore + markdownSurrounder + markdownSurrounder + textAfter;
                            this.selectionStart = textBefore.length + markdownSurrounder.length;
                            this.selectionEnd = textBefore.length + markdownSurrounder.length;
                        }
                    }
                }
                textarea.focus();
                
                
            } else {
                let textBefore = "";
                let textAfter = "";
                let selectedText = "";
                for(let i = 0; i < textarea.selectionStart; i++) {
                    textBefore += this.input.charAt(i);
                }
                for(let i = textarea.selectionEnd; i < this.input.length; i++ ) {
                    textAfter += this.input.charAt(i);
                }
                for(let i = textarea.selectionStart; i < textarea.selectionEnd; i++) {
                    selectedText += this.input.charAt(i);
                }
                if(textBefore.substring(textBefore.length - (markdownSurrounder + 1)) !== markdownSurrounder) {
                    this.input = textBefore + markdownSurrounder + selectedText + markdownSurrounder + textAfter;
                }
            }
        },
        post: function(event) {
            event.target.disabled = true;
            event.target.innerHTML = '<div class="spinner-border text-light spinner-border-sm" role="status"><span class="sr-only">Loading...</span></div>' + event.target.innerHTML;
            let form = new FormData();
            form.append("sessionToken", sessionToken);
            form.append("privacy", this.privacy);
            form.append("content", this.input);
            form.append("themeJson", JSON.stringify(this.theme));
            if(this.editing) {
                form.append("postId", this.editingPostId)
            }
            // this means that there is a blob to upload
            if(this.audio.consolidated && this.existingAudioUrl === null) {
                let storageRef = storage.ref();
                let date = new Date();
                
                // the resource has this pattern: /audio_posts/{userId}/{name of file}
                // where name of file follows this rule: {date}-{hours}-{minutes}-{seconds}_audio.webm
                let uploadTask = storageRef
                        .child(`audio_posts/${userData.id}/${date.toDateString()}-${date.getHours()}-${date.getMinutes()}-${date.getSeconds()}_audio.webm`);
                uploadTask.put(this.audio.recorder.resultantBlob).then(function(snapshot) {
                    uploadTask.getDownloadURL().then(function(url) {
                        form.append("audioUrl",url);
                        sendPostRequest();
                    });
                });
            } else {
                if(this.existingAudioUrl !== null) {
                    form.append("audioUrl", this.existingAudioUrl);
                }
                sendPostRequest();
            }
            function sendPostRequest() {
                let request = new XMLHttpRequest();
                request.open("POST", "/api/MakePost");
                request.onreadystatechange = () => {
                    if (request.readyState === XMLHttpRequest.DONE) {
                        switch (request.status) {
                            case 200: window.location = "/"; break;
                            case 404: alert(request.responseText); break;
                            case 401: alert(request.responseText); break;
                            case 500: alert(request.responseText); break;
                        }
                    }
                };
                request.send(form);
            }

            
        },
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
            if(this.editing) {
                this.existingAudioUrl = null;
            }
        },
        consolidate: function() {
            this.audio.consolidated = true;
            
            // makes this null because a new audio will be uploaded
            this.existingAudioUrl = null;
            
            $('#add-audio-modal').modal('hide');
        },
        save: function() {
            
        },
        addColor: function() {
            this.theme.background.colors.push("#FFFFFF");
        },
        removeColor: function(index) {
            this.theme.background.colors.splice(index, 1);
        }
    },
    mounted: function() {
        let globalThis = this;
        this.$nextTick(function() {
            if(editExistingPost) {
                
                // get the post that user wants to edit
                let form = new FormData();
                form.append("sessionToken", sessionToken);
                form.append("postId", editPostId);
                
                let request = new XMLHttpRequest();
                request.open("POST", "/api/GetPost");
                request.onload = function() {
                    let post = JSON.parse(request.responseText);
                    globalThis.input = post.textContent;
                    globalThis.privacy = post.privacy;
                    if(post.audioAttachedUrl !== null) {
                        // set state variables
                        globalThis.audio.consolidated = true;
                        globalThis.audio.recorded = true;
                        globalThis.existingAudioUrl = post.audioAttachedUrl;
                        // download audio an put it into a blob
                        fetch(post.audioAttachedUrl)
                            .then(res => res.blob())
                            .then(blob => globalThis.audio.recorder.resultantBlob = blob)
                    }
                    let theme = JSON.parse(post.themeJson);

                    if(theme.background === undefined) {
                        theme.background = {
                            type: "linear",
                            colors: ["#FFFFFF","#30098EE5"],
                            direction: "0"
                        }
                    }
                    
                    if(theme.gradient === undefined) {
                        theme.gradient = "false";
                    }
                    
                    globalThis.theme = theme; // this is fine
                    // globalThis.theme.fontColor = theme.fontColor;
                    // globalThis.theme.backgroundColor = theme.backgroundColor;
                    // globalThis.theme.border.size = theme.border.size;
                    // globalThis.theme.border.type = theme.border.type;
                    // globalThis.theme.border.color = theme.border.color;
                    // globalThis.theme.border.radius = theme.border.radius;
                    
                    
                    
                }
                request.send(form);
            }
        });
    }
});