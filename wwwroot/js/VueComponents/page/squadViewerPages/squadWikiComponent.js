const squadWikiComponent = {
    props:{
        squadId: {
            required: true,
            type: String
        }
    },
    data: function() {
        return {}
    },
    template: `
    <div>Wiki</div>
    `
}

Vue.component('squad-wiki', squadWikiComponent);