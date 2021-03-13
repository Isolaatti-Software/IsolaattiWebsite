

(function() {
    let postButton = document.querySelector("#post-button");
    let privacyModeSelector = document.querySelector("#privacy-selector");
    let postTextArea = document.querySelector("#simple-post-textarea");
    
    postButton.addEventListener("click", () => {
       postButton.disabled = true;
       postButton.innerHTML = 
           "Posting " + '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>';
       makePost(privacyModeSelector.value, postTextArea.value, (info) => {
           console.log("post made");
           postButton.innerHTML = "Post";
           postTextArea.value = "";
           postButton.disabled = false;
           
           // here I should add this post to the feed in the DOM
           let thisPost = document.createElement("p");
           thisPost.innerHTML = JSON.stringify(info.serverResponse);
           console.log(info.serverResponse);
           document.getElementById("posts-deposit").appendChild(thisPost);
       }, (error) => {
           console.error(error);
       });
    });
})();

function makePost(privacy, content, onComplete, onError) {
    let form = new FormData();
    form.append("userId", userData.id);
    form.append("password", userData.password);

    let privacyNumber;
    switch (privacy) {
        case "private" : privacyNumber = 1; break;
        case "isolaatti" : privacyNumber = 2; break;
        case "everyone" : privacyNumber = 3; break;
        default : privacyNumber = 1; break;
    }
    
    form.append("privacy", privacyNumber);
    form.append("content", content);
    
    let request = new XMLHttpRequest();
    request.open("POST", "/api/MakePost");
    request.onreadystatechange = () => {
        if(request.readyState === XMLHttpRequest.DONE) {
            switch (request.status) {
                case 200: onComplete({
                    statusCode: request.status,
                    serverResponse: JSON.parse(request.responseText)
                }); break;
                case 404: onError({
                    statusCode: 404,
                    serverResponse: request.responseText
                }); break;
                case 401 : onError({
                    statusCode: 401,
                    serverResponse: request.responseText
                }); break;
                case 500 : onError({
                    statusCode: 500,
                    serverResponse: "Unknown error, please report to developer"
                }); break;
            }
        }
    };
    request.send(form);
}