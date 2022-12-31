const squadSettingsComponent = {
    props: {
        squadId: {
            type: String,
            required: true
        }
    },
    methods: {
        setSquadPrivacy: async function(e) {
            const response = await fetch(``, {
                method: "post",
                headers: customHttpHeaders,
                body: JSON.stringify({
                    squadId: this.squadId,
                    privacy: e.target.value
                })
            });
        }
    },
    template: `
      <div>
        <div class="form-group">
          <label for="squad_privacy">Privacidad del squad</label>
          <select class="custom-select" id="squad_privacy" @change="setSquadPrivacy">
            <option value="private">Solo por invitación</option>
            <option value="public">Todos puedes solicitar entrar</option>
          </select>
        </div>
      </div>
    `
};

Vue.component("squad-settings", squadSettingsComponent);