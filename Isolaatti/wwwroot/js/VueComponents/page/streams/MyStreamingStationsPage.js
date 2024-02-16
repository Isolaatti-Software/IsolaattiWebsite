Vue.component("my-streaming-stations", {
    data: function() {
        return {
            userData: userData,
            stations: []
        }
    },
    methods: {
        fetchStations: async function() {
            const response = await fetch(`/api/streaming/user/${this.userData.id}`);
            
            if(response.ok) {
                const json = await response.json();
                this.stations = json.result;
            }
        },
        playStreamUrl: function(stationId) {
            return `/VideoStream/StreamPlayer?stationId=${stationId}`
        },
        configStreamUrl: function(stationId) {
            return `/MediaStreaming/StreamingStation?stationId=${stationId}`;
        }
    },
    mounted: async function() {
        await this.fetchStations();
    },
    template: `
<div class="row">
    <div class="col-md-3"></div>
    <div class="col-md-6">
    <div class="list-group list-group-flush">
    <div class="list-group-item list-group-item-action" v-for="station in stations">
    <p>{{station.name}}</p>
    <a :href="playStreamUrl(station.id)" target="_blank">Reproducir</a>
    <a :href="configStreamUrl(station.id)" target="_blank">Configurar</a>
    </div>
</div>
</div>
    <div class="col-md-3"></div>
</div>`
})