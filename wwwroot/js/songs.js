/*
* Isolaatti project
* Erik Cavazos, 2021
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
* 
* This file should be placed in the Pages/Songs.cshtml
*/

document.addEventListener("DOMContentLoaded", function () {
    var modifyInfoModal = $('#modifySongInfoModal');
    modifyInfoModal.on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget)
        var songName = button.data('name');
        var songArtist = button.data('artist');
        var songId = button.data('id');
        var modal = $(this);
        modal.find('.modal-title').text('Modify "' + button.data('name') + '" information');
        modal.find('.modal-body #song-name').val(songName);
        modal.find('.modal-body #song-artist').val(songArtist);
        modal.find('.modal-body #modal-song-id').val(songId);

    });
    modifyInfoModal.find(".modal-footer #modal-button-modify").on('click', function (event){
        var newName = modifyInfoModal.find('.modal-body #song-name').val();
        var newArtist = modifyInfoModal.find('.modal-body #song-artist').val();
        var songId = modifyInfoModal.find(".modal-body #modal-song-id").val();
        // starts call to API
        let XMLHttpRequestObj = new XMLHttpRequest();
        let formData = new FormData();
        formData.append("songId", songId);
        formData.append("songName", newName);
        formData.append("songArtist", newArtist);
        XMLHttpRequestObj.open("POST", "/api/ModifySongInfo");
        XMLHttpRequestObj.send(formData);
        XMLHttpRequestObj.onreadystatechange = function () {
            if (XMLHttpRequestObj.readyState === XMLHttpRequest.DONE){
                // updates DOM
                //document.querySelector(`#songid_${songId} .song-name a` ).innerHTML = newName;
                //document.querySelector(`#songid_${songId} .song-artist`).innerHTML = newArtist;

                //var modifiedSongModifyButton = document.querySelector(`#songid_${songId} .song-actions .modify-info-song`);
                //modifiedSongModifyButton.setAttribute("data-name", newName);
                //modifiedSongModifyButton.setAttribute("data-artist", newArtist);
                //modifyInfoModal.modal('hide');
                window.location.reload();
            }
        };
    });
    var shareSongModal = $("#shareSongModal");
    shareSongModal.on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget);
        var songId = button.data('songid');
        let songName = button.data('songname');
        let songArtist = button.data('artist');
        var modal = $(this);
        var linkField = modal.find(".modal-body #share-link-field");

        modal.find('.modal-body #share-modal-song-info').text(songName + " - " + songArtist);

        // Here request to get link is made
        let formData = new FormData();
        formData.append("songId", songId);
        formData.append("userId", userData.id);
        formData.append("passwd", userData.password);

        let request = new XMLHttpRequest();
        request.open("POST", "/api/ShareSong");
        request.send(formData);
        request.onreadystatechange = function (){
            if (request.readyState === XMLHttpRequest.DONE) {
                linkField.val(request.responseText)
            }
        }
    });
    shareSongModal.find(".modal-footer #modal-button-copy-to-clipboard").on('click', function (){
        let modal = shareSongModal;
        let copyButton = modal.find(".modal-footer #modal-button-copy-to-clipboard");
        let gottenUrl = modal.find(".modal-body #share-link-field");
        navigator.clipboard.writeText(gottenUrl.val()).then(function (){
            copyButton.text("Copied");
            console.log("Copied to clipboard");
        });
    });

    shareSongModal.on('hidden.bs.modal', function (){
        let modal = shareSongModal;
        let copyButton = modal.find(".modal-footer #modal-button-copy-to-clipboard");
        let gottenUrl = modal.find(".modal-body #share-link-field");
        copyButton.text("Copy to clipboard");
        gottenUrl.val("");
    });
});