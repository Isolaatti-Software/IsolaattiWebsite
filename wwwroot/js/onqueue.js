/*
* Isolaatti project
* Erik Cavazos, 2021
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
* 
* This file should be placed in the Pages/WebApp/OnQueue.cshtml
*/

function deleteElementFromQueue(elementId) {
    if(confirm("Do you really want to delete that song from the queue?")) {
        let formData = new FormData();
        formData.append("id", elementId);
        formData.append("userId", userData.id);
        formData.append("password", userData.password);
        
        let request = new XMLHttpRequest();
        request.open("POST","/DeleteElementFromQueue");
        request.send(formData);
        request.onreadystatechange = function() {
            if(request.readyState === XMLHttpRequest.DONE) {
                // deletes file from bucket
                ///////////        pending       ///////////
                
                // update DOM
                document.querySelector("#elementid_" + elementId).remove();
            }
        }
    }
    
}