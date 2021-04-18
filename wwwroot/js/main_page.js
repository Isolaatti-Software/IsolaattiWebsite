let postButton = document.querySelector("#post-button");
let privacyModeSelector = document.querySelector("#privacy-selector");
let postTextArea = document.querySelector("#simple-post-textarea");

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

    }, (error) => {
        console.error(error);
    });
});

function onScroll() {
    if (document.body.scrollHeight - window.innerHeight  === window.scrollY) {
        putPosts();
    }
}

function putPosts() {
    vueContainer.loading = true;
    window.onscroll = null
    getFeed(JSON.stringify(postsInDOM), (response) => {
        vueContainer.loading = false;
        console.log(response);
        if(response.serverResponse.length === 0) {
            vueContainer.noMoreContent = true;
            window.onscroll = null;
        } else {
            response.serverResponse.forEach((post) => {
                vueContainer.posts.push(post);
                postsInDOM.push(post.id);
                window.onscroll = onScroll;
            });
        }

    }, (error) => {
        console.error(error);
    });
}

function makePost(privacy, content, onComplete, onError) {
    let form = new FormData();
    form.append("sessionToken", sessionToken);
    form.append("privacy", privacy);
    form.append("content", content);

    let request = new XMLHttpRequest();
    request.open("POST", "/api/MakePost");
    request.onreadystatechange = () => {
        if (request.readyState === XMLHttpRequest.DONE) {
            switch (request.status) {
                case 200:
                    onComplete({
                        statusCode: request.status,
                        serverResponse: JSON.parse(request.responseText)
                    });
                    break;
                case 404:
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
                        serverResponse: "Unknown error, please report to developer"
                    });
                    break;
            }
        }
    };
    request.send(form);
}

function getFeed(postsInDOM, onComplete, onError) {
    let form = new FormData();
    form.append("sessionToken", sessionToken);
    form.append("postsInDom", postsInDOM);

    let request = new XMLHttpRequest();
    request.open("POST", "/api/Feed");
    request.onreadystatechange = () => {
        if (request.readyState === XMLHttpRequest.DONE) {
            switch (request.status) {
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

document.addEventListener("DOMContentLoaded", () => {
    putPosts();
    window.onscroll = onScroll;
});

let vueContainer = new Vue({
    el: "#posts-deposit",
    data: {
        posts: [],
        followingIdNameMap: followingIdNameMap, // this object is generated in the HTML, by the server
        loading: false,
        noMoreContent: false
    },
    methods: {
        likePost: function (post) {
            let formData = new FormData();
            formData.append("sessionToken", sessionToken);
            formData.append("postId", post.id);

            let request = new XMLHttpRequest();
            request.open("POST", "/api/Likes/LikePost");
            request.onreadystatechange = () => {
                if (request.readyState === XMLHttpRequest.DONE) {
                    if (request.status === 200) {
                        let modifiedPost = JSON.parse(request.responseText);
                        let index = vueContainer.posts.findIndex(post => post.id === modifiedPost.id);
                        Vue.set(vueContainer.posts, index, modifiedPost);
                    }
                }
            }
            request.send(formData);
        },
        unLikePost: function (post) {
            let formData = new FormData();
            formData.append("sessionToken", sessionToken);
            formData.append("postId", post.id);

            let request = new XMLHttpRequest();
            request.open("POST", "/api/Likes/UnLikePost");
            request.onreadystatechange = () => {
                if (request.readyState === XMLHttpRequest.DONE) {
                    if (request.status === 200) {
                        let modifiedPost = JSON.parse(request.responseText);
                        let index = vueContainer.posts.findIndex(post => post.id === modifiedPost.id);
                        Vue.set(vueContainer.posts, index, modifiedPost);
                    }
                }
            }
            request.send(formData);
        },
        refresh: function() {
            window.onscroll = onScroll;
            this.noMoreContent = false;
            postsInDOM = [];
            this.posts = [];
            putPosts();
        },
        compileMarkdown: function(raw) {
            return marked(raw);
        }
    }
});