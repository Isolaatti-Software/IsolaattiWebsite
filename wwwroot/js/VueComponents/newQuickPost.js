Vue.component('new-post', {
    template: `
      <div class="card max-640 mt-2">
      <div class="card-body">
        <h5 class="card-title"><i class="fa-solid fa-plus"></i> Nuevo</h5>
        <ul class="nav nav-tabs mt-2 max-640" id="new-post-tab">
          <li class="nav-item">
            <a class="nav-link active" id="new-post-discussion-tab" href="#new-post-discussion-pane" data-toggle="tab">
              Discusión rápida
            </a>
          </li>
          <li class="nav-item">
            <a class="nav-link" id="new-post-audio" href="#new-post-audio-pane" data-toggle="tab">
              Audio
            </a>
          </li>
        </ul>
        <div class="tab-content max-640 mt-1" id="new-post-tab-content">
          <div class="tab-pane show active" id="new-post-discussion-pane">
            <new-discussion></new-discussion>
          </div>
          <div class="tab-pane" id="new-post-audio-pane">
            <audio-recorder></audio-recorder>
          </div>
        </div>
      </div>
      </div>
    `
})