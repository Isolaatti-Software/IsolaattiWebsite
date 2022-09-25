Vue.component('squad-requests-list', {
    props: {
        items: {
            required: true,
            type: Array
        }
    },
    data: function() {
        return {
            selectedRequestId: undefined
        }
    },
    methods: {
        selectRequest: function(id) {
            if(this.selectedRequestId === id) {
                this.selectedRequestId = undefined;
            } else {
                this.selectedRequestId = id;
            }

        },
        profileLink: function(profileId) {
            return `/perfil/${profileId}`;
        },
        profileImageLink: function(profileId) {
            return `/api/Fetch/GetUserProfileImage?userId=${profileId}`
        }
    },
    template: `
        
    `
});