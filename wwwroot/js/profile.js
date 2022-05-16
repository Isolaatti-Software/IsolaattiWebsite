/*
* Isolaatti project
* Erik Cavazos, 2021, 2022
* Other's Profile page
*/

(function () {

    let followButton = document.querySelector("#followButton");
    followButton.addEventListener("click", function () {
        followButton.disabled = true;
        if (followingThisUser) {
            fetch("/api/Following/Unfollow", {
                method: "POST",
                headers: customHttpHeaders,
                body: JSON.stringify({id: accountData.userId})
            }).then(res => {
                followButton.innerHTML = 'Seguir <i class="fas fa-check"></i>';
                followingThisUser = false;
                followButton.disabled = false;
            })
        } else {
            fetch("/api/Following/Follow", {
                method: "POST",
                headers: customHttpHeaders,
                body: JSON.stringify({id: accountData.userId})
            }).then(res => {
                followButton.innerHTML = 'Dejar de seguir <i class="fas fa-times"></i>';
                followingThisUser = true;
                followButton.disabled = false
            })
        }

    });

    let vueContainer = new Vue({
        el: '#vue-container',
        data: {
            customHeaders: customHttpHeaders,
            accountData: accountData,
        },
    });


    /*****************************************************************************************************************/


    const leftBarVueJsInstance = new Vue({
        el: '#profile_photo_container',
        data: {
            sessionToken: sessionToken,
            userData: userData,
            audioDescriptionUrl: audioDescriptionUrl, // this value is defined on the page (rendered by te server)
            playing: false,
            paused: false
        },
        computed: {
            audioDescriptionButtonInnerText: function() {
                if(this.playing){
                    return '<i class="far fa-pause-circle"></i>'
                } else if(this.paused){
                    return '<i class="far fa-play-circle"></i>'
                } else {
                    return '<i class="far fa-play-circle"></i>'
                }
            }
        },
        methods: {
            // methods for audio description behaviour
            startRotatingProfilePicture: function() {
                if(this.paused) {
                    this.$refs.profilePictureElement.getAnimations().forEach(function (element) {
                        element.play();
                    });
                } else {
                    this.$refs.profilePictureElement.animate([
                        {transform: 'rotate(0deg)'},
                        {transform: 'rotate(360deg)'}
                    ], {
                        duration: 2800,
                        iterations: Infinity
                    });
                }

            },
            pauseRotatingProfilePicture: function() {
                this.$refs.profilePictureElement.getAnimations().forEach(function(element){
                    element.pause();
                });
            },
            stopRotatingProfilePicture: function() {
                this.$refs.profilePictureElement.getAnimations().forEach(function(element){element.cancel()})
            },
            playAudioDescription: function() {
                // I need to call this method that is on the other instance, so that I don't have to 
                // create more than one audio player, and for instance the audio description plays 
                // instead anything that is playing at the moment

                vueContainer.playAudio(this.audioDescriptionUrl);

                // this means user wants to pause
                if(this.playing) {
                    this.pauseRotatingProfilePicture();
                    this.paused = true;
                    this.playing = false;
                } else {
                    this.playing = true;
                    this.startRotatingProfilePicture();
                }
            }
        },
        mounted: function(){

        }
    });
    
})();