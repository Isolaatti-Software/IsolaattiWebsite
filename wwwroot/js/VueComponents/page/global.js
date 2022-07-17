// This is the only Vue.js instance to render all the components
const globalEventEmmiter = new Vue();
new Vue({
    el: '#app',
    data: {
        showAudiosFeedState: 'hide'
    },
    methods: {
        toggleAudiosFeed: function () {
            if (this.showAudiosFeedState === 'show') {
                this.showAudiosFeedState = 'collapse'
                return;
            }

            if (this.showAudiosFeedState === 'collapse' || this.showAudiosFeedState === 'hide')
                this.showAudiosFeedState = 'show'
        }
    }
});