/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
* 
* This file should be placed in the Pages/Profile.cshtml
*/
function deletePost(postId, onComplete, onError) {
    let form = new FormData();
    form.append("userId",userData.id);
    form.append("password", userData.password);
    form.append("postId",postId);

    let request = new XMLHttpRequest();
    request.open("POST", "/api/EditPost/Delete");
    request.onreadystatechange = () => {
        if(request.readyState === XMLHttpRequest.DONE) {
            switch(request.status) {
                case 200 : onComplete({
                    statusCode: 200,
                    serverResponse: request.responseText
                }); break;
                case 404 : onError({
                    statusCode: 404,
                    serverResponse: request.responseText
                }); break;
                case 401 : onError({
                    statusCode: 401,
                    serverResponse: request.responseText
                }); break;
                case 500 : onError({
                    statusCode: 500,
                    serverResponse: "Server error. Report this to developer"
                }); break;
                default : onError({
                    statusCode: request.status,
                    serverResponse: request.responseText
                }); break;
            }
        }
    }

    request.send(form);
}

function editPost(postId, newContent, onComplete, onError) {
    let form = new FormData();
    form.append("userId", userData.id);
    form.append("password", userData.password);
    form.append("postId", postId);
    form.append("newContent", newContent);

    let request = new XMLHttpRequest();
    request.open("POST", "/api/EditPost/TextContent");
    request.onreadystatechange = () => {
        if(request.readyState === XMLHttpRequest.DONE) {
            switch(request.status) {
                case 200 : onComplete({
                    statusCode: 200,
                    serverResponse: JSON.parse(request.responseText)
                }, JSON.parse(request.responseText)); break;
                case 404 : onError({
                    statusCode: 404,
                    serverResponse: request.responseText
                }); break;
                case 401 : onError({
                    statusCode: 401,
                    serverResponse: request.responseText
                }); break;
                case 500 : onError({
                    statusCode: 500,
                    serverResponse: "Server error. Report this to developer"
                }); break;
                default : onError({
                    statusCode: request.status,
                    serverResponse: request.responseText
                }); break;
            }
        }
    }
    request.send(form);
}

function changePostPrivacy(postId, privacyNumber, onComplete, onError) {
    let form = new FormData();
    form.append("userId",userData.id);
    form.append("password", userData.password);
    form.append("postId",postId);
    form.append("privacyNumber", privacyNumber);

    let request = new XMLHttpRequest();
    request.open("POST", "/api/EditPost/ChangePrivacy");
    request.onreadystatechange = () => {
        if(request.readyState === XMLHttpRequest.DONE) {
            switch(request.status) {
                case 200 : onComplete({
                    statusCode: 200,
                    serverResponse: JSON.parse(request.responseText)
                }); break;
                case 404 : onError({
                    statusCode: 404,
                    serverResponse: JSON.parse(request.responseText)
                }); break;
                case 401 : onError({
                    statusCode: 401,
                    serverResponse: JSON.parse(request.responseText)
                }); break;
                case 500 : onError({
                    statusCode: 500,
                    serverResponse: "Server error. Report this to developer"
                }); break;
                default : onError({
                    statusCode: request.status,
                    serverResponse: request.responseText
                }); break;
            }
        }
    }

    request.send(form);
}

function copyLinkToClipboard(button,link){
    navigator.clipboard.writeText(link).then(function() {
        button.innerHTML = '<i class="fas fa-check"></i>';
    });
}

function deleteShare(uid) {
    let formData = new FormData();
    formData.append("userId", userData.id);
    formData.append("pwd", userData.password);
    formData.append("uid", uid);

    let request = new XMLHttpRequest();
    request.open("POST", "/api/UnshareSong");
    request.send(formData);
    request.onreadystatechange = function() {
        if(request.readyState === XMLHttpRequest.DONE) {
            alert("Share deleted");
            window.location.reload();
        }
    }
}

// Use this self called function to define events
(function(){
    // "change password form" validation
    let newPasswordField = document.querySelector("#new_password_field");
    let newPasswordConfField = document.querySelector("#new_password_conf_field");
    let editProfileButton = document.getElementById("edit-profile-button");
    let editNameField = document.getElementById("edit-name-field");
    let editEmailField = document.getElementById("edit-email-field");
    let editPostTextarea = document.getElementById("edit-post-modal-simple-post-textarea");
    let editPostPrivacySelector = document.getElementById("edit-post-privacy-selector");
    let deletePostModalButtons = {
        yes: document.querySelector('#delete-post-modal-yes')
    }
    let editPostModalButtons = {
        save: document.querySelector('#post-save-changes')
    }
    
    // this value is used to know what post to modify or delete with modals
    let currentPostIdForModal = 0;

    document.addEventListener("DOMContentLoaded", function(){
        // Filling the fields in edit profile modal with current information
        editEmailField.value = userData.email;
        editNameField.value = userData.name;
    });

    newPasswordConfField.addEventListener("keyup", function() {
        if(newPasswordConfField.value !== newPasswordField.value) {
            newPasswordConfField.classList.add("is-invalid");
        } else {
            if(newPasswordConfField.classList.contains("is-invalid")) {
                newPasswordConfField.classList.remove("is-invalid");
            }
        }
    });
    
    editProfileButton.addEventListener("click", function(event) {
        $('#modal-edit-profile').modal('show');
    });
    
    // when the "delete post modal" is shown, the variable for current post is set, 
    // taking it from the button that opened the modal
    $('#modal-delete-post').on('shown.bs.modal', (event) => {
        console.log("se abre modal");
        let button = $(event.relatedTarget);
        currentPostIdForModal = button.data('postid');
        console.log(currentPostIdForModal);
    });

    // when the "edit post modal" is shown, the variable for current post is set, 
    // taking it from the button that opened the modal. Also, the value for the
    // textarea where the user will modify the content is populated with the current content
    $('#modal-edit-post').on('show.bs.modal', (event) => {
        console.log("Se abre modal para editar post");
        let button = $(event.relatedTarget);
        currentPostIdForModal = button.data('postid');
        editPostTextarea.value = 
            document.querySelector(`div[postid="${currentPostIdForModal}"].post .post-content`).innerHTML;
        let postContainer = document.querySelector(`div[postid="${currentPostIdForModal}"].post`);
        editPostPrivacySelector.value = postContainer.attributes.getNamedItem("privacy").value;
        console.log(currentPostIdForModal);
    });
    
    // These events will call the corresponding functions to delete or modify. The post id parameter is taken from
    // the variable set before (currentPostIdForModal)
    deletePostModalButtons.yes.addEventListener("click", () => {
        deletePostModalButtons.yes.innerHTML = 
            'Deleting <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>';
        deletePostModalButtons.yes.disabled = true;
        deletePost(currentPostIdForModal,(event) => {
           console.log(event);
           deletePostModalButtons.yes.innerHTML = "Yes";
            deletePostModalButtons.yes.disabled = false;
           $('#modal-delete-post').modal('hide');
           document.querySelector(`div[postid="${currentPostIdForModal}"].post`).remove();
       }, () => {
           
       });
    });
    
    editPostModalButtons.save.addEventListener("click", () => {
        editPostModalButtons.save.innerHTML = 
            'Saving <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>';
        editPostModalButtons.save.disabled = true;
        editPost(currentPostIdForModal,editPostTextarea.value,(event) => {
           console.log(event);
           editPostModalButtons.save.innerHTML = "Save";
           editPostModalButtons.save.disabled = false;
           $('#modal-edit-post').modal('hide');
           let postContentInDOM = 
               document.querySelector(`div[postid="${currentPostIdForModal}"].post .post-content`);
           postContentInDOM.innerHTML = event.serverResponse.textContent;
       }, (error)  => {
           
       });
       changePostPrivacy(currentPostIdForModal,editPostPrivacySelector.value,(event) => {
           console.log(event);
           let privacyIconContainer = 
               document.querySelector(`div[postid="${currentPostIdForModal}"].post .privacy-icon-container`);
           switch(event.serverResponse.privacy) {
               case 1 : privacyIconContainer.innerHTML = '<i class="fas fa-user" title="Only you"></i>'; break;
               case 2 : privacyIconContainer.innerHTML = '<i class="fas fa-user-friends" title="People on Isolaatti"></i>';break;
               case 3 : privacyIconContainer.innerHTML = '<i class="fas fa-globe" title="All the world"></i>'; break;
           }
           document.querySelector(`div[postid="${currentPostIdForModal}"].post`)
               .attributes.getNamedItem("privacy").value = event.serverResponse.privacy;
       }, (error) => {
          console.error(error);
       });
    });
    
})();