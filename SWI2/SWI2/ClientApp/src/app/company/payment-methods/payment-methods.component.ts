import { SelectionModel } from '@angular/cdk/collections';
import { Component } from '@angular/core';
import { Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { FormModel } from '../../models/Form.model';
import { PaymentMethodViewComponent } from '../payment-method-view/payment-method-view.component';

@Component({
  selector: 'swi-payment-methods',
  templateUrl: './payment-methods.component.html',
  styleUrls: ['./payment-methods.component.css']
})
/** payment-methods component*/
export class PaymentMethodsComponent {
  sortActive = "created";
  requestUrl = "api/companypanel/{id}/paymentmethod";
  detailsComponent = PaymentMethodViewComponent;
  headerRow = [
    { description: "COMPANY.PAYMENT_METHODS.TABLE_NAME_DESCRIPTION", name: "name", type: "text", ifRange: false } as FormModel,
    { description: "COMPANY.PAYMENT_METHODS.TABLE_ACCOUNT_NUMBER_DESCRIPTION", name: "accountNumber", type: "text", ifRange: false } as FormModel,
    { description: "COMPANY.PAYMENT_METHODS.TABLE_CURRENCY_DESCRIPTION", name: "currency", type: "text", ifRange: false } as FormModel,
    { description: "COMPANY.PAYMENT_METHODS.TABLE_CREATION_DATE_DESCRIPTION", name: "created", type: "date", ifRange: true } as FormModel
  ];
  validators = [Validators.nullValidator, Validators.nullValidator, Validators.nullValidator, Validators.nullValidator];
  selection = new SelectionModel<any>(true, []);
  /** payment-methods ctor */
  constructor(public dialog: MatDialog) {

  }
  dbClickHandler(data: any) {
    //this._router.navigateByUrl(this.detailsComponent + "/" + data["id"]);

    this.dialog.open(this.detailsComponent, {
      height: 'auto',
      width: 'auto',
      maxHeight: '75vh',
      data: data
    }).afterClosed().subscribe(x => {
    });
  }
}
