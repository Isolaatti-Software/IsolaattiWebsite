const squadLists = {
    data: function() {
        return {
            expanded: undefined //yourSquads or squadsBelong
        }
    },
    methods: {
        setExpanded: function(value) {
            if(this.expanded === value) {
                this.expanded = undefined;
            } else {
                this.expanded = value;
            }
        }
    },
    template: `
        <section>
        <router-link to="/solicitudes_union" class="d-flex align-items-center justify-content-between list-group-item-action btn btn-light">
          <h5>Ver solicitudes</h5>
          <i class="fa-solid fa-chevron-right"></i>
        </router-link>
        
        <router-link to="invitaciones" class="d-flex align-items-center justify-content-between list-group-item-action btn btn-light mt-1">
          <h5>Ver invitaciones</h5>
          <i class="fa-solid fa-chevron-right"></i>
        </router-link>
        
        <div>
          <div class="d-flex align-items-center justify-content-between list-group-item-action btn btn-light mt-1" @click="setExpanded('yourSquads')">
            <h5>Tus Squads</h5>
            <i class="fa-solid fa-chevron-up" v-if="expanded==='yourSquads'"></i>
            <i class="fa-solid fa-chevron-down" v-else></i>
          </div>
          <your-squads v-show="expanded==='yourSquads'" />
        </div>

        <div class="d-flex align-items-center justify-content-between list-group-item-action btn btn-light mt-1" @click="setExpanded('squadsBelong')">
          <h5>Los demás squads</h5>
          <i class="fa-solid fa-chevron-up" v-if="expanded==='squadsBelong'"></i>
          <i class="fa-solid fa-chevron-down" v-else></i>
        </div>
        <squads-belong v-show="expanded==='squadsBelong'" />
        </section>
    `
}

Vue.component("squad-lists", squadLists);