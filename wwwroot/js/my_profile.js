/*
* Isolaatti project
* Erik Cavazos, 2020, 2021
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
* 
* This file should be placed in the Pages/Profile.cshtml
*/


const profilePictureLoadFormElement = document.getElementById("profilePictureLoadFormElement");
const canvas = document.getElementById("previewProfilePicture");
const canvasContext = canvas.getContext('2d');
const previewProfilePictureImage = new Image();

const uploadProfilePhoto = document.getElementById("uploadProfilePhoto");

uploadProfilePhoto.addEventListener("click", function() {
    if(previewProfilePictureImage.src == null) {
        return;
    }
    let form = new FormData();
    canvas.toBlob(function(blob) {
        form.append("file",blob);

        let request = new XMLHttpRequest();
        request.open("POST", "/api/EditProfile/UpdatePhoto");
        request.setRequestHeader("sessionToken", sessionToken);
        request.onload = function () {
            if (request.status === 200) {
                console.log(request.responseText);
                window.location.reload();
            }
        }
        request.send(form); 
    });

});

previewProfilePictureImage.addEventListener("load", function() {
    uploadProfilePhoto.disabled = false;
    // horizontal
    if(previewProfilePictureImage.width > previewProfilePictureImage.height) {
        let constant = previewProfilePictureImage.height / 120;
        let margin = (120 - (previewProfilePictureImage.width/constant))/2;
        canvasContext.drawImage(previewProfilePictureImage, margin, 0, previewProfilePictureImage.width/constant,previewProfilePictureImage.height/constant);
    }

    // vertical
    else if(previewProfilePictureImage.width < previewProfilePictureImage.height) {
        let constant = previewProfilePictureImage.width / 120;
        let margin = (120 - (previewProfilePictureImage.height/constant))/2;
        canvasContext.drawImage(previewProfilePictureImage, 0, margin, previewProfilePictureImage.width/constant, previewProfilePictureImage.height/constant);
    } else {
        canvasContext.drawImage(previewProfilePictureImage, 0, 0, 120, 120);
    }
});


profilePictureLoadFormElement.addEventListener("input", function () {
    let file = profilePictureLoadFormElement.files[0];
    document.getElementById("fileNameProfilePic").innerHTML = file.name;
    let fileReader = new FileReader();
    fileReader.readAsDataURL(file);
    fileReader.onload = function () {
        previewProfilePictureImage.src = fileReader.result;
    };
});

document.querySelector("#setColorBtn").addEventListener('click', function () {
    const htmlColor = document.querySelector('#profileColorField').value;

    fetch('/api/EditProfile/SetProfileColor', {
        method: "POST",
        headers: customHttpHeaders,
        body: JSON.stringify({data: htmlColor})
    }).then(res => {
        window.location.reload()
    })

});

// "change password form" validation
let newPasswordField = document.querySelector("#new_password_field");
let newPasswordConfField = document.querySelector("#new_password_conf_field");
let editProfileButton = document.getElementById("edit-profile-button");
let editNameField = document.getElementById("edit-name-field");
let editEmailField = document.getElementById("edit-email-field");

// this value is used to know what post to modify or delete with modals
let currentPostIdForModal = 0;
document.addEventListener("DOMContentLoaded", function () {
    // Filling the fields in edit profile modal with current information
    editEmailField.value = userData.email;
    editNameField.value = userData.name;

    function fetchMyPosts(lastId, event) {
        this.loading = true;
        if (event !== undefined) {
            event.target.disabled = true;
        }
        fetch(`api/Fetch/PostsOfUser/${this.userData.id}/8/${lastId}?olderFirst=${this.sortingData.ascending === "1" ? "True" : "False"}`, {headers: this.customHeaders}).then(result => {
            result.json().then(res => {
                this.posts = this.posts.concat(res.feed);
                this.moreContent = res.moreContent;
                this.loading = false;
                if (event !== undefined) {
                    event.target.disabled = false;
                }
                this.lastId = res.lastId;
            })
        })
    }

    function fetchComments() {
        fetch(`api/Fetch/Post/${this.commentsViewer.postId}/Comments`, {
            headers: this.customHeaders
        }).then(res => {
            res.json().then(comments => {
                this.commentsViewer.comments = comments;
            })
        })
    }

    async function loadProfileLink() {
        const that = this;
        let response = await fetch(`/api/UserLinks/Get/${this.userData.id}`, {
            method: "GET",
            headers: this.customHeaders
        });

        if (!response.ok) {
            this.userLink.error = true
            return;
        }
        try {
            let parsedResponse = await response.json();
            this.userLink.isCustom = parsedResponse.isCustom;
            const that = this;
            if (this.userLink.isCustom) {
                this.userLink.customId = parsedResponse.customId;
                this.userLink.url = "https://isolaatti.com/" + this.userLink.customId;
                new QRious({
                    element: document.getElementById('user-profile-link-qr'),
                    value: that.userLink.url
                });
            } else {
                this.userLink.url = parsedResponse.url;
                await that.createCustomLink();
            }

        } catch (error) {
            this.userLink.error = true;
        }
    }

    async function createCustomLink() {
        let response = await fetch("/api/UserLinks/Create", {
            method: "POST",
            headers: this.customHeaders
        });
        if (response.ok) {
            const that = this;
            let parsedResponse = await response.json();
            this.userLink.customId = parsedResponse.id;
            this.userLink.url = "https://isolaatti.com/" + this.userLink.customId;
            new QRious({
                element: document.getElementById('user-profile-link-qr'),
                value: that.userLink.url
            });
        }
    }

    async function modifyCustomLink() {
        const that = this;
        let response = await fetch("api/UserLinks/ChangeUserLink", {
            method: "POST",
            headers: this.customHeaders,
            body: JSON.stringify({
                data: that.userLink.customId
            })
        });
        if (response.ok) {
            this.userLink.isCustom = true;
            this.userLink.error = false;
            this.userLink.available = true;
            $('#modal-custom-user-link').modal('hide');
        } else if (response.status === 400) {
            this.userLink.error = true;
        } else if (response.status === 401) {
            this.userLink.available = false
        }
    }

    let vueContainer = new Vue({
        el: '#vue-container',
        data: {
            customHeaders: customHttpHeaders,
            userData: userData,
            userLink: {
                isCustom: false,
                url: "",
                customId: "",
                available: true,
                error: false,
                isValid: true
            },
            currentSection: "discussions" // other are "audios", and "profile-pictures"
        },
        methods: {
            loadProfileLink: loadProfileLink,
            createCustomLink: createCustomLink,
            modifyCustomLink: modifyCustomLink,
            validateCustomLink: function () {
                const regex = new RegExp('^([a-zA-Z0-9 _-]+)$')
                this.userLink.isValid = regex.test(this.userLink.customId)
                if (this.userLink.isValid) {
                    this.userLink.url = "https://isolaatti.com/" + this.userLink.customId;
                    new QRious({
                        element: document.getElementById('user-profile-link-qr'),
                        value: this.userLink.url
                    });
                } else {
                    const canvas = document.getElementById('user-profile-link-qr');
                    canvas.getContext('2d').clearRect(0, 0, canvas.width, canvas.height);

                }
            }
        },
        mounted: function () {
            this.$nextTick(function () {
                let globalThis = this;
                this.loadProfileLink();
            });
        }
    })

    // vue instance to manage profile audio description
    let vueContainerForLeftBar = new Vue({
        el: '#vue-container-for-left-bar',
        data: {
            customHeaders: customHttpHeaders,
            userData: userData,
            audioDescriptionUrl: audioDescriptionUrl, // this value is defined on the page (rendered by te server)
            playing: false,
            paused: false,
            recorder: {
                mediaStream: null,
                audioBlob: null,
                timeInSeconds: 0,
                recording: false,
                recorded: false,
                mediaRecorder: null,
                audioData: [],
                consolidated: false,
                timerReference: null,
                stopTimeoutReference: null
            },
            noAudioSource: false,
            isUploadingAudio: false
        },
        computed: {
            clockFormatTime: function () {
                let truncatedSecs = Math.round(this.recorder.timeInSeconds);
                let minutes = truncatedSecs / 60;
                let seconds = truncatedSecs % 60;
                if (seconds < 10) {
                    seconds = `0${seconds}`
                }
                return `${Math.trunc(minutes)}:${seconds}`;
            },
            audioDescriptionButtonInnerText: function () {
                if (this.playing) {
                    return '<i class="far fa-pause-circle"></i>'
                } else if (this.paused) {
                    return '<i class="far fa-play-circle"></i>'
                } else {
                    return '<i class="far fa-play-circle"></i>'
                }
            },
            progressStyle: function () {
                return `width:${(this.recorder.timeInSeconds * 100) / 120}%`
            }
        },
        methods: {
            uploadAudioDescriptionAudio: function () {
                const globalThis = this;
                // first I need to upload the audio blob to google cloud storage
                if (this.recorder.consolidated) {
                    let storageRef = storage.ref();

                    let uploadTask = storageRef.child(`audio_descriptions/${this.userData.id}/audio.webm`);
                    this.isUploadingAudio = true;
                    uploadTask.put(this.recorder.audioBlob).then(function (s) {

                        // then get the url and send it to the backend to store it on the database
                        uploadTask.getDownloadURL().then(function (url) {
                            fetch('/api/EditProfile/UpdateAudioDescription', {
                                method: "POST",
                                headers: globalThis.customHeaders,
                                body: JSON.stringify({data: url})
                            }).then(res => window.location.reload())
                        }).catch(error => this.isUploadingAudio = false)
                    })
                }
            },
            startRotatingProfilePicture: function () {
                if (this.paused) {
                    this.$refs.profilePhoto.getAnimations().forEach(function (element) {
                        element.play();
                    });
                } else {
                    this.$refs.profilePhoto.animate([
                        {transform: 'rotate(0deg)'},
                        {transform: 'rotate(360deg)'}
                    ], {
                        duration: 2800,
                        iterations: Infinity
                    });
                }

            },
            pauseRotatingProfilePicture: function () {
                this.$refs.profilePhoto.getAnimations().forEach(function (element) {
                    element.pause();
                });
            },
            stopRotatingProfilePicture: function () {
                this.$refs.profilePhoto.getAnimations().forEach(function (element) {
                    element.cancel()
                })
            },
            requestMicrophone: function () {
                let audioStreamPromise = navigator.mediaDevices.getUserMedia({audio: true, video: false})

                audioStreamPromise.then((stream) => {
                    this.recorder.mediaStream = stream;
                    this.noAudioSource = false;
                }).catch(error => {
                    this.noAudioSource = true;
                });
            },
            onEditProfileAudioDescriptionModal: function () {
                if (this.recorder.mediaStream === null) {
                    this.requestMicrophone();
                }
            },
            playAudioDescription: function () {
                // I need to call this method that is on the other instance, so that I don't have to 
                // create more than one audio player, and for instance the audio description plays 
                // instead anything that is playing at the moment

                vueContainer.playAudio(this.audioDescriptionUrl);

                // this means user wants to pause
                if (this.playing) {
                    this.pauseRotatingProfilePicture();
                    this.paused = true;
                    this.playing = false;
                } else {
                    this.playing = true;
                    this.startRotatingProfilePicture();
                }
            },
            startRecording: function () {
                this.recorder.consolidated = false;
                this.recorder.timeInSeconds = 0;
                this.recorder.mediaRecorder = new MediaRecorder(this.recorder.mediaStream, {
                    mimeType: 'audio/webm; codecs:opus',
                    bitsPerSecond: 128000
                });
                this.recorder.mediaRecorder.start();
                const globalThis = this;

                // this timer counts every second and updated UI to show time
                this.recorder.timerReference = setInterval(function () {
                    globalThis.recorder.timeInSeconds++;
                }, 1000);

                // recording will stop at 2 minutes
                this.recorder.stopTimeoutReference = setTimeout(function () {
                    clearInterval(globalThis.recorder.timerReference);
                }, 120000);


                this.recorder.mediaRecorder.ondataavailable = function (e) {
                    globalThis.recorder.audioData.push(e.data);
                }

                this.recorder.mediaRecorder.onstop = function () {
                    globalThis.recorder.audioBlob = new Blob(globalThis.recorder.audioData,
                        {'type': 'audio/webm; codecs=opus'});

                    clearInterval(globalThis.recorder.timerReference); // stops timer
                    clearTimeout(globalThis.recorder.stopTimeoutReference); // stops timeout that was set to execute to minutes after

                    globalThis.recorder.consolidated = true;
                }
                this.recorder.recording = true;
            },
            stopRecording: function () {
                this.recorder.mediaRecorder.stop();
                this.recorder.recording = false;
                this.recorder.recorded = true;
            },
            playAudioDescriptionPreview: function () {
                vueContainer.playAudio(window.URL.createObjectURL(this.recorder.audioBlob));
            },
            resetRecording: function () {
                this.recorder.recording = false;
                this.recorder.audioBlob = null;
                this.recorder.audioData = [];
                this.recorder.timeInSeconds = 0;
                this.recorder.recorded = false;
            }

        },
        mounted: function () {
            const globalThis = this;
            this.$nextTick(function () {

                // I am sorry I have to do it this way, but it's easier than adding 
                // events to the button that is outside vue.js
                $('#modal-audio-description').on('show.bs.modal', function () {
                    globalThis.requestMicrophone();
                });
            });
        }
    })
});

newPasswordConfField.addEventListener("keyup", function () {
    if (newPasswordConfField.value !== newPasswordField.value) {
        newPasswordConfField.classList.add("is-invalid");
    } else {
        if (newPasswordConfField.classList.contains("is-invalid")) {
            newPasswordConfField.classList.remove("is-invalid");
        }
    }
});
