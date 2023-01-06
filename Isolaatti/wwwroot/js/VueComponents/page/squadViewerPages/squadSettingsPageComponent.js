const squadSettingsComponent = {
    props: {
        squad: {
            type: Object,
            required: true
        }
    },
    data: function() {
      return {
        squadPrivacy: undefined
      }
    },
    methods: {
        setSquadPrivacy: async function(e) {
            const response = await fetch(`/api/Squads/${this.squad.id}/UpdatePrivacyTo/${this.squadPrivacy}`, {
                method: "post",
                headers: customHttpHeaders
            });
        }
    },
    mounted: function() {
      switch(this.squad.privacy) {
        case 0: this.squadPrivacy = "Private"; break;
        case 1: this.squadPrivacy = "Public"; break;
      }
    },
    template: `
      <div>
        <div class="form-group">
          <label for="squad_privacy">Privacidad del squad</label>
          <select class="custom-select" id="squad_privacy" v-model="squadPrivacy">
            <option value="Private">Solo por invitación</option>
            <option value="Public">Todos puedes solicitar entrar</option>
          </select>
          <div class="d-flex justify-content-end mt-1">
            <button class="btn btn-primary" @click="setSquadPrivacy">Guardar</button>
          </div>
        </div>
      </div>
    `
};

Vue.component("squad-settings", squadSettingsComponent);