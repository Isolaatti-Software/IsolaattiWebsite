Vue.component('notification',{
    props: {
        notification: {
            type: Object,
            required: true
        }
    },
    data: function() {
        return {}
    },
    computed: {
        notificationTitle: function() {
            switch(this.notification.payload.type) {
                case "like": 
                    return "Se ha dado like";
                
            }
        }
    },
    template: `
<div class="list-group list-group-flush">
    <like-notification v-if="notification.payload.type === 'like' " :notification="notification"/>
</div>
    `
})