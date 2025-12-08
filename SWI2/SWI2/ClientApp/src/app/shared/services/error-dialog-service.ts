import { HttpErrorResponse } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { MatDialog } from "@angular/material/dialog";
import { AlertComponent, AlertData } from "../../alert/alert.component";
import { SWIError } from "../../errors/error";
import { ErrorDialogComponent } from "../../errors/error-dialog/error-dialog.component";

@Injectable()
export class ErrorDialogService {
  constructor(public dialog: MatDialog) {

  }

  showDialog(x: HttpErrorResponse) {
    this.dialog.open(ErrorDialogComponent, {
      height: 'auto',
      width: 'auto',
      maxHeight: '50vh',
      data: new SWIError(x.status, x.message,x.error)
    })
  }
  showAlert(x: AlertData) {
    this.dialog.open(AlertComponent, {
      height: 'auto',
      width: 'auto',
      maxHeight: '50vh',
      data: x
    })
  }
}
