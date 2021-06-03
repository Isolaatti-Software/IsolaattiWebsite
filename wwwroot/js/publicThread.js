(function() {
    let form = new FormData();
    form.append("id", postId);
    
    let postRequest = new Request("/api/GetPost/PublicThread",{method:"POST", body: form, headers:{'Accept': 'text/json'}});
    fetch(postRequest)
        .then(function(response) {
            console.log(response.json().then(function(value) {
                new Vue({
                    el: "#vue-container",
                    data: {
                        thread: value
                    },
                    methods: {
                        compileMarkdown: function(raw) {
                            return marked(raw)
                        }
                    }
                })
            }));
        })
        .catch(function() {
            
        });
    
})();