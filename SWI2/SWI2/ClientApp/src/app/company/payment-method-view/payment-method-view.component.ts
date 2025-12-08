import { Component, Inject } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ApiService } from '../../shared/services/api.service';
import { PaymentMethod } from '../payment-method';
import { PaymentMethodEditComponent } from '../payment-method-edit/payment-method-edit.component';
import { SWIError } from '../../errors/error';
import { HttpErrorResponse } from '@angular/common/http';
import { ErrorDialogComponent } from '../../errors/error-dialog/error-dialog.component';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';

@Component({
    selector: 'app-payment-method-view',
    templateUrl: './payment-method-view.component.html',
    styleUrls: ['./payment-method-view.component.css']
})
/** payment-method-view component*/
export class PaymentMethodViewComponent {
/** payment-method-view ctor */

  get url() {
    return "/api/companypanel/paymentmethod/" + this.data.id;
  }

  constructor(
    public _api: ApiService,
    @Inject(MAT_DIALOG_DATA) public data: PaymentMethod,
    private dialogRef: MatDialogRef<PaymentMethod>,
    private errorD: ErrorDialogService,
    public dialog: MatDialog) {

  }

  edit() {
    this.dialog.open(PaymentMethodEditComponent, {
      height: 'auto',
      width: 'auto',
      maxHeight: '50vh',
      data: this.data
    }).afterClosed().subscribe(x => {
      if (x && x.op === "edit")
        this.data = x.data;
    })
  }

  delete() {
    this._api.delete("/api/companypanel/paymentmethod/" + this.data.id).toPromise().then(() => {
      this.dialogRef.close({ op: "remove", data: this.data });
    }).catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    });
  }
}
