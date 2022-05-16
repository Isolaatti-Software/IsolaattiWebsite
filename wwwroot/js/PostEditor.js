let markdownInput = document.getElementById("raw-post-markdown");
let audioContext = new AudioContext();

const vue = new Vue({
    el: "#vue-container"
});

globalEventEmmiter.$on("posted", function () {
    window.location = "/";
})
globalEventEmmiter.$on("audio-posted", function () {
    window.location = "/tu";
})