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

@Component({
    selector: 'app-payment-method-create',
    templateUrl: './payment-method-create.component.html',
    styleUrls: ['./payment-method-create.component.css']
})
/** payment-method-create component*/
export class PaymentMethodCreateComponent {
  paymentMethod: PaymentMethod;
  createFormGroup: FormGroup;
    paymentCurrencyOptions: any;
    currencyCodes: any;
    /** payment-method-create ctor */
  constructor(private _api: ApiService,
    private formBuilder: FormBuilder,
    private dialogRef: MatDialogRef<PaymentMethod>,
    private dialog: MatDialog,
    private errorD: ErrorDialogService,
    @Inject(MAT_DIALOG_DATA) public data: string) {
    this.createFormGroup = formBuilder.group({
      name: ['', Validators.required],
      accountNumber: ['', [Validators.required, Validators.minLength(26), Validators.maxLength(26)]],
      currency: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(4)]]
    });
    this.currencyCodes = require('currency-codes');
    this.paymentCurrencyOptions = this.createFormGroup.get('currency').valueChanges.pipe(
      startWith(''),
      map(value => {
        return this.filterPaymentCurrency(value);
      })
    );
  }


  onSubmit() {
    
    if (this.createFormGroup.valid) {
      const rawData = this.createFormGroup.value;
      const data = new PaymentMethod(rawData);
      this._api.post("/api/companypanel/" + (this.data !== "" ? this.data + "/" : "") + "paymentmethod", data).toPromise().then(() => {
        this.dialogRef.close({ op: "create", data: data });
      }).catch((x: HttpErrorResponse) => {
        this.errorD.showDialog(x);
      });
    }
  }
  filterPaymentCurrency(value: string): any {
    return this.currencyCodes.codes().filter(option => option.toLowerCase().indexOf(value.toLowerCase()) === 0);
  }

  displayPaymentCurrencyAutocomplet(displayValue: string): string {
    return displayValue + ' | ' + getCurrencySymbol(displayValue, 'narrow');
  }
}
