// This is the only Vue.js instance to render all the components

let app;
const data = {
    showTopBarMobileMenu: false,
    renderBackdrop: false,
    showingMobileSearch: false
}

const methods = {
    toggleTopBarShowMenu: function() {
        this.showTopBarMobileMenu = !this.showTopBarMobileMenu;
    },
    onBackdropClick: function() {
        events.$emit('backdrop-clicked');
    },
    mobileSearchToggle: function() {
        // disappear-on-search
        this.showingMobileSearch = !this.showingMobileSearch
        
    },
    closeMobileSearch: function() {
        this.showingMobileSearch = false;
        this.onBackdropClick();
    }
}

// This functions is called when app is mounted.
// Initialize here global events related to UI.
// Use 'events' Vue instance to broadcast global events
function mounted() {
    const that = this;
    events.$on('backdrop-toggle', function(value){
        that.renderBackdrop = value;
    })
    that.$refs.footer.style.display = '';
}

if(!renderRouter) {
    app = new Vue({
        el: '#app',
        data,
        methods,
        mounted
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
        router,
        mounted
    });
}