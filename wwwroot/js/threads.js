// Threads page

(function () {
    let form = new FormData();
    form.append("userId", userData.id);
    form.append("password", userData.password);
    form.append("postId", threadToReadId);

    let request = new XMLHttpRequest();
    request.open("POST", "/api/GetPost");
    request.onload = () => {
        if (request.status === 200) {
            //return 
            new Vue({
                el: "#vue-container",
                data: {
                    post: JSON.parse(request.responseText),
                    commentBoxFullSize: false
                },
                methods: {
                    likePost: function (post) {
                        let formData = new FormData();
                        formData.append("userId", userData.id)
                        formData.append("password", userData.password);
                        formData.append("postId", post.id);

                        let request = new XMLHttpRequest();
                        request.open("POST", "/api/Likes/LikePost");
                        request.onreadystatechange = () => {
                            if (request.readyState === XMLHttpRequest.DONE) {
                                if (request.status === 200) {
                                    let modifiedPost = JSON.parse(request.responseText);
                                    this.post = modifiedPost;
                                    // let index = vueContainer.posts.findIndex(post => post.id === modifiedPost.id);
                                    // Vue.set(vueContainer.posts, index, modifiedPost);
                                }
                            }
                        }
                        request.send(formData);
                    },
                    unLikePost: function (post) {
                        let formData = new FormData();
                        formData.append("userId", userData.id)
                        formData.append("password", userData.password);
                        formData.append("postId", post.id);

                        let request = new XMLHttpRequest();
                        request.open("POST", "/api/Likes/UnLikePost");
                        request.onreadystatechange = () => {
                            if (request.readyState === XMLHttpRequest.DONE) {
                                if (request.status === 200) {
                                    let modifiedPost = JSON.parse(request.responseText);
                                    this.post = modifiedPost;
                                    // let index = vueContainer.posts.findIndex(post => post.id === modifiedPost.id);
                                    // Vue.set(vueContainer.posts, index, modifiedPost);
                                }
                            }
                        }
                        request.send(formData);
                    },
                    makeComment: function () {

                    },
                    compileMarkdown: function(raw) {
                        return marked(raw);
                    },
                    toggleCommentBoxFullSize: function() {
                        let button = this.$refs.buttonToggleComSize;
                        if(!this.commentBoxFullSize) {

                            this.$refs.commentBox.style.animation = "expandCommentBox";
                            this.$refs.commentBox.style.animationDuration = "0.4s"
                            this.$refs.commentBox.style.animationFillMode = "forwards";
                            this.commentBoxFullSize = true;
                            button.innerHTML = '<i class="fas fa-arrow-down"></i>';
                        } else {

                            this.$refs.commentBox.style.animation = "shrinkCommentBox";
                            this.$refs.commentBox.style.animationDuration = "0.4s";
                            this.$refs.commentBox.style.animationFillMode = "forwards";
                            button.innerHTML = '<i class="fas fa-arrow-up"></i>';
                            this.commentBoxFullSize = false;
                        }
                    }
                }
            })
        }
    }
    request.send(form);
})();