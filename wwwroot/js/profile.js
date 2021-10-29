/*
* Isolaatti project
* Erik Cavazos, 2021
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
* 
* This file should be placed in the Pages/Profile.cshtml
*/

function getPosts(accountId, onComplete, onError) {
    let formData = new FormData();
    formData.append("sessionToken", sessionToken);
    formData.append("accountId", accountId);
    
    let request = new XMLHttpRequest();
    request.open("POST", "/api/Feed/GetUserPosts");
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

(function(){
    let followButton = document.querySelector("#followButton");
    followButton.addEventListener("click", function(){
        followButton.disabled = true;
        if(followingThisUser) {
            let formData = new FormData();
            formData.append("sessionToken", sessionToken);
            formData.append("userToUnfollowId", thisProfileId);
            
            let request = new XMLHttpRequest();
            request.open("POST", "/api/Follow/Unfollow");
            request.onreadystatechange = function() {
                if(request.readyState === XMLHttpRequest.DONE) {
                    switch (request.status) {
                        case 200 :
                            followButton.innerHTML = "Follow " + '<i class="fas fa-check"></i>';
                            followingThisUser = false;
                            break;
                        case 404 :
                            console.error("User was not found. Please report this");
                            break;
                        case 401 :
                            console.error("An authentication error occurred. Please report this");
                            break;
                        default :
                            console.log(request.responseText);
                            console.log("Status code: " + request.status);
                            break;
                            
                    }
                    followButton.disabled = false;
                }
            }
            request.send(formData);
        } else {
            let formData = new FormData();
            formData.append("sessionToken", sessionToken);
            formData.append("userToFollowId", thisProfileId);

            let request = new XMLHttpRequest();
            request.open("POST","/api/Follow");
            request.onreadystatechange = function() {

                if(request.readyState === XMLHttpRequest.DONE) {
                    switch (request.status) {
                        case 200 :
                            followButton.innerHTML = "Unfollow " + '<i class="fas fa-times"></i>';
                            followingThisUser = true;
                            break;
                        case 404 :
                            console.error("User was not found. Please report this");
                            break;
                        case 401 :
                            console.error("An authentication error occurred. Please report this");
                            break;
                        default :
                            console.log(request.responseText);
                            console.log("Status code: " + request.status);
                            break;
                    }
                    followButton.disabled = false;
                }
            }
            request.send(formData);
        }
        
        
    });
    
    let vueContainer = new Vue({
        el: '#vue-container',
        data: {
            posts: [],
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
            profilePictureElement: document.getElementById("profile_photo_el"),
            audioDescription: {
                playing: false,
                paused: false
            },
            audioDescriptionUrl: audioDescriptionUrl
        },
        computed: {
            openThreadLink: function() {
                return `/Threads/${this.commentsViewer.postId}`;
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
            },
        },
        mounted: function() {
            this.$nextTick(function() {
                let globalThis = this;
                this.audioPlayer.onended = function() {
                    // if it was playing the description it has to stop the photo rotating
                    if(globalThis.audioUrl === leftBarVueJsInstance.audioDescriptionUrl) {
                        leftBarVueJsInstance.stopRotatingProfilePicture();
                        leftBarVueJsInstance.playing = false;
                    }
                    globalThis.audioUrl = "";
                }
            });
        }
    });
    
    const leftBarVueJsInstance = new Vue({
        el: '#profile_photo_container', 
        data: {
            sessionToken: sessionToken,
            userData: userData,
            audioDescriptionUrl: audioDescriptionUrl, // this value is defined on the page (rendered by te server)
            playing: false,
            paused: false
        },
        computed: {
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
            // methods for audio description behaviour
            startRotatingProfilePicture: function() {
                if(this.paused) {
                    this.profilePictureElement.getAnimations().forEach(function(element){
                        element.play();
                    });
                } else {
                    this.profilePictureElement.animate([
                        {transform: 'rotate(0deg)'},
                        {transform: 'rotate(360deg)'}
                    ], {
                        duration: 2800,
                        iterations: Infinity
                    });
                }

            },
            pauseRotatingProfilePicture: function() {
                this.$refs.profilePictureElement.getAnimations().forEach(function(element){
                    element.pause();
                });
            },
            stopRotatingProfilePicture: function() {
                this.$refs.profilePictureElement.getAnimations().forEach(function(element){element.cancel()})
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
            }
        },
        mounted: function(){
            this.$nextTick(function() {
                
            });
        }
    });
    
    getPosts(accountData.userId, (responseObject) => {
        vueContainer.posts = responseObject;
        vueContainer.loading = false;
    }, () => {
        alert("Could not get posts");
    });
})();