import {Component, inject} from '@angular/core';
import {MatDialog} from "@angular/material/dialog";
import {NewPostDialogComponent} from "../app/posting/new-post-dialog/new-post-dialog.component";


@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
})
export class HomeComponent {
  newPostDialog = inject(MatDialog)

  openNewPostDialog() {
    this.newPostDialog.open(NewPostDialogComponent, {
      hasBackdrop: true,
      width: "640px",
      height: "640px"
    })
  }
}
