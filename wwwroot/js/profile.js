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
})();