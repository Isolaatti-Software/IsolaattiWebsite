const squadContentComponent = {
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
    <div>Contenido</div>
    `
}

Vue.component('squad-content', squadContentComponent);