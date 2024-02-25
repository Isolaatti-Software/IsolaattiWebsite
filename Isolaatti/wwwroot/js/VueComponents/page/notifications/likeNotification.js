Vue.component('like-notification',{
    props: {
        notification: {
            type: Object,
            required: true
        }
    },
    methods: {
        handleClick: function(e) {
            location.href = `/pub/${this.notification.payload.intentData}`;
            return false;
        },
        onDelete: function(e) {
            e.stopPropagation();
            
            return true;
        }
    },
    computed: {
        authorImgUrl: function() {
            return `/api/images/profile_image/of_user/${this.notification.payload.authorId}`
        }
    },
    template: `
      <div class="w-100 list-group-item list-group-item-action pointer" @click="handleClick">
        <div class="d-flex w-100">
            <img class="user-avatar mr-2 mt-auto mb-auto" :src="authorImgUrl"/>
            <i class="fa-solid fa-hands-clapping mr-2  mt-auto mb-auto" />
            <div class="d-flex flex-column">
                <div><b>{{notification.payload.authorName}}</b> aplaude tu publicación</div>
                <small>{{notification.date}}</small>
            </div>
            <button class="btn btn-sm btn-light ml-auto mt-auto mb-auto" @click="onDelete"><i class="fa-solid fa-trash"></i></button>
            
        </div>
      </div>
    `
})