(function() {

    fetch(`/api/Fetch/PublicThread/${postId}`)
        .then(function (response) {
            console.log(response.json().then(function (value) {
                new Vue({
                    el: "#vue-container",
                    data: {
                        thread: value,
                        audioPlayer: new Audio(),
                        audioUrl: "",
                        playing: false,
                        paused: false,
                        postLinkToShare: ""
                    },
                    methods: {
                        compileMarkdown: function(raw) {
                            return marked(raw)
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
                        }
                    },
                    mounted: function() {
                        let globalThis = this;
                        this.audioPlayer.onended = function() {
                            globalThis.audioUrl = "";
                        }
                    }
                })
            }));
        })
        .catch(function() {
            
        });
    
})();