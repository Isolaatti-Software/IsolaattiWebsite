new Vue({
    el: "#sign-in",
    data: {
        email: "",
        password: ""
    },
    computed: {
        emailIsValid: function () {
            const emailValidationRegex = new RegExp("^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$");
            return emailValidationRegex.test(this.email);
        },
        passwordIsValid: function () {
            return this.password.length > 0;
        },
        canSignIn: function () {
            return this.emailIsValid && this.passwordIsValid;
        }
    },
    mounted: function () {
        if (username !== undefined) {
            this.email = username;
        }
    }
})