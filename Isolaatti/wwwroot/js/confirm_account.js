﻿new Vue({
    el: "#sign-up",
    data: {
        usernameField: {
            value: username, //server rendered
            hadInput: false,
            isAvailable: undefined
        },
        nameField: {
            value: name, // server rendered
            hadInput: false
        },
        passwordField: {
            value: "",
            hadInput: false
        },
        passwordConfirmationField: {
            value: "",
            hadInput: false
        }
    },
    methods: {
        queryNameAvailability: async function() {
            const result = await fetch(`/api/usernames/check?username=${this.usernameField.value}`);
            if(!result.ok) {
                this.usernameField.isAvailable = undefined;
            }
            const availability = await result.json();
            this.usernameField.isAvailable = availability.available;
        }
    },
    computed: {
        usernameIsInvalid: function() {
            return this.usernameField.value.length < 3 || this.usernameField.value.length > 20;
        },
        nameIsInvalid: function () {
            return this.nameField.value.length < 1 || this.nameField.value.length > 20;
        },
        passwordIsInvalid: function () {
            return this.passwordField.value.length < 8;
        },
        passwordUnMatches: function () {
            return this.passwordField.value !== this.passwordConfirmationField.value && this.passwordConfirmationField.value.length > 0 && this.passwordField.value.length > 0;
        },
        canSignUp: function () {
            return !this.nameIsInvalid && !this.passwordIsInvalid && !this.passwordUnMatches && this.usernameField.isAvailable;
        }
    },
    watch: {
        "usernameField.value": async function() {
            this.usernameField.hadInput = true
            this.usernameField.isAvailable = undefined;
            if(!this.usernameIsInvalid) {
                await this.queryNameAvailability();
            }
        },
        "nameField.value": function() {
            this.nameField.hadInput = true;
        },
        "passwordField.value": function() {
            this.passwordField.hadInput = true;
        },
        "passwordConfirmationField.value": function() {
            this.passwordConfirmationField.hadInput = true
        }
    }
})