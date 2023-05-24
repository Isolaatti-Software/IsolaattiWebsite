Vue.component('notifications-page',{
    template: `
    <div class="d-flex flex-column">
        <h5 class="mt-3 text-center"><i class="fa-solid fa-bell"></i> Notificaciones</h5>
        <div class="d-flex justify-content-end">
            <button class="btn btn-light">
                <i aria-hidden="true" class="fas fa-ellipsis-h"></i>
            </button>
        </div>
        <div class="d-flex">
            <notification/>
        </div>
    </div>
    `
})