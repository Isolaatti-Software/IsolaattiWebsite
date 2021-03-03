/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
* 
* This file should be placed in the Pages/WebApp/Profile.cshtml
*/

// "change password form" validation
let newPasswordField = document.querySelector("#new_password_field");
let newPasswordConfField = document.querySelector("#new_password_conf_field");

newPasswordConfField.addEventListener("keyup", function() {
    if(newPasswordConfField.value !== newPasswordField.value) {
        newPasswordConfField.classList.add("is-invalid");
    } else {
        if(newPasswordConfField.classList.contains("is-invalid")) {
            newPasswordConfField.classList.remove("is-invalid");
        }
    }
});

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


let editProfileButton = document.getElementById("edit-profile-button");
editProfileButton.addEventListener("click", function(event) {
    $('#modal-edit-profile').modal('show');
});

let saveEditedProfileButton = document.getElementById("save-edited-profile");
saveEditedProfileButton.addEventListener("click", function(event) {
    
});

function editProfile(newName, newEmail) {
    let formData = new FormData();
    formData.append("userId", userData.id);
    formData.append("password", userData.password);
    formData.append("newEmail", newEmail);
    formData.append("newUsername", newName);
    
    let request = new XMLHttpRequest();
    request.open("POST", "/api/EditProfile");
    request.send(formData);
    request.onreadystatechange = function() {
        if(request.readyState === XMLHttpRequest.DONE) {
            switch (request.status) {
                case 200 : console.log("all good"); break;
                case 401 : console.log("profile was not updated"); //TODO: manage statuses
                    break;
                default : console.log("An error occurred, report!");
            }
        }
    }
}