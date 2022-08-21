const squadActivityComponent = {
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
    <div>Actividad</div>
    `
}

Vue.component('squad-activity', squadActivityComponent);