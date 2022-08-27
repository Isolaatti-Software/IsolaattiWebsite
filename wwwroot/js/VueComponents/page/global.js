// This is the only Vue.js instance to render all the components

let app;
const data = {
    showAudiosFeedState: 'hide'
}

const methods = {
    
}

if(!renderRouter) {
    app = new Vue({
        el: '#app',
        data,
        methods
    });
} else {
    
    // this is a temporal workaround. Use these routes on squads page
    const routes = getRoutesForPage(); // This functions must be declared on the page
    
    const router = new VueRouter({
        routes
    });

    app = new Vue({
        el: '#app',
        data,
        methods,
        router
    });
}