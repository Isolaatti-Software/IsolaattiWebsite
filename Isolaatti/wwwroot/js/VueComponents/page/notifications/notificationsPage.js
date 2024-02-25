Vue.component('notifications-page',{
    data: function() {
        return {
            lastPageSize: 0,
            notifications: []
        }
    },
    methods: {
        getParams: function() {
            if(this.notifications.length < 20) {
                return "";
            }
            if(this.lastPageSize === 20) {
                return `after=${this.notifications[this.notifications.length - 1]}`
            }
        },
        fetchNotifications: async function() {
            const url = `/api/notifications/list?${this.getParams()}`
            
            const response = await fetch(url);
            if(response.ok) {
                const json = await response.json();
                this.notifications = this.notifications.concat(json.result);
            }
            
        }
    },
    mounted: async function() {
        await this.fetchNotifications();
    },
    template: `
    <div class="d-flex flex-column">
        <h5 class="mt-3 text-center"><i class="fa-solid fa-bell"></i> Notificaciones</h5>
        <div class="d-flex justify-content-end mb-2">
            <button class="btn btn-light">
                <i aria-hidden="true" class="fas fa-ellipsis-h"></i>
            </button>
        </div>
        <div class="d-flex flex-column">
            <notification v-for="notification in notifications" :key="notification.id" :notification="notification"/>
        </div>
    </div>
    `
})