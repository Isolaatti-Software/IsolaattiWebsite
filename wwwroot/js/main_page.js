

(function() {
    let postButton = document.querySelector("#post-button");
    let privacyModeSelector = document.querySelector("#privacy-selector");
    let postTextArea = document.querySelector("#simple-post-textarea");
    let postDeposit = document.getElementById("posts-deposit");
    let spinnerElement = document.createElement("div");
    spinnerElement.classList.add("spinner-border");
    spinnerElement.id = "spinner-element";
    
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
           deposit.insertBefore(generatePostDOMElement(userData.id,post.textContent),deposit.firstChild);
       }, (error) => {
           console.error(error);
       });
    });
    
    window.onscroll = () => {
        if(document.body.scrollHeight - window.innerHeight === window.scrollY) {
            postDeposit.appendChild(spinnerElement);
            getFeed(JSON.stringify(postsInDOM), (event) => {
                console.log(event);
                postDeposit.removeChild(spinnerElement);
                event.serverResponse.forEach((post) => {
                    console.log(post);
                    postDeposit.appendChild(generatePostDOMElement(post.userId,post.textContent));
                    postsInDOM.push(post.id);
                });
            }, (error) => {
                console.error(error);
            });
        }
    }
    
    document.addEventListener("DOMContentLoaded", () => {
        postDeposit.appendChild(spinnerElement);
        getFeed(JSON.stringify(postsInDOM), (event) => {
            console.log(event);
            postDeposit.removeChild(spinnerElement);
            event.serverResponse.forEach((post) => {
                console.log(post);
                postDeposit.appendChild(generatePostDOMElement(post.userId,post.textContent));
                postsInDOM.push(post.id);
            });
        }, (error) => {
            console.error(error);
        });
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

function generatePostDOMElement(author, text) {
    let container = document.createElement("div");
    container.classList.add("d-flex", "mb-2", "rounded-border-black", "black-box-shadow", "flex-column", "p-2", "post");
    
    let authorParagraph = document.createElement("p");
    authorParagraph.classList.add("user-name");
    authorParagraph.innerHTML = author;
    
    let contentParagraph = document.createElement("p");
    contentParagraph.classList.add("mt-2", "post-content");
    contentParagraph.innerHTML = text;
    
    let topPart = document.createElement("div");
    topPart.classList.add("d-flex");
    
    let bodyPart = document.createElement("div");
    bodyPart.classList.add("d-flex");
    
    topPart.appendChild(authorParagraph);
    bodyPart.appendChild(contentParagraph);
    
    container.appendChild(topPart);
    container.appendChild(bodyPart);
    
    return container;
}