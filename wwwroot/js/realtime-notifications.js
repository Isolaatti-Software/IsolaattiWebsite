const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notifications_hub").build();

async function start() {
    try {
        await connection.start();
        console.log("Conexion exitosa");
        
        connection.invoke("EstablishConnection").then(function(data) {
           console.log(data);
        }).catch(function (error) {
            console.error(error);
        });
        
        connection.on("SessionSaved", function(serverResponse) {
            console.log(serverResponse);
        });
        
        connection.on("hola", function(data) {
            alert(data);
        });
    } catch(err) {
        console.log(err);
        setTimeout(start, 5000);
    }
}

connection.onclose(start);

start();