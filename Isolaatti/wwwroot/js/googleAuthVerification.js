var firebaseConfig = {
    apiKey: "AIzaSyDZHVXLR_1imwVb2mCArnYpdbNx6XUPZBQ",
    authDomain: "isolaatti-b6641.firebaseapp.com",
    databaseURL: "https://isolaatti-b6641.firebaseio.com",
    projectId: "isolaatti-b6641",
    storageBucket: "isolaatti-b6641.appspot.com",
    messagingSenderId: "556033448926",
    appId: "1:556033448926:web:35759afc37d6a1585b1b4f",
    measurementId: "G-JQV4WX75RK"
};
// Initialize Firebase
firebase.initializeApp(firebaseConfig);

let infoContainerText = document.getElementById("info");

firebase.auth().onAuthStateChanged(function(user){
    user.getIdToken().then(function(token){
        const param = encodeURIComponent(token)
        const thenParamAppend = typeof thenParam === 'undefined' ? "" : `&then=${encodeURIComponent(thenParam)}`;
        const url = `/api/ExternalSignIn/Web?accessToken=${param}` + thenParamAppend;
        window.location = url;
    });
});