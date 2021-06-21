let postButton = document.querySelector("#post-button");
let privacyModeSelector = document.querySelector("#privacy-selector");
let postTextArea = document.querySelector("#simple-post-textarea");

let postsInDOM = [];

// postButton.addEventListener("click", () => {
//     if(postTextArea.value === "") {
//         alert("Come on, you can't upload an empty post");
//         return;
//     }
//     postButton.disabled = true;
//     postButton.innerHTML =
//         "Posting " + '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>';
//     makePost(privacyModeSelector.value, postTextArea.value, (info) => {
//         console.log("post made");
//         postButton.innerHTML = "Post";
//         postTextArea.value = "";
//         postButton.disabled = false;
//         putPosts();
//     }, (error) => {
//         console.error(error);
//     });
// });

function onScroll() {
    console.log(document.body.scrollHeight * 0.50,window.scrollY);
    if (Math.trunc(document.body.scrollHeight * 0.5) >= Math.trunc(window.scrollY)) {
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

// function makePost(privacy, content, onComplete, onError) {
//     if(content === "") {
//         alert("Come on, you can't upload an empty post");
//         return;
//     }
//     let form = new FormData();
//     form.append("sessionToken", sessionToken);
//     form.append("privacy", privacy);
//     form.append("content", content);
//
//     let request = new XMLHttpRequest();
//     request.open("POST", "/api/MakePost");
//     request.onreadystatechange = () => {
//         if (request.readyState === XMLHttpRequest.DONE) {
//             switch (request.status) {
//                 case 200:
//                     onComplete({
//                         statusCode: request.status,
//                         serverResponse: JSON.parse(request.responseText)
//                     });
//                     break;
//                 case 404:
//                     onError({
//                         statusCode: 404,
//                         serverResponse: request.responseText
//                     });
//                     break;
//                 case 401 :
//                     onError({
//                         statusCode: 401,
//                         serverResponse: request.responseText
//                     });
//                     break;
//                 case 500 :
//                     onError({
//                         statusCode: 500,
//                         serverResponse: "Unknown error, please report to developer"
//                     });
//                     break;
//             }
//         }
//     };
//     request.send(form);
// }

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
    el: "#vue-container",
    data: {
        sessionToken: sessionToken,
        userData: userData,
        posts: [],
        loading: true,
        noMoreContent: false,
        audioPlayer: new Audio(),
        audioUrl: "",
        playing: false,
        paused: false,
        postLinkToShare: ""
    },
    methods: {
        likePost: function (post) {
            let formData = new FormData();
            formData.append("sessionToken", this.sessionToken);
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
            formData.append("sessionToken", this.sessionToken);
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
            if(!confirm("Are your sure you want to delete this post?")) {
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
                    globalThis.refresh();
                }
            }
            request.send(form);
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
});