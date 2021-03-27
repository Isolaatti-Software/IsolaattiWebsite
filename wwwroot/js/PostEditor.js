
let markdownInput = document.getElementById("raw-post-markdown");

const vue = new Vue({
    el: "#vue-container",
    data: {
        input: "",
        selectionStart: 0,
        selectionEnd: 0,
        privacy: "2"
    },
    computed: {
        compiledMarkdown: function() {
            return marked(this.input, { sanitize: true});
        }
    },
    methods: {
        update: _.debounce(function(e){
            this.input = e.target.value
        }, 300),
        insertHeading: function(headingN) {
            let markdown = "";
            switch(headingN) {
                case 1: markdown = "# Header 1"; break;
                case 2: markdown = "## Header 2"; break;
                case 3: markdown = "### Header 3"; break;
                case 4: markdown = "#### Header 4"; break;
                case 5: markdown = "##### Header 5"; break;
                case 6: markdown = "###### Header 6"; break;
            }
            
            if(this.$refs.markdownTextarea.selectionStart === this.$refs.markdownTextarea.selectionEnd) {
                let textBefore = "";
                let textAfter = "";
                for(let i = 0; i < this.$refs.markdownTextarea.selectionStart; i++) {
                    textBefore += this.input.charAt(i);
                }
                for(let i = this.$refs.markdownTextarea.selectionStart; i < this.input.length; i++ ) {
                    textAfter += this.input.charAt(i);
                }
                if(textBefore.length !== 0) {
                    this.input = textBefore + "\n" + markdown + textAfter;
                    this.selectionStart = textBefore.length + markdown.length + 1;
                    this.selectionEnd = textBefore.length + markdown.length + 1;
                } else {
                    this.input = textBefore + markdown + textAfter;
                    this.selectionStart = textBefore.length + markdown.length;
                    this.selectionEnd = textBefore.length + markdown.length;
                }
                this.$refs.markdownTextarea.focus();
            } else {
                
            }
            
        },
        surroundTextWith: function(tag) {
            let markdownSurrounder = "";
            switch(tag) {
                case "bold": markdownSurrounder = "**"; break;
                case "italics": markdownSurrounder = "*"; break;
            }
            
            let textarea = this.$refs.markdownTextarea;
            
            if(textarea.selectionStart === textarea.selectionEnd) {
                let textBefore = "";
                let textAfter = "";
                for(let i = 0; i < textarea.selectionStart; i++) {
                    textBefore += this.input.charAt(i);
                }
                for(let i = textarea.selectionStart; i < this.input.length; i++ ) {
                    textAfter += this.input.charAt(i);
                }
                
                // cursor is not inside any pair of "*"
                if(!(textBefore.charAt(textBefore.length - 1) === "*" && textAfter.charAt(0) === "*")) {
                    if(textBefore.charAt(textBefore.length - 1) === "*") {
                        textBefore += " ";
                    }
                    
                    this.input = textBefore + markdownSurrounder + markdownSurrounder + textAfter;
                    this.selectionStart = textBefore.length + markdownSurrounder.length;
                    this.selectionEnd = textBefore.length + markdownSurrounder.length;
                    
                } else {
                    if(tag === "italics"){
                        // this means that it is has only bold, so it can add another * for italics
                        if(textBefore.substring(textBefore.length - 4) !== "***" 
                            && textBefore.substring(textBefore.length - 3) === "**"){
                            this.input = textBefore + markdownSurrounder + markdownSurrounder + textAfter;
                            this.selectionStart = textBefore.length + markdownSurrounder.length;
                            this.selectionEnd = textBefore.length + markdownSurrounder.length;
                        }
                    } else {
                        if(textBefore.substring(textBefore.length - 4) !== "***"
                            && textBefore.substring(textBefore.length - 3) !== "**"){
                            this.input = textBefore + markdownSurrounder + markdownSurrounder + textAfter;
                            this.selectionStart = textBefore.length + markdownSurrounder.length;
                            this.selectionEnd = textBefore.length + markdownSurrounder.length;
                        }
                    }
                }
                textarea.focus();
                
                
            } else {
                let textBefore = "";
                let textAfter = "";
                let selectedText = "";
                for(let i = 0; i < textarea.selectionStart; i++) {
                    textBefore += this.input.charAt(i);
                }
                for(let i = textarea.selectionEnd; i < this.input.length; i++ ) {
                    textAfter += this.input.charAt(i);
                }
                for(let i = textarea.selectionStart; i < textarea.selectionEnd; i++) {
                    selectedText += this.input.charAt(i);
                }
                if(textBefore.substring(textBefore.length - (markdownSurrounder + 1)) !== markdownSurrounder) {
                    this.input = textBefore + markdownSurrounder + selectedText + markdownSurrounder + textAfter;
                }
            }
        },
        post: function() {
            let form = new FormData();
            form.append("userId", userData.id);
            form.append("password", userData.password);
            form.append("privacy", this.privacy);
            form.append("content", this.input);

            let request = new XMLHttpRequest();
            request.open("POST", "/api/MakePost");
            request.onreadystatechange = () => {
                if (request.readyState === XMLHttpRequest.DONE) {
                    switch (request.status) {
                        case 200: window.location = "/"; break;
                        case 404: alert(request.responseText); break;
                        case 401: alert(request.responseText); break;
                        case 500: alert(request.responseText); break;
                    }
                }
            };
            request.send(form);
        }
    }
})