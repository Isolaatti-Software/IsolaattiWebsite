const squadImages = {
    props: {
        squadId: {
            required: true,
            type: String
        }
    },
    template: `
      <div>
        <profile-images :squad-id="squadId"></profile-images>
      </div>
    `
}