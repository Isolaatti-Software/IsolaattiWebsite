/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
* 
* This file should be placed in the Pages/WebApp/Songs.cshtml
*/

function deleteSong(songId) {
    if(confirm("Do you really want to delete that song?")) {
        let formData = new FormData();
        formData.append("songId", songId);
        formData.append("userId", userData.id);
        formData.append("password", userData.password);

        let request = new XMLHttpRequest();
        request.open("POST", "/api/DeleteSong");
        request.send(formData);
        request.onreadystatechange = function() {
            if(request.readyState === XMLHttpRequest.DONE) {
                console.log("Song with Id " + songId + " has been deleted");
                document.getElementById("songid_" + songId).remove();
                deleteFiles(request.responseText);
            }
        }
    }
}

let storageRef = storage.ref();

function deleteFiles(uid) {
    // TODO: delete every console.log when everything is working correct
    
    // for now, as there are only the 4 base tracks, they are deleted one by one
    storageRef.child(`results/${userData.id}/${uid}/bass.mp3`).delete()
        .then(function() {
            console.log("bass deleted from bucket");
        }).catch();
    
    storageRef.child(`results/${userData.id}/${uid}/drums.mp3`).delete()
        .then(function() {
            console.log("drums deleted from bucket");
        }).catch();
    
    storageRef.child(`results/${userData.id}/${uid}/vocals.mp3`).delete()
        .then(function() {
            console.log("vocals deleted from bucket");
        }).catch();
    
    storageRef.child(`results/${userData.id}/${uid}/other.mp3`).delete()
        .then(function() {
            console.log("other was deleted from bucket");
        }).catch();
}