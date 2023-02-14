const interactions = {
    template: `
        <div>
            <h5 class="mt-3">Interacciones</h5>
            <div class="isolaatti-card">
                <div class="list-group list-group-flush">
                    <router-link to="/interacciones/likes" class="list-group-item list-group-item-action">Consultar discusiones a las que este perfil dio like</router-link>
                    <router-link to="/interacciones/discusiones" class="list-group-item list-group-item-action">Consultar discusiones en las que este usuario participó</router-link>
                    <router-link to="/interacciones/en_squads" class="list-group-item list-group-item-action">Ver squads donde este perfil está</router-link>
                </div>
            </div>
        </div>
    `
}