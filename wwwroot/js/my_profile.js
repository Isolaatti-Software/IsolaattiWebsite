/*
* Isolaatti project
* Erik Cavazos, 2020, 2021
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
* 
* This file should be placed in the Pages/Profile.cshtml
*/

function deletePost(postId, onComplete, onError) {
    let form = new FormData();
    form.append("sessionToken", sessionToken);
    form.append("postId",postId);

    let request = new XMLHttpRequest();
    request.open("POST", "/api/EditPost/Delete");
    request.onreadystatechange = () => {
        if(request.readyState === XMLHttpRequest.DONE) {
            switch(request.status) {
                case 200 : onComplete({
                    statusCode: 200,
                    serverResponse: request.responseText
                }); break;
                case 404 : onError({
                    statusCode: 404,
                    serverResponse: request.responseText
                }); break;
                case 401 : onError({
                    statusCode: 401,
                    serverResponse: request.responseText
                }); break;
                case 500 : onError({
                    statusCode: 500,
                    serverResponse: "Server error. Report this to developer"
                }); break;
                default : onError({
                    statusCode: request.status,
                    serverResponse: request.responseText
                }); break;
            }
        }
    }

    request.send(form);
}

function editPost(postId, newContent, onComplete, onError) {
    let form = new FormData();
    form.append("sessionToken", sessionToken);
    form.append("postId", postId);
    form.append("newContent", newContent);

    let request = new XMLHttpRequest();
    request.open("POST", "/api/EditPost/TextContent");
    request.onreadystatechange = () => {
        if(request.readyState === XMLHttpRequest.DONE) {
            switch(request.status) {
                case 200 : onComplete({
                    statusCode: 200,
                    serverResponse: JSON.parse(request.responseText)
                }, JSON.parse(request.responseText)); break;
                case 404 : onError({
                    statusCode: 404,
                    serverResponse: request.responseText
                }); break;
                case 401 : onError({
                    statusCode: 401,
                    serverResponse: request.responseText
                }); break;
                case 500 : onError({
                    statusCode: 500,
                    serverResponse: "Server error. Report this to developer"
                }); break;
                default : onError({
                    statusCode: request.status,
                    serverResponse: request.responseText
                }); break;
            }
        }
    }
    request.send(form);
}

function changePostPrivacy(postId, privacyNumber, onComplete, onError) {
    let form = new FormData();
    form.append("sessionToken", sessionToken);
    form.append("postId",postId);
    form.append("privacyNumber", privacyNumber);

    let request = new XMLHttpRequest();
    request.open("POST", "/api/EditPost/ChangePrivacy");
    request.onreadystatechange = () => {
        if(request.readyState === XMLHttpRequest.DONE) {
            switch(request.status) {
                case 200 : onComplete({
                    statusCode: 200,
                    serverResponse: JSON.parse(request.responseText)
                }); break;
                case 404 : onError({
                    statusCode: 404,
                    serverResponse: JSON.parse(request.responseText)
                }); break;
                case 401 : onError({
                    statusCode: 401,
                    serverResponse: JSON.parse(request.responseText)
                }); break;
                case 500 : onError({
                    statusCode: 500,
                    serverResponse: "Server error. Report this to developer"
                }); break;
                default : onError({
                    statusCode: request.status,
                    serverResponse: request.responseText
                }); break;
            }
        }
    }

    request.send(form);
}

function copyLinkToClipboard(button,link){
    navigator.clipboard.writeText(link).then(function() {
        button.innerHTML = '<i class="fas fa-check"></i>';
    });
}

function deleteShare(uid) {
    let formData = new FormData();
    formData.append("sessionToken", sessionToken);
    formData.append("uid", uid);

    let request = new XMLHttpRequest();
    request.open("POST", "/api/UnshareSong");
    request.send(formData);
    request.onreadystatechange = function() {
        if(request.readyState === XMLHttpRequest.DONE) {
            alert("Share deleted");
            window.location.reload();
        }
    }
}

function getPosts(onComplete, onError) {
    let formData = new FormData();
    formData.append("sessionToken", sessionToken);
    
    let request = new XMLHttpRequest();
    request.open("POST", "/api/GetProfile/GetPosts");
    request.onreadystatechange = () => {
        if(request.readyState === XMLHttpRequest.DONE) {
            switch(request.status) {
                case 200: onComplete(JSON.parse(request.responseText)); break;
                default: onError(JSON.parse(request.responseText)); break;
            }
        }
    }
    request.send(formData);
}

const profilePictureLoadFormElement = document.getElementById("profilePictureLoadFormElement");
const canvas = document.getElementById("previewProfilePicture");
const canvasContext = canvas.getContext('2d');
const previewProfilePictureImage = new Image();

const uploadProfilePhoto = document.getElementById("uploadProfilePhoto");

uploadProfilePhoto.addEventListener("click", function() {
    if(previewProfilePictureImage.src == null) {
        return;
    }
    let form = new FormData();
    canvas.toBlob(function(blob) {
        form.append("file",blob);
        form.append("sessionToken", sessionToken);

        let request = new XMLHttpRequest();
        request.open("POST","/api/EditProfile/UpdatePhoto");
        request.onload = function() {
            if(request.status === 200) {
                console.log(request.responseText);
                window.location = "/MyProfile";
            }
        }
        request.send(form); 
    });

});

previewProfilePictureImage.addEventListener("load", function() {
    uploadProfilePhoto.disabled = false;
    // horizontal
    if(previewProfilePictureImage.width > previewProfilePictureImage.height) {
        let constant = previewProfilePictureImage.height / 120;
        let margin = (120 - (previewProfilePictureImage.width/constant))/2;
        canvasContext.drawImage(previewProfilePictureImage, margin, 0, previewProfilePictureImage.width/constant,previewProfilePictureImage.height/constant);
    }

    // vertical
    else if(previewProfilePictureImage.width < previewProfilePictureImage.height) {
        let constant = previewProfilePictureImage.width / 120;
        let margin = (120 - (previewProfilePictureImage.height/constant))/2;
        canvasContext.drawImage(previewProfilePictureImage, 0, margin, previewProfilePictureImage.width/constant, previewProfilePictureImage.height/constant);
    } else {
        canvasContext.drawImage(previewProfilePictureImage, 0, 0, 120, 120);
    }
});


profilePictureLoadFormElement.addEventListener("input", function() {
    let file = profilePictureLoadFormElement.files[0];
    document.getElementById("fileNameProfilePic").innerHTML = file.name;
    let fileReader = new FileReader();
    fileReader.readAsDataURL(file);
    fileReader.onload = function() {
        previewProfilePictureImage.src = fileReader.result;
    };
});



// Use this self called function to define events
(function(){
    // "change password form" validation
    let newPasswordField = document.querySelector("#new_password_field");
    let newPasswordConfField = document.querySelector("#new_password_conf_field");
    let editProfileButton = document.getElementById("edit-profile-button");
    let editNameField = document.getElementById("edit-name-field");
    let editEmailField = document.getElementById("edit-email-field");
    
    // this value is used to know what post to modify or delete with modals
    let currentPostIdForModal = 0;
    document.addEventListener("DOMContentLoaded", function(){
        // Filling the fields in edit profile modal with current information
        editEmailField.value = userData.email;
        editNameField.value = userData.name;
        
        let posts;

        let vueContainer = new Vue({
            el: '#vue-container',
            data: {
                sessionToken: sessionToken,
                posts: [],
                selectedPostForModalsId: 0,
                textareaEditPost: "",
                privacyEditPost: "",
                audioPlayer: new Audio(),
                audioUrl: "",
                playing: false,
                paused: false,
                loading: true,
                postLinkToShare: "",
                commentsViewer: {
                    postId: 0,
                    comments: []
                },
                filterData: {
                    privacy: {
                        private: true,
                        isolaatti: true,
                        public: true
                    },
                    content: "all"
                },
                sortingData: {
                    ascending: "0"
                },
            },
            computed: {
                openThreadLink: function() {
                    return `/Hilo/${this.commentsViewer.postId}`;
                },
                filterAndSortedPosts: function() {
                    let filteredArray = this.posts.filter(value => {
                        let privacy = value.privacy;
                        let audioUrl = value.audioAttachedUrl;
                        
                        //let's filter first by audio availability
                        if(audioUrl === null && this.filterData.content === "withAudio") {
                            return false;
                        }
                        
                        if(audioUrl !== null && this.filterData.content === "withoutAudio") {
                            return false;
                        }
                        
                        // and then by privacy
                        switch(privacy) {
                            case 1: return this.filterData.privacy.private;
                            case 2: return this.filterData.privacy.isolaatti;
                            case 3: return this.filterData.privacy.public;
                        }
                    });
                    
                    if(this.sortingData.ascending === "1") {
                        return filteredArray.reverse();
                    }
                    return filteredArray;
                }
            },
            methods: {
                likePost: function(post) {
                    let formData = new FormData();
                    formData.append("sessionToken", sessionToken);
                    formData.append("postId", post.id);

                    let request = new XMLHttpRequest();
                    request.open("POST", "/api/Likes/LikePost");
                    request.onreadystatechange = () => {
                        if(request.readyState === XMLHttpRequest.DONE) {
                            if(request.status === 200) {
                                let modifiedPost = JSON.parse(request.responseText);
                                let index = vueContainer.posts.findIndex(post => post.id === modifiedPost.id);
                                Vue.set(vueContainer.posts,index,modifiedPost);
                            }
                        }
                    }
                    request.send(formData);
                },
                unLikePost: function(post) {
                    let formData = new FormData();
                    formData.append("sessionToken", sessionToken);
                    formData.append("postId", post.id);

                    let request = new XMLHttpRequest();
                    request.open("POST", "/api/Likes/UnLikePost");
                    request.onreadystatechange = () => {
                        if(request.readyState === XMLHttpRequest.DONE) {
                            if(request.status === 200) {
                                let modifiedPost = JSON.parse(request.responseText);
                                let index = vueContainer.posts.findIndex(post => post.id === modifiedPost.id);
                                Vue.set(vueContainer.posts,index,modifiedPost);
                            }
                        }
                    }
                    request.send(formData);
                },
                compileMarkdown: function(raw) {
                    if(raw === null) raw = "";
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
                    let globalThis = this;
                    let form = new FormData();
                    form.append("sessionToken", sessionToken);
                    form.append("postId", postId);

                    let request = new XMLHttpRequest();
                    request.open("POST", "/api/EditPost/Delete");
                    request.onload = function() {
                        if(request.status === 200) {
                            let postIndex = globalThis.posts.findIndex(value => value.id === postId);
                            globalThis.posts.splice(postIndex,1);
                        }
                    }
                    request.send(form);
                },
                refresh: function() {
                    postsInDOM = [];
                    this.posts = [];
                    getPosts((response) => {
                        vueContainer.posts = response;
                        vueContainer.loading = false;
                    }, (error) => {
                        alert("Error getting your posts");
                    });
                },
                viewComments: function(post) {
                    this.commentsViewer.postId = post.id;
                    this.getComments();
                },
                getComments: function() {
                    let form = new FormData();
                    form.append("sessionToken", sessionToken);
                    form.append("postId", this.commentsViewer.postId);

                    let request = new XMLHttpRequest();
                    request.open("POST", "/api/GetPost/Comments");
                    request.onload = () => {
                        if(request.status === 200) {
                            this.commentsViewer.comments = JSON.parse(request.responseText);
                        }
                    }
                    request.send(form);
                }                
            },
            mounted: function() {
                this.$nextTick(function() {
                    let globalThis = this;
                    this.audioPlayer.onended = function() {
                        // if it was playing the description it has to stop the photo rotating
                        if(globalThis.audioUrl === vueContainerForLeftBar.audioDescriptionUrl) {
                            vueContainerForLeftBar.stopRotatingProfilePicture();
                            vueContainerForLeftBar.playing = false;
                        }
                        globalThis.audioUrl = "";
                    }
                });
            }
        })

        // vue instance to manage profile audio description
        let vueContainerForLeftBar = new Vue({
            el: '#vue-container-for-left-bar',
            data: {
                sessionToken: sessionToken,
                userData: userData,
                audioDescriptionUrl: audioDescriptionUrl, // this value is defined on the page (rendered by te server)
                playing: false,
                paused: false,
                recorder: {
                    mediaStream: null,
                    audioBlob: null,
                    timeInSeconds: 0,
                    recording: false,
                    recorded: false,
                    mediaRecorder: null,
                    audioData: [],
                    consolidated: false,
                    timerReference: null,
                    stopTimeoutReference: null
                }
            },
            computed: {
                clockFormatTime: function() {
                    let truncatedSecs = Math.round(this.recorder.timeInSeconds);
                    let minutes = truncatedSecs / 60;
                    let seconds = truncatedSecs % 60;
                    if (seconds < 10) {
                        seconds = `0${seconds}`
                    }
                    return `${Math.trunc(minutes)}:${seconds}`;
                },
                audioDescriptionButtonInnerText: function() {
                    if(this.playing){
                        return '<i class="far fa-pause-circle"></i>'
                    } else if(this.paused){
                        return '<i class="far fa-play-circle"></i>'
                    } else {
                        return '<i class="far fa-play-circle"></i>'
                    }
                }
            },
            methods: {
                uploadAudioDescriptionAudio: function() {
                    const globalThis = this;
                    // first I need to upload the audio blob to google cloud storage
                    if(this.recorder.consolidated) {
                        let storageRef = storage.ref();
                        
                        let uploadTask = storageRef.child(`audio_descriptions/${this.userData.id}/audio.webm`);
                        uploadTask.put(this.recorder.audioBlob).then(function(s) {
                            
                            // then get the url and send it to the backend to store it on the database
                            uploadTask.getDownloadURL().then(function(url) {
                                
                                let form = new FormData();
                                form.append("sessionToken", globalThis.sessionToken);
                                form.append("url", url);
                                
                                let request = new XMLHttpRequest();
                                request.open("POST", "/api/EditProfile/UpdateAudioDescription");
                                request.onload = function() {
                                    if(request.status === 200) {
                                        window.location.reload();
                                    }
                                }
                                request.send(form);
                            })
                        })
                    }
                },
                startRotatingProfilePicture: function() {
                    if(this.paused) {
                        this.$refs.profilePhoto.getAnimations().forEach(function(element){
                            element.play();
                        });
                    } else {
                        this.$refs.profilePhoto.animate([
                            {transform: 'rotate(0deg)'},
                            {transform: 'rotate(360deg)'}
                        ], {
                            duration: 2800,
                            iterations: Infinity
                        });
                    }
                    
                },
                pauseRotatingProfilePicture: function() {
                  this.$refs.profilePhoto.getAnimations().forEach(function(element){
                      element.pause();
                  });  
                },
                stopRotatingProfilePicture: function() {
                    this.$refs.profilePhoto.getAnimations().forEach(function(element){element.cancel()})
                },
                requestMicrophone: function() {
                    let audioStreamPromise = navigator.mediaDevices.getUserMedia({audio:true, video:false})

                    audioStreamPromise.then((stream) => {
                        this.recorder.mediaStream = stream;
                    });
                },
                onEditProfileAudioDescriptionModal: function() {
                    if(this.recorder.mediaStream === null) {
                        this.requestMicrophone();
                    }
                },
                playAudioDescription: function() {
                    // I need to call this method that is on the other instance, so that I don't have to 
                    // create more than one audio player, and for instance the audio description plays 
                    // instead anything that is playing at the moment
                    
                    vueContainer.playAudio(this.audioDescriptionUrl);
                    
                    // this means user wants to pause
                    if(this.playing) {
                        this.pauseRotatingProfilePicture();
                        this.paused = true;
                        this.playing = false;
                    } else {
                        this.playing = true;
                        this.startRotatingProfilePicture();
                    }
                },
                startRecording: function() {
                    this.recorder.consolidated = false;
                    this.recorder.timeInSeconds = 0;
                    this.recorder.mediaRecorder = new MediaRecorder(this.recorder.mediaStream,{
                        mimeType: 'audio/webm; codecs:opus',
                        bitsPerSecond: 128000
                    });
                    this.recorder.mediaRecorder.start();
                    const globalThis = this;
                    
                    // this timer counts every second and updated UI to show time
                    this.recorder.timerReference = setInterval(function() {
                        globalThis.recorder.timeInSeconds++;
                    },1000);
                    
                    // recording will stop at 2 minutes
                    this.recorder.stopTimeoutReference = setTimeout(function() {
                        clearInterval(globalThis.recorder.timerReference);
                    },120000);
                    
                    
                    this.recorder.mediaRecorder.ondataavailable = function(e) {
                        globalThis.recorder.audioData.push(e.data);
                    }
                    
                    this.recorder.mediaRecorder.onstop = function() {
                        globalThis.recorder.audioBlob = new Blob(globalThis.recorder.audioData,
                            {'type':'audio/webm; codecs=opus'});
                        
                        clearInterval(globalThis.recorder.timerReference); // stops timer
                        clearTimeout(globalThis.recorder.stopTimeoutReference); // stops timeout that was set to execute to minutes after
                        
                        globalThis.recorder.consolidated =  true;
                    }
                    this.recorder.recording = true;
                },
                stopRecording: function() {
                    this.recorder.mediaRecorder.stop();
                    this.recorder.recording = false;
                    this.recorder.recorded = true;
                },
                playAudioDescriptionPreview: function() {
                    vueContainer.playAudio(window.URL.createObjectURL(this.recorder.audioBlob));
                },
                resetRecording: function() {
                    this.recorder.recording = false;
                    this.recorder.audioBlob = null;
                    this.recorder.audioData = [];
                    this.recorder.timeInSeconds = 0;
                    this.recorder.recorded = false;
                },
                
            },
            mounted: function() {
                const globalThis = this;
                this.$nextTick(function() {
                    
                    // I am sorry I have to do it this way, but it's easier than adding 
                    // events to the button that is outside vue.js
                    $('#modal-audio-description').on('show.bs.modal', function() {
                        globalThis.requestMicrophone();
                    });
                });
            }
        })
        
        getPosts((response) => {
            vueContainer.posts = response;
            vueContainer.loading = false;
        }, (error) => {
            alert("Ha sucedido un error obteniendo tus publicaciones.");
        });
    });

    newPasswordConfField.addEventListener("keyup", function() {
        if(newPasswordConfField.value !== newPasswordField.value) {
            newPasswordConfField.classList.add("is-invalid");
        } else {
            if(newPasswordConfField.classList.contains("is-invalid")) {
                newPasswordConfField.classList.remove("is-invalid");
            }
        }
    });
})();