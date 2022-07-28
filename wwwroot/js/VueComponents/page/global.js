// This is the only Vue.js instance to render all the components
const globalEventEmmiter = new Vue();

const data = {
    showAudiosFeedState: 'hide'
}

const methods = {
    toggleAudiosFeed: function () {
        if (this.showAudiosFeedState === 'show') {
            this.showAudiosFeedState = 'collapse'
            return;
        }

        if (this.showAudiosFeedState === 'collapse' || this.showAudiosFeedState === 'hide')
            this.showAudiosFeedState = 'show'
    }
}

if(!renderRouter) {
    const app = new Vue({
        el: '#app',
        data,
        methods
    });
} else {
    
    // this is a temporal workaround. Use these routes on squads page
    const routes = [
        { path: "/", component: squadsFeed },
        { path: "/crear", component: createSquadComponent },
        { path: "/tuyos", component: yourSquadsComponent }
    ]
    
    const router = new VueRouter({
        routes,
    });

    const app = new Vue({
        el: '#app',
        data,
        methods,
        router
    });
}