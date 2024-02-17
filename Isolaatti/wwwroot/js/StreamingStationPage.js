
Vue.component("streaming-station", {
    data: function() {
        return {
            url: "",
            name: "",
            dashUrl: "",
            helpUrl: "https://isolaattisoftware.com.mx/ayuda/2024/02/17/guia-para-transmitir-a-isolaatti-con-obs-studio.html"
        }
    },
    methods: {
        getPlayStreamUrl: async function() {
            const endpoint = `/api/streaming/station/${stationId}/get_stream_play_url`;

            const response = await fetch(endpoint);

            if(response.ok) {
                const json = await response.json();
                this.dashUrl = json.dash;
            }
        },
        playStream: function(){
            const player = dashjs.MediaPlayer().create();
            player.initialize(document.querySelector("#player"), this.dashUrl, true);
        },
        regenerateStreamKey: async function() {
            const endpoint = `/api/streaming/station/${stationId}/get_stream_config`;

            const response = await fetch(endpoint);

            if(response.ok) {
                const json = await response.json()
                this.url = json.url;
                this.name = json.name;
            }
        }
    },
    mounted: async function(){
        await this.regenerateStreamKey();
        await this.getPlayStreamUrl();
    },
    template: `
    <div class="row">
        <div class="col-md-6">
            <h5>Comienza a transmitir</h5>
            <div class="card">
                <div class="card-body">
                    
                    <h5>Clave transmisión</h5>
                    <p>Esta clave se genera cada vez que esta página carga. No es necesario venir aquí para comenzar una transmisión</p>
                    <button class="btn btn-primary" @click="regenerateStreamKey">Regenerar clave de transmisión</button>
                    <hr/>
                    <div class="alert alert-info">
                        Utiliza estos datos para configurar tu software de transmisión. Se recomienda utilizar OBS Studio. <strong><i class="fa-solid fa-arrow-up-right-from-square"></i><a :href="helpUrl">Guía para configurar OBS</a></strong>
                    </div>
                    <div class="form-group">
                        <label for="url_input">Url</label>
                        <input id="url_input" disabled="disabled" type="text" class="form-control" :value="url"/>
                    </div>
                    <div class="form-group">
                        <label for="name_input">Nombre</label>
                        <input id="name_input" disabled="disabled" type="text" class="form-control" :value="name"/>
                    </div>

                </div>
            </div>
        </div>
        <div class="col-md-6">
            <h5>Vista previa</h5>
            <button class="btn btn-primary" @click="playStream">Refrescar</button>
            <video id="player" width="100%" controls autoplay="autoplay" muted></video>
        </div>
    </div>
    `
})