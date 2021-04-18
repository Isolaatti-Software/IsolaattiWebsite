// Threads page
(function () {
    let form = new FormData();
    form.append("sessionToken", sessionToken);
    form.append("postId", threadToReadId);

    let request = new XMLHttpRequest();
    request.open("POST", "/api/GetPost");
    request.onload = () => {
        if (request.status === 200) {
            //return 
            let vueInstance = new Vue({
                el: "#vue-container",
                data: {
                    post: JSON.parse(request.responseText),
                    commentBoxFullSize: false,
                    commentTextarea: "",
                    selectionStart: 0,
                    selectionEnd: 0,
                    comments: []
                },
                computed: {
                  makeCommentButtonDisabled: function() {
                      return this.commentTextarea === "" || this.commentTextarea === null
                  }  
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
                        formData.append("sessionToken", sessionToken);
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
                        console.log("Se hizo el comentario!!");
                        let form = new FormData();
                        form.append("sessionToken", sessionToken);
                        form.append("postId",this.post.id);
                        form.append("content", this.commentTextarea);
                        
                        let request = new XMLHttpRequest();
                        request.open("POST","/api/MakeComment");
                        request.onload = () => {
                            if(request.status === 200) {
                                console.log(JSON.parse(request.responseText));
                                this.commentTextarea = "";
                                this.getComments();
                            }
                        }
                        request.send(form);
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
                    },
                    insertHeading: function(headingN) {
                        let markdown = "";
                        let textarea = vueInstance.$refs.commentTextareaRef;
                        switch(headingN) {
                            case 1: markdown = "# Header 1"; break;
                            case 2: markdown = "## Header 2"; break;
                            case 3: markdown = "### Header 3"; break;
                            case 4: markdown = "#### Header 4"; break;
                            case 5: markdown = "##### Header 5"; break;
                            case 6: markdown = "###### Header 6"; break;
                        }

                        if(textarea.selectionStart === textarea.selectionEnd) {
                            let textBefore = "";
                            let textAfter = "";
                            for(let i = 0; i < textarea.selectionStart; i++) {
                                textBefore += this.commentTextarea.charAt(i);
                            }
                            for(let i = textarea.selectionStart; i < this.commentTextarea.length; i++ ) {
                                textAfter += this.commentTextarea.charAt(i);
                            }
                            if(textBefore.length !== 0) {
                                this.commentTextarea = textBefore + "\n" + markdown + textAfter;
                                textarea.selectionStart = textBefore.length + markdown.length + 1;
                                textarea.selectionEnd = textBefore.length + markdown.length + 1;
                                console.log(textBefore.length + markdown.length + 1);
                            } else {
                                this.commentTextarea = textBefore + markdown + textAfter;
                                textarea.selectionStart = textBefore.length + markdown.length;
                                textarea.selectionEnd = textBefore.length + markdown.length;
                                console.log(textBefore.length + markdown.length);
                            }
                            textarea.focus();
                        } else {

                        }
                    },
                    surroundTextWith: function(tag) {
                        let markdownSurrounder = "";
                        let textarea = vueInstance.$refs.commentTextareaRef;
                        switch(tag) {
                            case "bold": markdownSurrounder = "**"; break;
                            case "italics": markdownSurrounder = "*"; break;
                        }

                        console.log(textarea);
                        

                        if(textarea.selectionStart === textarea.selectionEnd) {
                            let textBefore = "";
                            let textAfter = "";
                            for(let i = 0; i < textarea.selectionStart; i++) {
                                textBefore += this.commentTextarea.charAt(i);
                            }
                            for(let i = textarea.selectionStart; i < this.commentTextarea.length; i++ ) {
                                textAfter += this.commentTextarea.charAt(i);
                            }

                            // cursor is not inside any pair of "*"
                            if(!(textBefore.charAt(textBefore.length - 1) === "*" && textAfter.charAt(0) === "*")) {
                                if(textBefore.charAt(textBefore.length - 1) === "*") {
                                    textBefore += " ";
                                }

                                this.commentTextarea = textBefore + markdownSurrounder + markdownSurrounder + textAfter;
                                this.selectionStart = textBefore.length + markdownSurrounder.length;
                                this.selectionEnd = textBefore.length + markdownSurrounder.length;

                            } else {
                                if(tag === "italics"){
                                    // this means that it is has only bold, so it can add another * for italics
                                    if(textBefore.substring(textBefore.length - 4) !== "***"
                                        && textBefore.substring(textBefore.length - 3) === "**"){
                                        this.commentTextarea = textBefore + markdownSurrounder + markdownSurrounder + textAfter;
                                        textarea.selectionStart = textBefore.length + markdownSurrounder.length;
                                        textarea.selectionEnd = textBefore.length + markdownSurrounder.length;
                                        console.log(textBefore.length + markdownSurrounder.length);
                                    }
                                } else {
                                    if(textBefore.substring(textBefore.length - 4) !== "***"
                                        && textBefore.substring(textBefore.length - 3) !== "**"){
                                        this.commentTextarea = textBefore + markdownSurrounder + markdownSurrounder + textAfter;
                                        this.selectionStart = textBefore.length + markdownSurrounder.length;
                                        this.selectionEnd = textBefore.length + markdownSurrounder.length;
                                        console.log(textBefore.length + markdownSurrounder.length);
                                    }
                                }
                            }
                            textarea.focus();
                            

                        } else {
                            let textBefore = "";
                            let textAfter = "";
                            let selectedText = "";
                            for(let i = 0; i < textarea.selectionStart; i++) {
                                textBefore += this.commentTextarea.charAt(i);
                            }
                            for(let i = textarea.selectionEnd; i < this.commentTextarea.length; i++ ) {
                                textAfter += this.commentTextarea.charAt(i);
                            }
                            for(let i = textarea.selectionStart; i < textarea.selectionEnd; i++) {
                                selectedText += this.commentTextarea.charAt(i);
                            }
                            if(textBefore.substring(textBefore.length - (markdownSurrounder + 1)) !== markdownSurrounder) {
                                this.commentTextarea = textBefore + markdownSurrounder + selectedText + markdownSurrounder + textAfter;
                            }
                        }
                    },
                    getComments: function() {
                        let form = new FormData();
                        form.append("sessionToken", sessionToken);
                        form.append("postId", this.post.id);
                        
                        let request = new XMLHttpRequest();
                        request.open("POST", "/api/GetPost/Comments");
                        request.onload = () => {
                            if(request.status === 200) {
                                this.comments = JSON.parse(request.responseText);
                            }
                        }
                        request.send(form);
                    }
                },
                mounted: function() {
                    this.$nextTick(function() {
                        this.getComments();
                    });
                }
            })
        }
    }
    request.send(form);
})();