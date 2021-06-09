(function() {
    let form = new FormData();
    form.append("id", postId);
    
    let postRequest = new Request("/api/GetPost/PublicThread",{method:"POST", body: form, headers:{'Accept': 'text/json'}});
    fetch(postRequest)
        .then(function(response) {
            console.log(response.json().then(function(value) {
                new Vue({
                    el: "#vue-container",
                    data: {
                        thread: value,
                        audioPlayer: new Audio(),
                        audioUrl: "",
                        playing: false,
                        paused: false
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
                })
            }));
        })
        .catch(function() {
            
        });
    
})();