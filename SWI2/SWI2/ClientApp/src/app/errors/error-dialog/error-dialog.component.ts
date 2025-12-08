import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { SWIError } from '../error';

@Component({
    selector: 'app-error-dialog',
    templateUrl: './error-dialog.component.html',
    styleUrls: ['./error-dialog.component.css']
})
/** error-dialog component*/
export class ErrorDialogComponent {


  constructor(
    private dialogRef: MatDialogRef<null>,
    @Inject(MAT_DIALOG_DATA) public data: SWIError) {

  }

  close() {
    this.dialogRef.close()
  }
}
