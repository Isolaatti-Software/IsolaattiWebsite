/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
* 
* This file should be placed in the Pages/WebApp/Songs.cshtml
*/

// "change password form" validation
let newPasswordField = document.querySelector("#new_password_field");
let newPasswordConfField = document.querySelector("#new_password_conf_field");

newPasswordConfField.addEventListener("keyup", function() {
    if(newPasswordConfField.value !== newPasswordField.value) {
        newPasswordConfField.classList.add("is-invalid");
    } else {
        if(newPasswordConfField.classList.contains("is-invalid")){
            newPasswordConfField.classList.remove("is-invalid");
        }
    }
});