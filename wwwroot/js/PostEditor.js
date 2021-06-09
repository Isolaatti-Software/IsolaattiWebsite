
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
        }
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
        post: function() {
            let form = new FormData();
            form.append("sessionToken", sessionToken);
            form.append("privacy", this.privacy);
            form.append("content", this.input);
            // this means that there is a blob to upload
            if(this.audio.consolidated) {
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
        },
        consolidate: function() {
            this.audio.consolidated = true;
            $('#add-audio-modal').modal('hide');
        }
    }
});