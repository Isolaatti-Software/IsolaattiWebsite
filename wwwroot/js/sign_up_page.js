new Vue({
    el: "#sign-up",
    data: {
        name: "",
        email: "",
        password: "",
        passwordConfirmation: ""
    },
    computed: {
        nameIsValid: function () {
            return this.name.length > 0 && this.name.length <= 20;
        },
        emailIsValid: function () {
            const emailValidationRegex = new RegExp("^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$");
            return emailValidationRegex.test(this.email);
        },
        passwordIsValid: function () {
            return this.password.length > 7;
        },
        passwordUnMatches: function () {
            return this.password !== this.passwordConfirmation && this.passwordConfirmation.length > 0 && this.password.length > 0;
        },
        canSignUp: function () {
            return this.emailIsValid && this.passwordIsValid;
        }
    },
    mounted: function () {
        if (username !== undefined) {
            this.email = username;
        }
    }
})