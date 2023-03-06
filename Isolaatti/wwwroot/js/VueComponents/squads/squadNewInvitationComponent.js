Vue.component('squad-new-invitation', {
    props: {
        squadId: {
            type: String,
            required: true
        }
    },
    methods: {

    },
    template: `
    <section>
    <squad-invitation-creator-user-searcher
        :auto-send="false"
        :squad-id="squadId"
        @created="$emit('created')"
    ></squad-invitation-creator-user-searcher>
    </section>
    `
})