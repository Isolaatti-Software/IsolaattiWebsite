import {Component, Inject} from '@angular/core';
import {MAT_DIALOG_DATA, MatDialogRef} from "@angular/material/dialog";

@Component({
  selector: 'app-new-post-dialog',
  templateUrl: './new-post-dialog.component.html',
  styleUrls: ['./new-post-dialog.component.css'],
})
export class NewPostDialogComponent {
  constructor(public dialogRef: MatDialogRef<NewPostDialogComponent>, @Inject(MAT_DIALOG_DATA) public data: NewPostDialogArgs) {
  }
}

interface NewPostDialogArgs {

}
