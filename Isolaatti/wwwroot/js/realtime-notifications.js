// socket is globally available
const socket = io("http://localhost:3000",{
    withCredentials: true,
    auth: {
        sessionToken
    }
});

socket.on("connect_error", (err) => {
    console.error(`Real time service error: ${err.message}`);
    console.error(err.data);
});

socket.on("notification", data => {
    console.log(data);
});