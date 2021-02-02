/*
* Isolaatti project
* Erik Cavazos, 2021
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
* 
* This file should be placed in the Pages/WebApp/Settings.cshtml
*/

(function() {
    let notifyByEmailSwitch = document.querySelector("#notifyByEmail");
    let notifyWhenProcessStartsSwitch = document.querySelector("#notifyWhenProcessStarts");
    let notifyWhenProcessFinishesSwitch = document.querySelector("#notifyWhenProcessFinishes");
    
    notifyByEmailSwitch.addEventListener("change", function() {
        setPreference(0, notifyByEmailSwitch.checked);
    });
    
    notifyWhenProcessStartsSwitch.addEventListener("change", function() {
       setPreference(1, notifyWhenProcessStartsSwitch.checked); 
    });
    
    notifyWhenProcessFinishesSwitch.addEventListener("change", function() {
       setPreference(2, notifyWhenProcessFinishesSwitch.checked); 
    });
})();

function setPreference(what, value) {
    let formData = new FormData();
    formData.append("userId", userData.id);
    formData.append("what",what);
    formData.append("value", value);
    
    let request = new XMLHttpRequest();
    request.open("POST","/api/SetPreferences");
    request.send(formData);
    request.onreadystatechange = function() {
        if(request.readyState === XMLHttpRequest.DONE) {
            //console.log("Preferences updated");
        }
    }
}

function deleteAllSongs() {
    if(confirm("Are you sure you want to delete all your songs?")) {
        let formData = new FormData();
        formData.append("userId", userData.id);
        formData.append("password", userData.password);

        let request = new XMLHttpRequest();
        request.open("POST", "/api/DeleteSong/All");
        request.send(formData);
        request.onreadystatechange = function() {
            if(request.readyState === XMLHttpRequest.DONE) {
                alert("All your song were deleted!");
                window.location.reload();
            }
        }
    }
}

function deleteAllShares() {
    if(confirm("Are you sure you want to delete all your shares?")) {
        let formData = new FormData();
        formData.append("userId", userData.id);
        formData.append("password", userData.password);
        
        let request = new XMLHttpRequest();
        request.open("POST", "/api/UnshareSong/All");
        request.send(formData);
        request.onreadystatechange = function() {
            if(request.readyState === XMLHttpRequest.DONE) {
                alert("All your shares were deleted!");
                window.location.reload();
            }
        }
    }
}