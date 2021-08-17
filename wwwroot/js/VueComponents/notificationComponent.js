

Vue.component('notification',{
    props: ['notification-obj'],
    data: function() {
        return {}
    },
    computed: {
        title: function() {
            let res = "";
            switch (this.notificationObj.type) {
                case 1: res = "Someone liked your post."; break;
                case 2: res = `Someone commented on your post`; break;
                case 3: res = `New follower`; break;
            }
            
            return res;
        },
        notificationData: function() {
            return JSON.parse(this.notificationObj.dataJson);
        },
        dateTime: function (){
            return new Date(this.notificationObj.timeSpan);
        }
    },
    methods: {
        
    },
    template: `
      <div class="notification-item m-1">
        <h6 class="m-2">{{this.title}}</h6>
        <small>{{this.dateTime}}</small>
      </div>
    `
})