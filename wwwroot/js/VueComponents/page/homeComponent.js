Vue.component('home-page', {
    template: `
      <main class="mt-4">
      <div class="d-flex align-items-center flex-column"
           id="feed-and-thread-reader-container"
           ref="feedAndThreadReaderContainer">
        <new-post class="w-100"></new-post>
        <feed></feed>
      </div>
      </main>
    `
})