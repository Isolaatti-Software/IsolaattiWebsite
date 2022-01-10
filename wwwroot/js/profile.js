/*
* Isolaatti project
* Erik Cavazos, 2021
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
* 
* This file should be placed in the Pages/Profile.cshtml
*/


function fetchUserOnlineStatus() {
    const greenDot = document.getElementById("online-dot");
    fetch(`/api/Fetch/IsUserOnline/${accountData.userId}`, {
        method: "GET",
        headers: customHttpHeaders
    }).then(res => {
        res.json().then(val => {
            greenDot.style.display = val ? 'block' : 'none'
        })
    })
}


(function () {

    let followButton = document.querySelector("#followButton");
    followButton.addEventListener("click", function () {
        followButton.disabled = true;
        if (followingThisUser) {
            fetch("/api/Following/Unfollow", {
                method: "POST",
                headers: customHttpHeaders,
                body: JSON.stringify({id: accountData.userId})
            }).then(res => {
                followButton.innerHTML = 'Seguir <i class="fas fa-check"></i>';
                followingThisUser = false;
                followButton.disabled = false;
            })
        } else {
            fetch("/api/Following/Follow", {
                method: "POST",
                headers: customHttpHeaders,
                body: JSON.stringify({id: accountData.userId})
            }).then(res => {
                followButton.innerHTML = 'Dejar de seguir <i class="fas fa-times"></i>';
                followingThisUser = true;
                followButton.disabled = false
            })
        }

    });
    /*                                 Vue.js part                                                                    */

    /* Functions for vue methods starts here */

    function fetchMyPosts() {
        fetch(`/api/Fetch/PostsOfUser/${this.accountData.userId}`, {headers: this.customHeaders}).then(result => {
            result.json().then(posts => {
                this.posts = posts;
                this.loading = false;
            })
        })
    }

    function fetchComments() {
        fetch(`/api/Fetch/Post/${this.commentsViewer.postId}/Comments`, {
            headers: this.customHeaders
        }).then(res => {
            res.json().then(comments => {
                this.commentsViewer.comments = comments;
            })
        })
    }

    async function likePost(post) {
        const postId = post.id;
        const requestData = {id: postId}
        const globalThis = this;
        const response = await fetch("/api/Likes/LikePost", {
            method: "POST",
            headers: this.customHeaders,
            body: JSON.stringify(requestData),
        });

        response.json().then(function (post) {
            let index = vueContainer.posts.findIndex(post => post.postData.id === postId);
            Vue.set(vueContainer.posts, index, post);
        });
    }

    async function unLikePost(post) {
        const postId = post.id;
        const requestData = {id: postId}
        const globalThis = this;
        const response = await fetch("/api/Likes/UnLikePost", {
            method: "POST",
            headers: this.customHeaders,
            body: JSON.stringify(requestData),
        });

        response.json().then(function (post) {
            let index = vueContainer.posts.findIndex(post => post.postData.id === postId);
            Vue.set(vueContainer.posts, index, post);
        });
    }

    function compileMarkdown(raw) {
        if (raw === null) raw = "";
        return marked(raw);
    }

    function playAudio(url) {
        if (this.audioUrl !== url) {
            this.audioPlayer.pause();
            this.audioUrl = url
            this.audioPlayer.src = url;
            this.audioPlayer.play();
            this.paused = false;
        } else {
            if (!this.audioPlayer.paused) {
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

    function getPostStyle(themeDefinitionJson) {
        if (themeDefinitionJson === null)
            return "";
        const theme = themeDefinitionJson;
        return `color: ${theme.fontColor};
                        background-color: ${theme.backgroundColor};
                        border: ${theme.border.size} ${theme.border.type} ${theme.border.color}`;
    }

    function copyToClipboard(relativeUrl) {
        let absoluteUrl = `${window.location.protocol}//${window.location.host}${relativeUrl}`;
        navigator.clipboard.writeText(absoluteUrl).then(function () {
            alert("Se copió el texto al portapapeles");
        });
    }

    function deletePost(postId) {
        if (!confirm("¿De verdad deseas eliminar esta publicación?")) {
            return;
        }

        fetch("/api/Posting/Delete", {
            method: "POST",
            headers: this.customHeaders,
            body: JSON.stringify({id: postId})
        }).then(res => {
            if (res.ok) {
                let postIndex = this.posts.findIndex(value => value.id === postId);
                this.posts.splice(postIndex, 1);
            }
        });
    }

    function deleteComment(id) {
        if (!confirm("¿De verdad deseas eliminar esta publicación?")) {
            return;
        }

        fetch("/api/Posting/Comment/Delete", {
            method: "POST",
            headers: this.customHeaders,
            body: JSON.stringify({id: id})
        }).then(res => {
            if (res.ok) {
                let commentId = this.commentsViewer.comments.findIndex(value => value.id === id);
                if (commentId === -1) return;
                this.commentsViewer.comments.splice(commentId, 1);
            }
        });
    }

    function viewComments(post) {
        this.commentsViewer.postId = post.id;
        this.fetchComments();
    }

    /* Functions for vue methods ends here */

    let vueContainer = new Vue({
        el: '#vue-container',
        data: {
            customHeaders: customHttpHeaders,
            accountData: accountData,
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
            audioDescription: {
                playing: false,
                paused: false
            },
            audioDescriptionUrl: audioDescriptionUrl
        },
        computed: {
            openThreadLink: function () {
                return `/pub/${this.commentsViewer.postId}`;
            }
        },

        /* to add a method, add first a function above and then reference it here */
        methods: {
            likePost: likePost,
            unLikePost: unLikePost,
            compileMarkdown: compileMarkdown,
            playAudio: playAudio,
            getPostStyle: getPostStyle,
            copyToClipboard: copyToClipboard,
            viewComments: viewComments,
            fetchPosts: fetchMyPosts,
            fetchComments: fetchComments,
            deleteComment: deleteComment
        },
        mounted: function () {
            this.$nextTick(function () {
                fetchUserOnlineStatus();
                setInterval(function () {
                    fetchUserOnlineStatus();
                }, 30000);

                this.fetchPosts();
                let globalThis = this;
                this.audioPlayer.onended = function () {
                    // if it was playing the description it has to stop the photo rotating
                    if (globalThis.audioUrl === leftBarVueJsInstance.audioDescriptionUrl) {
                        leftBarVueJsInstance.stopRotatingProfilePicture();
                        leftBarVueJsInstance.playing = false;
                    }
                    globalThis.audioUrl = "";
                }
            });
        }
    });


    /*****************************************************************************************************************/


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
                    this.$refs.profilePictureElement.getAnimations().forEach(function (element) {
                        element.play();
                    });
                } else {
                    this.$refs.profilePictureElement.animate([
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

        }
    });
    
})();