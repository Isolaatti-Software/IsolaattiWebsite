const squadSettingsComponent = {
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
    <div>Ajustes</div>
    `
}

Vue.component('squad-settings', squadSettingsComponent);