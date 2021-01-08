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

function getUserSharedSongs() {
    let formData = new FormData();
    formData.append("userId", userData.id);
    
    let request = new XMLHttpRequest();
    request.open("POST", "/GetUserSharedSongs");
    request.send(formData);
    // response object from API should be an array of objects with following properties:
    // ==> name : "The name of the song"
    // => artist : "The name of the Artist"
    // => uid : " "The-ui-of-the-song"
    // => url : "https://host.example/publicAPI/Shared?uid={uidvalue}
    request.onreadystatechange = function() {
        if(request.readyState === XMLHttpRequest.DONE) {
            JSON.parse(request.response)
            .forEach(function(value, index) {
                console.log(`Element ${index}:`);
                console.log(`Name: ${value.name}`);
                console.log(`Artist: ${value.artist}`);
                console.log(`UID: ${value.uid}`);
                console.log(`URL: ${value.url}`);
            })
        }
    }
}

function copyLinkToClipboard(button,link){
    navigator.clipboard.writeText(link).then(function() {
       button.innerHTML = '<i class="fas fa-check"></i>'; 
    });
}