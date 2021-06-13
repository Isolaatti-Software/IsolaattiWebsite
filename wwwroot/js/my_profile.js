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
                posts: [],
                selectedPostForModalsId: 0,
                textareaEditPost: "",
                privacyEditPost: "",
                audioPlayer: new Audio(),
                audioUrl: "",
                playing: false,
                paused: false,
                loading: true
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
                openEditPostModal: function(post) {
                    this.selectedPostForModalsId = post.id;
                    this.textareaEditPost = post.textContent;
                    this.privacyEditPost = post.privacy;
                    $('#modal-edit-post').modal('show');
                },
                openDeletePostModal: function(post) {
                    this.selectedPostForModalsId = post.id;
                    this.textareaEditPost = post.textContent;
                    this.privacyEditPost = post.privacy;
                    $('#modal-delete-post').modal('show');
                },
                confirmPostModification: function() {
                    let postReference = this.posts.find(post => post.id === this.selectedPostForModalsId);
                    let textChanged = postReference.textContent !== this.textareaEditPost;
                    let privacyChanged = postReference.privacy !== this.privacyEditPost;
                    let index = this.posts.findIndex(post => post.id === this.selectedPostForModalsId);
                    
                    if(textChanged) {
                        editPost(this.selectedPostForModalsId,this.textareaEditPost,(event) => {
                            Vue.set(vueContainer.posts, index, event.serverResponse);
                        }, (error) => {

                        }); 
                    }
                    if(privacyChanged){
                        changePostPrivacy(this.selectedPostForModalsId,this.privacyEditPost,(event) => {
                            Vue.set(vueContainer.posts, index, event.serverResponse);
                        }, () => {
                            
                        });
                    }
                    $("#modal-edit-post").modal('hide');
                },
                confirmPostDeletion: function() {
                    let index = this.posts.findIndex(post => post.id === this.selectedPostForModalsId);
                    deletePost(this.selectedPostForModalsId, () => {
                        this.posts.splice(index,1);
                        $("#modal-delete-post").modal('hide');
                    }, () => {})
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
                }
            },
            mounted: function() {
                this.$nextTick(function() {
                    let globalThis = this;
                    this.audioPlayer.onended = function() {
                        globalThis.audioUrl = "";
                    }
                });
            }
        })
        
        getPosts((response) => {
            vueContainer.posts = response;
            vueContainer.loading = false;
        }, (error) => {
            alert("Error getting your posts");
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