

(function() {
    let postButton = document.querySelector("#post-button");
    let privacyModeSelector = document.querySelector("#privacy-selector");
    let postTextArea = document.querySelector("#simple-post-textarea");
    let postDeposit = document.getElementById("posts-deposit");
    let spinnerElement = document.createElement("div");
    spinnerElement.classList.add("spinner-border");
    spinnerElement.id = "spinner-element";
    
    let noContentDOMItem = document.createElement("div");
    let updateFeedButton = document.createElement("button");
    let noContentText = document.createElement("span");
    noContentDOMItem.classList.add("d-flex","flex-column");
    noContentText.innerHTML = "No more new content so far";
    updateFeedButton.innerHTML = '<i class="fas fa-redo-alt"></i>'
    updateFeedButton.classList.add("btn", "btn-link");
    updateFeedButton.addEventListener("click", () => {
        postsInDOM = [];
        postDeposit.innerHTML = "";
        putPosts();
    });
    noContentDOMItem.appendChild(noContentText);
    noContentDOMItem.appendChild(updateFeedButton);
    
    let postsInDOM = [];
    
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
           let post = info.serverResponse;
           console.log(info.serverResponse);
           let deposit = document.getElementById("posts-deposit");
           deposit.insertBefore(generatePostDOMElement(userData.name,post.textContent),deposit.firstChild);
       }, (error) => {
           console.error(error);
       });
    });

    function onScroll() {
        if(document.body.scrollHeight - window.innerHeight === window.scrollY) {
            postDeposit.appendChild(spinnerElement);
            getFeed(JSON.stringify(postsInDOM), (event) => {
                console.log(event);
                postDeposit.removeChild(spinnerElement);
                if(event.serverResponse.length === 0) {
                    window.onscroll = null;
                    postDeposit.appendChild(noContentDOMItem);
                } else {
                    event.serverResponse.forEach((post) => {
                        console.log(post);
                        postDeposit.appendChild(generatePostDOMElement(followingIdNameMap.get(post.userId),post.textContent,
                            post.privacy,post.numberOfLikes));
                        postsInDOM.push(post.id);
                    });
                }

            }, (error) => {
                console.error(error);
            });
        }
    }
    
    function putPosts() {
        postDeposit.appendChild(spinnerElement);
        getFeed(JSON.stringify(postsInDOM), (event) => {
            console.log(event);
            postDeposit.removeChild(spinnerElement);
            if(event.serverResponse.length === 0) {
                postDeposit.appendChild(noContentDOMItem);
            } else {
                event.serverResponse.forEach((post) => {
                    console.log(post);
                    postDeposit.appendChild(generatePostDOMElement(followingIdNameMap.get(post.userId),
                        post.textContent,post.privacy,post.numberOfLikes));
                    postsInDOM.push(post.id);
                    window.onscroll = onScroll;
                });
            }

        }, (error) => {
            console.error(error);
        });
    }
        
    document.addEventListener("DOMContentLoaded", () => {
        putPosts();
    });
})();

function makePost(privacy, content, onComplete, onError) {
    let form = new FormData();
    form.append("userId", userData.id);
    form.append("password", userData.password);
    form.append("privacy", privacy);
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

function getFeed(postsInDOM, onComplete, onError) {
    let form = new FormData();
    form.append("userId", userData.id);
    form.append("password", userData.password);
    form.append("postsInDom", postsInDOM);
    
    let request = new XMLHttpRequest();
    request.open("POST", "/api/Feed");
    request.onreadystatechange = () => {
        if(request.readyState === XMLHttpRequest.DONE) {
            switch(request.status) {
                case 200 :
                    onComplete({
                        statusCode: 200,
                        serverResponse: JSON.parse(request.responseText)
                    }, JSON.parse(request.responseText));
                    break;
                case 404 :
                    onError({
                        statusCode: 404,
                        serverResponse: request.responseText
                    });
                    break;
                case 401 :
                    onError({
                        statusCode: 401,
                        serverResponse: request.responseText
                    });
                    break;
                case 500 :
                    onError({
                        statusCode: 500,
                        serverResponse: "Server error. Report this to developer"
                    });
                    break;
                default :
                    onError({
                        statusCode: request.status,
                        serverResponse: request.responseText
                    });
                    break;
            }
        }
    }
    request.send(form);
}


/* This function generates the equivalent to this HTML */
/*
<div class="d-flex mb-2 rounded-border-black flex-column p-2 post">
    <div class="d-flex justify-content-between align-items-center">
        <span class="user-name"><b>Name</b></span>
    </div>
    <div class="d-flex privacy-icon-container">
        <i class="fas fa-user-friends" title="People on Isolaatti" aria-hidden="true"></i><span class="sr-only">People on Isolaatti</span>
            or
        <i class="fas fa-globe" title="All the world" aria-hidden="true"></i><span class="sr-only">Everyone</span>
    </div>
    <p class="mt-2 post-content">{content}</p>
    <div class="d-flex justify-content-end">
        <button class="btn btn-dark btn-sm" type="button">
            <i class="fas fa-comments" aria-hidden="true"></i> {_}
        </button>
        <button class="btn btn-dark btn-sm" type="button">
            <i class="fas fa-thumbs-up" aria-hidden="true"></i> {likes}
        </button>
     </div>
</div>
*/
function generatePostDOMElement(author, text, privacy, likes, comments) {
    let container = document.createElement("div");
    container.classList.add("d-flex","mb-2","rounded-border-black","flex-column","p-2","post");
    
    let topPart = document.createElement("div");
    topPart.classList.add("d-flex","justify-content-between","align-items-center");
    
    let privacyContainer = document.createElement("div");
    privacyContainer.classList.add("d-flex","privacy-icon-container");
    
    let contentParagraph = document.createElement("p");
    contentParagraph.classList.add("mt-2","post-content");
    
    let bottomPart = document.createElement("div");
    bottomPart.classList.add("d-flex","justify-content-end");
    
    // create and populate name span
    let nameSpan = document.createElement("span");
    nameSpan.classList.add("user-name");
    nameSpan.innerHTML = '<b>' + author + '</b>';
    
    // create privacy icon
    let privacyIcon = document.createElement("i");
    privacyIcon.classList.add("fas");
    switch(privacy) {
        case 2 : privacyIcon.classList.add("fa-user-friends"); break;
        case 3 : privacyIcon.classList.add("fa-globe"); break;
    }
    
    // populate Post content paragraph
    contentParagraph.innerHTML = text;
    
    // create and populate bottom buttons
    let commentsButton = document.createElement("button");
    commentsButton.classList.add("btn","btn-dark","btn-sm");
    commentsButton.innerHTML = '<i class="fas fa-comments" aria-hidden="true"></i>' + " " + comments;
    
    let likeButton = document.createElement("button");
    likeButton.classList.add("btn","btn-dark","btn-sm");
    likeButton.innerHTML = '<i class="fas fa-thumbs-up" aria-hidden="true"></i>' + " " + likes;
    
    //  populate top part
    topPart.appendChild(nameSpan);
    
    // populate privacy container
    privacyContainer.appendChild(privacyIcon);
    
    // populate bottom part
    bottomPart.appendChild(commentsButton);
    bottomPart.appendChild(likeButton);
    
    // populate container
    container.appendChild(topPart);
    container.appendChild(privacyContainer);
    container.appendChild(contentParagraph);
    container.appendChild(bottomPart);
    
    return container;
}

