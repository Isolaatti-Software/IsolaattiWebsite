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
    formData.append("userId", userData.id);
    formData.append("password", userData.password);
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
            formData.append("userId", userData.id);
            formData.append("password", userData.password);
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
            formData.append("userId", userData.id);
            formData.append("password", userData.password);
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
            posts: []
        },
        methods: {
            likePost: function(post) {
                let formData = new FormData();
                formData.append("userId", userData.id)
                formData.append("password", userData.password);
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
                formData.append("userId", userData.id)
                formData.append("password", userData.password);
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
            }
        }
    });
    
    getPosts(accountData.userId, (responseObject) => {
        vueContainer.posts = responseObject
    }, () => {
        alert("Could not get posts");
    });
})();