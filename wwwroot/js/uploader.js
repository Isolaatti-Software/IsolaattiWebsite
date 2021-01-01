/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*
* uploader.js
* This file is intended to be inserted at the bottom of the WebApp/Index.cshtml (where the uploading form is)
* What this does:
*   Adds event listener to input file: to perform DOM changes when a file has been selected
*   Adds event listener to "GO" button: to perform some validation and then upload the file.
*       Once the file is uploaded, a URL to that file is gotten and sent to the API to create 
*       a new element on the queue.
*/

var storageRef = storage.ref();

let labelNameOfFile = document.getElementById("label-input-file");
let fileSource = document.getElementById("audioFileInput");
let buttonUploadFile = document.getElementById("btnUploadAudioFile");
let fieldSongName = document.getElementById("field_songs_name");
let fieldArtistName = document.getElementById("field_songs_artist");
let progressBarContainer = document.getElementById("progress-bar-container");
let progressBar = document.getElementById("file_upload_progress_bar");

fileSource.addEventListener("change", function(){
    fieldSongName.disabled = false;
    fieldArtistName.disabled = false;
    fieldSongName.value = fileSource.files[0].name;
    labelNameOfFile.innerHTML = fileSource.files[0].name;
    fieldArtistName.value = "Unknown (change me)"
});

buttonUploadFile.addEventListener("click",function (){
    if(fieldSongName.value === "" || fieldArtistName.value === "")
        alert("Field cannot be empty");
    else{
        buttonUploadFile.disabled = true;
        fileSource.disabled = true;
        fieldArtistName.disabled = true;
        fieldSongName.disabled = true;
        progressBarContainer.style.visibility = "visible";
        let uploadTask = storageRef
            .child(`source_files/${fileSource.files[0].name}`)
            .put(fileSource.files[0],{
                contentType:"audio/mpeg"
            });
        
        let songName = fieldSongName.value;
        let songArtist = fieldArtistName.value;

        uploadTask.on("state_changed",function(snapshot){
            //updates progress bar
            progressBar.style.width = `${(snapshot.bytesTransferred / snapshot.totalBytes) * 100}%`;
        }, function(error){
            alert(error)
        }, function(){
            buttonUploadFile.disabled = false;
            fileSource.disabled = false;
            fieldArtistName.disabled = false;
            fieldSongName.disabled = false;
            uploadTask.snapshot.ref.getDownloadURL().then(function(downloadURL) {
                console.log('File available at', downloadURL);
                registerFile(downloadURL,songName,songArtist);
            });
        });
    }
});

function registerFile(fileUrl, songName, songArtist){
    let formData = new FormData();
    formData.append("userId",userData.id);
    formData.append("songName", songName);
    formData.append("url", fileUrl);
    
    if(songArtist !== "")
        formData.append("songArtist",songArtist);
    
    let createRecordRequest = new XMLHttpRequest();
    createRecordRequest.open("POST", "/AddSongToQueue");
    createRecordRequest.send(formData);
    createRecordRequest.onreadystatechange = function (){
        if(createRecordRequest.readyState === XMLHttpRequest.DONE){
            alert("Song added to queue");
            window.location = "/WebApp/OnQueue";
        }
    }
        
}