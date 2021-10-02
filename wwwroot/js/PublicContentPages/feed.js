new Vue({
    el: '#vue-container',
    data: {
        audioPlayer: new Audio(),
        posts: [],
        postLinkToShare: "",
        audioUrl: "",
        paused: false,
        playing: false
    },
    computed: {
        openThreadLink: function() {
            return `/Threads/${this.commentsViewer.postId}`;
        }
    },
    methods: {
        getFeed: function(mostLiked) {
            const globalThis = this;
            const path = mostLiked ? "/api/Feed/Public?mostLiked=True" : "/api/Feed/Public"
            fetch(path).then(function(res) {
                res.json().then(function(obj){
                    globalThis.posts = obj
                });
            });
            
        },
        copyToClipboard: function(relativeUrl) {
            let absoluteUrl = `${window.location.protocol}//${window.location.host}${relativeUrl}`;
            navigator.clipboard.writeText(absoluteUrl).then(function() {
                alert("Se copió el texto al portapapeles");
            });
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
        }
    },
    mounted: function() {
        this.$nextTick(function() {
            this.getFeed(false); // false bc dont wanna get most liked
            this.audioPlayer.onended = function() {
                globalThis.audioUrl = "";
            }
        });
    }
});