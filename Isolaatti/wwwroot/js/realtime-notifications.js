// socket is globally available
const cookieName = 'isolaatti_user_session_token';
const authorization = document.cookie
    .split('; ')
    .find(row => row.startsWith(cookieName)).split("=")[1];

const socket = io(realtimeServer ,{
    withCredentials: true,
    auth: {
        authorization,
        clientId
    }
});

socket.on("connect_error", (err) => {
    console.error(`Real time service error: ${err.message}`);
    console.error(err.data);
});

socket.on("notification", data => {
    console.log(data);
});