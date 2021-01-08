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
    request.open("POST", "/UnshareSong");
    request.send(formData);
    request.onreadystatechange = function() {
        if(request.readyState === XMLHttpRequest.DONE) {
            alert("Share deleted");
            window.location.reload();
        }
    }
}