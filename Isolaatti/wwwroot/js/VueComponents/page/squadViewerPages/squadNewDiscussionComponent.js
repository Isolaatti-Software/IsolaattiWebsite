const squadNewDiscussionComponent = {
    props:{
        squadId: {
            required: true,
            type: String
        }
    },
    data: function() {
        return {}
    },
    template: `
    <section>
      <div class="isolaatti-card">
        <new-discussion :squad-id="squadId"></new-discussion>
      </div>
      <posts-list :squad-id="squadId"></posts-list>
    </section>
    `
}

Vue.component('squad-new-discussion', squadNewDiscussionComponent);