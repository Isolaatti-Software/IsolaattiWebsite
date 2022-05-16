const audioPlayer = new Vue({
    el: "#audio-player",
    data: {
        relativeUrl: "",
        audioElement: new Audio(),
        paused: false,
        playing: false,
        stopped: false,
        currentTime: 0
    },
    methods: {
        play: async function (url, type, id) {
            const that = this;
            if (url === this.relativeUrl) {
                // the same audio is resumed from paused
                if (this.paused) {
                    this.audioElement.currentTime = this.currentTime;
                    this.audioElement.play().then(function () {
                        that.playing = true;
                        that.paused = false;
                        that.stopped = false;
                        that.currentTime = 0;
                        audioPlayer.$emit("play", that.relativeUrl, type, id);
                    });
                } else if (this.stopped) {
                    this.audioElement.currentTime = 0;
                    this.audioElement.play().then(function () {
                        that.playing = true;
                        that.paused = false;
                        that.stopped = false;
                        that.currentTime = 0;
                        audioPlayer.$emit("play", that.relativeUrl, type, id);
                    });
                }
                return;
            }
            // Another audio is played
            // First I emit stop event
            audioPlayer.$emit("stop", this.relativeUrl);

            this.relativeUrl = url;
            this.audioElement.src = this.relativeUrl;
            this.audioElement.play().then(function () {
                that.playing = true;
                that.paused = false;
                that.stopped = false;
                that.currentTime = 0;
                that.$emit("play", that.relativeUrl, type, id);
            });
        },
        pause: function () {
            this.audioElement.pause();
            this.currentTime = this.audioElement.currentTime;
            this.playing = false;
            this.stopped = false;
            this.paused = true;
            this.$emit("pause", this.relativeUrl);
        },
        stop: function () {
            this.audioElement.pause();
            this.audioElement.currentTime = 0;
            this.paused = false;
            this.playing = false;
            this.stopped = true;
            this.$emit("stop", this.relativeUrl);
        }
    },
    mounted: function () {
        const globalThis = this;
        this.audioElement.onended = function () {
            globalThis.currentTime = 0;
            globalThis.stopped = true;
            globalThis.playing = false;
            globalThis.paused = false;
            globalThis.$emit("stop", globalThis.relativeUrl);
        }
    }
});

const globalEventEmmiter = new Vue();