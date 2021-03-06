const vueInstanceForNotifications = new Vue({
    el: "#vue-container-for-notifications",
    data: {
        notificationSliderShowing: false,
        sessionToken: sessionToken,
        notifications: []
    },
    computed: {
      unreadCount: function() {
          
          let count = this.notifications.filter((value, index, array) => {
              return !value.read
          }).length.toString();
          document.getElementById("unread-count").innerText = count;
          return count;
      }  
    },
    methods: {
        toggleNotificationsSlider: function() {
            if(this.notificationSliderShowing){
                this.$refs.notificationsSlider.style.animation = "hideNotificationsContainer";
                this.$refs.notificationsSlider.style.animationDuration = "0.4s";
                this.$refs.notificationsSlider.style.animationFillMode = "forwards";
            } else {
                this.$refs.notificationsSlider.style.animation = "showNotificationsContainer";
                this.$refs.notificationsSlider.style.animationDuration = "0.4s";
                this.$refs.notificationsSlider.style.animationFillMode = "forwards";

                this.markAsRead();
            }
            this.notificationSliderShowing = !this.notificationSliderShowing;
            
            
        },
        fetchNotifications: function() {
            let form = new FormData();
            form.append("sessionToken",this.sessionToken);
            
            let request = new XMLHttpRequest();
            request.open("POST","/api/Notifications/GetAllNotifications");
            request.onload = () => {
               if(request.status !== 500){
                   this.notifications = JSON.parse(request.responseText)
               }
            };
            
            request.send(form);
            
        },
        
        markAsRead: function() {
            if(this.unreadCount > 0) {
                this.notifications.forEach((value => value.read = true));
                
                let form = new FormData();
                form.append("sessionToken", this.sessionToken);
                
                let request = new XMLHttpRequest();
                request.open("POST", "/api/Notifications/MarkAsRead");
                request.onload = () => {
                    if(request.status !== 500) {
                        console.log("all notifications marked as read");
                    }
                }
                request.send(form);
            }
        }
    },
    mounted: function(){
        const globalThis = this;
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/notifications_hub").build();
        
        // types of notification
        const types = {
            like: 1,
            post: 2,
            followers: 3
        }

        async function start() {
            try {
                await connection.start();
                console.log("Conexion exitosa");
                
                connection.on("SessionSaved", function(serverResponse) {
                    console.log(serverResponse);
                });

                connection.on("hola", function(data) {
                    alert(data);
                });
                
                connection.on("fetchNotification", function(data, type, comment /* nullable */) {
                    globalThis.fetchNotifications();
                    switch (type) {
                        case types.like: /* hacer algo */ break;
                        case types.post:
                            // vue.js instance from thread page
                            if(thisVueInstance.post.id === comment.simpleTextPostId) {
                                thisVueInstance.addAComment(comment);
                            }
                            break;
                        case types.followers: /* hacer algo */ break;
                    }
                });
            } catch(err) {
                console.log(err);
                setTimeout(start, 5000);
            }
        }

        connection.onclose(start);

        start();
        
        this.fetchNotifications();
    }
});