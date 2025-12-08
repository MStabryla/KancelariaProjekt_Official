import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ApiService } from '../../shared/services/api.service';
import { PaymentMethod } from '../payment-method';
import { SWIError } from '../../errors/error';
import { HttpErrorResponse } from '@angular/common/http';
import { ErrorDialogComponent } from '../../errors/error-dialog/error-dialog.component';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';
import { map, startWith } from 'rxjs/operators';
import { getCurrencySymbol } from '@angular/common';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-payment-method-edit',
  templateUrl: './payment-method-edit.component.html',
  styleUrls: ['./payment-method-edit.component.css']
})
/** payment-method-edit component*/
export class PaymentMethodEditComponent {

  editFormGroup: FormGroup;
  currencyCodes: any;
  paymentCurrencyOptions: Observable<string[]>;

  constructor(private _api: ApiService,
    private dialogRef: MatDialogRef<PaymentMethod>,
    private formBuilder: FormBuilder,
    private dialog: MatDialog,
    private errorD: ErrorDialogService,
    @Inject(MAT_DIALOG_DATA) public data: PaymentMethod) {
    this.editFormGroup = formBuilder.group({
      name: [data.name, Validators.required],
      accountNumber: [data.accountNumber, [Validators.required, Validators.minLength(26), Validators.maxLength(26)]],
      currency: [data.currency, [Validators.required, Validators.minLength(2), Validators.maxLength(4)]]
    })
    this.currencyCodes = require('currency-codes');
    this.paymentCurrencyOptions = this.editFormGroup.get('currency').valueChanges.pipe(
      startWith(''),
      map(value => {
        return this.filterPaymentCurrency(value);
      })
    );
  }

  onSubmit() {
    const rawData = this.editFormGroup.value;
    const data = new PaymentMethod(rawData);
    const id = this.data.id;
    this.data = data;
    this._api.put("/api/companypanel/paymentmethod/" + id, data).toPromise().then(() => {
      this.dialogRef.close({ op:"edit",data: this.data });
    }).catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    });
  }

  filterPaymentCurrency(value: string): any {
    return this.currencyCodes.codes().filter(option => option.toLowerCase().indexOf(value.toLowerCase()) === 0);
  }

  displayPaymentCurrencyAutocomplet(displayValue: string): string {
    return displayValue + ' | ' + getCurrencySymbol(displayValue, 'narrow');
  }
}
