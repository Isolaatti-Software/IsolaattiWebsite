/*
* Isolaatti project
* Erik Cavazos, 2020
* Last modified Jan 31 2021
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
* 
* This file should be placed in the Pages/Songs.cshtml
*/

//                    Web api calls
/* /api/DeleteSong                 :   Method: POST
*                                     Description: "Deletes the record indicated by the id. Returns the uid of that project and the urls"
*                                     Form data parameters: sessionToken
* 
* /api/IsTrackUrlUsedBySomeoneElse:   Method: POST
*                                     Description: "Returns true if the url passed is used by an user other that the current
*                                     FormData parameters: url, userWhoAsksId
*/

let storageRef = storage.ref();

function deleteSong(songId) {
    if(confirm("Do you really want to delete that song?")) {
        let formData = new FormData();
        formData.append("songId", songId);
        formData.append("sessionToken", sessionToken);

        let request = new XMLHttpRequest();
        request.open("POST", "/api/DeleteSong");
        request.send(formData);
        request.onreadystatechange = function() {
            if(request.readyState === XMLHttpRequest.DONE) {
                console.log("Song with Id " + songId + " has been deleted");
                document.getElementById("songid_" + songId).remove();
                deleteFiles(request.response);
            }
        }
    }
}

function deleteFiles(data) {
    // TODO: delete every console.log when everything is working correct
    // As files might be being used by another account (shared), it's necessary to know which files
    // should not be deleted. This is got by asking the API as follows.
    
    // As now there are only 4 tracks, I ask the api for each one of them individually.
    // NOTE: This will be done iterating an array when custom tracks are available
    // We have from the "data" parameter an object called "urls", which contains the 4 urls, identified by its name
    let parsedData = JSON.parse(data)
    
    // Let's start by asking for the bass track file
    
    let askForBassTrackRequestForm = new FormData();
    askForBassTrackRequestForm.append("url",parsedData.urls.bass);      // this is how the url is passed
    askForBassTrackRequestForm.append("userWhoAsksId",userData.id);     // I pass this to get who is using the
    let askForBassTrackRequest = new XMLHttpRequest();                        //   url but not the current user
    askForBassTrackRequest.open("POST","/api/IsTrackUrlUsedBySomeoneElse");
    askForBassTrackRequest.send(askForBassTrackRequestForm);
    askForBassTrackRequest.onreadystatechange = function() {
        if(askForBassTrackRequest.readyState === XMLHttpRequest.DONE && askForBassTrackRequest.status === 200) {
            if(askForBassTrackRequest.responseText === "false"){
                // here I delete the file
                storageRef.child(`results/${userData.id}/${parsedData.uid}/bass.mp3`).delete()
                    .then(function() {
                        console.log("bass deleted from bucket");
                    }).catch();
            }
        }
    }
    
    // And, let's do the same for the rest of the tracks
    
    // Drums
    let askForDrumsRequestForm = new FormData();
    askForBassTrackRequestForm.append("url", parsedData.urls.drums);
    askForBassTrackRequestForm.append("userWhoAsksId", userData.id);
    let askForDrumsRequest = new XMLHttpRequest();
    askForDrumsRequest.open("POST","/api/IsTrackUrlUsedBySomeoneElse");
    askForDrumsRequest.send(askForDrumsRequestForm)
    askForDrumsRequest.onreadystatechange = function() {
        if(askForDrumsRequest.readyState === XMLHttpRequest.DONE && askForDrumsRequest.status === 200) {
            if(askForDrumsRequest.responseText === "false") {
                storageRef.child(`results/${userData.id}/${parsedData.uid}/drums.mp3`).delete()
                    .then(function() {
                        console.log("drums deleted from bucket");
                    }).catch();
            }
        }
    }

    // Vocals
    let askForVocalsRequestForm = new FormData();
    askForVocalsRequestForm.append("url", parsedData.urls.vocals);
    askForVocalsRequestForm.append("userWhoAsksId", userData.id);
    let askForVocalsRequest = new XMLHttpRequest();
    askForVocalsRequest.open("POST","/api/IsTrackUrlUsedBySomeoneElse");
    askForVocalsRequest.send(askForVocalsRequestForm)
    askForVocalsRequest.onreadystatechange = function() {
        if(askForVocalsRequest.readyState === XMLHttpRequest.DONE && askForVocalsRequest.status === 200) {
            if(askForVocalsRequest.responseText === "false") {
                storageRef.child(`results/${userData.id}/${parsedData.uid}/vocals.mp3`).delete()
                    .then(function() {
                        console.log("vocals deleted from bucket");
                    }).catch();
            }
        }
    }

    // Other
    let askForOtherRequestForm = new FormData();
    askForOtherRequestForm.append("url", parsedData.urls.drums);
    askForOtherRequestForm.append("userWhoAsksId", userData.id);
    let askForOtherRequest = new XMLHttpRequest();
    askForOtherRequest.open("POST","/api/IsTrackUrlUsedBySomeoneElse");
    askForOtherRequest.send(askForOtherRequestForm)
    askForOtherRequest.onreadystatechange = function() {
        if(askForOtherRequest.readyState === XMLHttpRequest.DONE && askForOtherRequest.status === 200) {
            if(askForOtherRequest.responseText === "false") {
                storageRef.child(`results/${userData.id}/${parsedData.uid}/other.mp3`).delete()
                    .then(function() {
                        console.log("other was deleted from bucket");
                    }).catch();
            }
        }
    }
}