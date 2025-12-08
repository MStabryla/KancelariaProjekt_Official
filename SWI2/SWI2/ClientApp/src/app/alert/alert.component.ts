import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
    selector: 'app-alert',
    templateUrl: './alert.component.html',
    styleUrls: ['./alert.component.css']
})
/** alert component*/
export class AlertComponent {
    /** alert ctor */
  constructor(private dialogRef: MatDialogRef<null>,
    @Inject(MAT_DIALOG_DATA) public data: AlertData) {

  }
  close() {
    this.dialogRef.close();
  }
}
export class AlertData {
  constructor(public title: string, public message: string, public translationParams = {}) {

  }
}
