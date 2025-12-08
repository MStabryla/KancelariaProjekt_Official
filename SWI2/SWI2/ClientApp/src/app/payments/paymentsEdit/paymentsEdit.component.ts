import { Component, Inject, ViewChild } from "@angular/core";
import { AbstractControl, FormArray, FormBuilder, FormControl, FormGroup, ValidatorFn, Validators } from "@angular/forms";
import { MatDialog, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { MatPaginator } from "@angular/material/paginator";
import { MatTableDataSource } from "@angular/material/table";
import { BehaviorSubject, merge, observable, Observable } from "rxjs";
import { count, debounceTime, distinctUntilChanged, map, startWith } from "rxjs/operators";
import { Contractor } from "../../models/contractors/Contractor.model";
import { ContractorBankAccount } from "../../models/contractors/ContractorBankAccount.model";
import { FormModel } from "../../models/Form.model";
import { Invoice } from "../../models/invoices/Invoice.model";
import { checkType } from "../../shared/custom-validators/checkTypeValidator.service";
import { ContractorBankAccountPaymentFormModel, ContractorPaymentFormModel, InvoicePaymentFormModel, PaymentFormModel, PaymentModel, PaymentsForInvoicesFormModel, PaymentTableModel } from "../../models/payments/Payment.model";
import { TableParamsModel } from "../../models/tableParams.model";
import { AskIfDialog } from "../../shared/components/askIfDialog/askIfDialog.component";
import { ApiService } from "../../shared/services/api.service";
import { AuthenticationService } from "../../shared/services/authentication.service";
import { isArray } from "util";
import { getCurrencySymbol } from "@angular/common";

@Component({
  selector: 'swi-paymentsEdit',
  templateUrl: './paymentsEdit.component.html',
  styleUrls: ['./paymentsEdit.component.css']
})

export class paymentsEdit {
  dataSource = new MatTableDataSource<PaymentFormModel>();
  paymentForm: FormGroup;
  actualCompanyId: number;
  contractors: ContractorPaymentFormModel[] = null;
  contractorsOptions: Observable<ContractorPaymentFormModel[]>;
  contractorBankAccounts: ContractorBankAccountPaymentFormModel[] = null;
  contractorBankAccountsOptions = new BehaviorSubject<ContractorBankAccountPaymentFormModel[]>([]);
  invoices: InvoicePaymentFormModel[] = null;
  invoicesOptionsList: BehaviorSubject<InvoicePaymentFormModel[]>[] = [];
  @ViewChild(MatPaginator) paginator: MatPaginator;
  InvoiceDictionaryIndex: number;
  enabledInvoicesLength = 1;
  page = 0;
  invoicesChanges = [];
  addedInvoices = {};
  waitForUpdateOthersinvoicesValues = false;
  currencyCodes = require('currency-codes');
  paymentCurrencyOptions: Observable<string[]>;

  constructor(public dialog: MatDialog,
    private formBuilder: FormBuilder,
    private _api: ApiService,
    public _authService: AuthenticationService,
    @Inject(MAT_DIALOG_DATA) public data: { data: PaymentTableModel[], action: string }) {
    this.dataSource.data = this.data.data.map(ptm => {
      return {
        id: ptm.id,
        contractor: { id: ptm.contractorId, name: ptm.contractorName, nip: ptm.contractorNip },
        contractorBankAccount: { id: ptm.contractorBankAccountId, bankName: ptm.contractorBankAccountName, accountNumber: ptm.contractorBankAccountNumber },
        paymentsForInvoices: ptm.paymentsForInvoices,
        currency: ptm.currency,
        topic: ptm.topic,
        paymentValue: ptm.paymentValue,
        paymentDate: ptm.paymentDate
      } as PaymentFormModel
    });
    this.invoicesChanges = this.dataSource.data.map(i => { return i.paymentsForInvoices.map(ie => { return {} }) });

    this._authService.currentSelectedCompany.subscribe(c => { this.actualCompanyId = c.id; });

    this.paymentForm = this.formBuilder.group({
      id: [this.dataSource.data[0].id, Validators.required],
      contractor: [this.dataSource.data[0].contractor, Validators.required],
      contractorBankAccount: [this.dataSource.data[0].contractorBankAccount, Validators.required],
      paymentsForInvoices: this.formBuilder.array([]),
      topic: [this.dataSource.data[0].topic, Validators.required],
      paymentValue: [this.dataSource.data[0].paymentValue, Validators.required],
      currency: [this.dataSource.data[0].currency, Validators.required],
      paymentDate: [this.dataSource.data[0].paymentDate, Validators.required]
    });
    let counter = 0;
    this.dataSource.data[0].paymentsForInvoices.forEach(i => { this.addInvoice(counter, i); counter++ });
    this.enabledInvoicesLength = counter;

    this.paymentForm.controls.paymentValue.valueChanges.pipe(
      startWith()
    ).subscribe(i => {
      (<FormArray>this.paymentForm.controls.paymentsForInvoices).controls.forEach(c => {
        c['controls'].paymentValueForInvoice.updateValueAndValidity();
      });
    });

    this._api.get<Contractor[]>(`api/contractor/` + this.actualCompanyId, { query: btoa(JSON.stringify({ pageSize: -1, sort: 'created' + " " + 'asc' } as TableParamsModel)) }).pipe(map(cm => cm.map(c => new ContractorPaymentFormModel(c)))).subscribe(result => {
      this.contractors = result;

      this.contractorsOptions = this.paymentForm.get('contractor').valueChanges.pipe(
        startWith(this.dataSource.data[0].contractor),
        map(value => {
          return this.filterContractors(value);
        })
      );

      const observableOfContractorBankAccounts = this.paymentForm.get('contractorBankAccount').valueChanges.pipe(
        startWith(this.dataSource.data[0].contractorBankAccount),
        map(value => {
          return this.filterContractorBankAccounts(value);
        })
      );
      observableOfContractorBankAccounts.subscribe(this.contractorBankAccountsOptions);
    });
    this.paymentCurrencyOptions = this.paymentForm.get('currency').valueChanges.pipe(
      startWith(''),
      map(value => {
        return this.filterPaymentCurrency(value);
      })
    );
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.paginator.page
      .pipe(
        debounceTime(50),
        distinctUntilChanged()
      ).subscribe(
        index => {
          this.update(index.pageIndex);
        });
  }

  update(pageIndex: number) {
    const currentPayment = this.dataSource.data[pageIndex];
    this.getContractorbankAccounts(currentPayment.contractor.id).then(cba => { this.contractorBankAccounts = cba });
    this.getInvoices(currentPayment.contractor.id).then(i => { this.invoices = i });
    this.updateForm(this.dataSource.data[pageIndex], this.paymentForm);
    this.getInvoiceChanges();
    this.updateFormInvoices(pageIndex);
    this.page = pageIndex;
  }

  updateForm(currentPayment: object, form: any) {
    Object.keys(form.controls).forEach(key => {
      const currentControl = form.controls[key];
      if (currentControl.controls && !isArray(currentControl.controls)) {
        this.updateForm(currentPayment[key], currentControl);
      }
      else {
        if (key == 'id') {
          currentControl.patchValue(currentPayment[key]);
        } else {
          if (!currentControl.dirty) {
            currentControl.patchValue(currentPayment[key]);
          }
        }
      }
    });
  }

  updateFormInvoices(pageIndex: number) {
    for (let i = 0; i < this.invoicesChanges[this.page].length; i++) {
      this.removeInvoice(0);
    }

    Object.entries(this.dataSource.data[pageIndex].paymentsForInvoices).forEach(([_, v], index) => {
      let changedInvoiceEntrie = v;
      Object.entries(this.invoicesChanges[pageIndex][index]).forEach(([k, v]) => {
        changedInvoiceEntrie[k] = v;
      });
      this.addInvoice(index, v, pageIndex);
    });
  }

  getInvoiceChanges() {
    const entriesChanges = this.invoicesChanges[this.page];
    const entriesForm = this.paymentForm.controls.paymentsForInvoices['controls'];

    Object.keys(entriesForm).forEach(arraykey => {
      if (arraykey < entriesChanges.length) {
        const currentEntrie = entriesForm[arraykey];
        if (currentEntrie.dirty) {
          Object.keys(currentEntrie.controls).forEach(key => {
            const currentEntrieControl = currentEntrie.controls[key];
            if (currentEntrieControl.dirty) {
              entriesChanges[arraykey][key] = currentEntrieControl.value;
            }
          });
          if (entriesChanges[arraykey] && Object.keys(entriesChanges[arraykey]).length != 0 && !entriesChanges[arraykey].hasOwnProperty("id")) {
            entriesChanges[arraykey]["id"] = currentEntrie.controls.id.value;
          }
        }
      } else {

        //this.addedInvoices[] =
      }
    });
  }

  addPayment() {
    const newPayment = this.paymentForm.value;
    this._api.post<PaymentModel>(`api/payment/` + this.actualCompanyId, newPayment).pipe().subscribe(
      res => {
        this.dialog.open(AskIfDialog, {
          disableClose: true,
          height: 'auto',
          width: 'auto',
          data: { data: "PEYMENTS.PAYMENT_EDIT.PAYMENT_ADDED_SUCCES", close: "CLOSE", ok: null }
        });
      },
      error => {
        this.dialog.open(AskIfDialog, {
          disableClose: true,
          height: 'auto',
          width: 'auto',
          data: { data: "PEYMENTS.PAYMENT_EDIT.PAYMENT_ADDED_ERROR", close: "CLOSE", ok: null }
        });
      });

  }
  editPayment() {
    this.getInvoiceChanges();
    let errors = [];
    let counter = 0;
    const currentIndex = this.page;
    this.dataSource.data.forEach((e, index) => {
      this.updateFormInvoices(index);
      const x = this.getDirtyValues(this.paymentForm);
      const editedPayment = new PaymentModel(x);
      if (editedPayment.paymentsForInvoices) {
        editedPayment.paymentsForInvoices.forEach((pfi, index) => {
          if (Object.keys(pfi).length != 0) {
            if (e.paymentsForInvoices.length > index) {
              pfi.id = e.paymentsForInvoices[index].id
            } else {
              pfi.id = 0
            }
          }
        });
      }
      editedPayment.id = e.id;

      this._api.put<PaymentModel>(`api/payment/` + this.actualCompanyId, editedPayment).pipe().subscribe(
        res => {
          counter++;
          if (this.dataSource.data.length == counter) {
            this.dialog.open(AskIfDialog, {
              disableClose: true,
              height: 'auto',
              width: 'auto',
              data: { data: errors.length > 0 ? "PEYMENTS.PAYMENT_EDIT.PAYMENT_EDIT_ERROR" : "PEYMENTS.PAYMENT_EDIT.PAYMENT_EDIT_SUCCES", close: "CLOSE", ok: null }
            });
          }


        },
        error => {
          counter++;
          errors.push(error);
          if (this.dataSource.data.length == counter) {
            this.dialog.open(AskIfDialog, {
              disableClose: true,
              height: 'auto',
              width: 'auto',
              data: { data: errors.length > 0 ? "PEYMENTS.PAYMENT_EDIT.PAYMENT_EDIT_ERROR" : "PEYMENTS.PAYMENT_EDIT.PAYMENT_EDIT_SUCCES", close: "CLOSE", ok: null }
            });
          }
        });
    });
    this.updateFormInvoices(currentIndex);
  }
  //filters
  filterContractors(value: any): ContractorPaymentFormModel[] {
    let filterValue = "";
    if (typeof (value) === "object" && value != null) {
      filterValue = value.name.toLowerCase();
    } else {
      filterValue = value.toLowerCase();
    }
    const filterdContractor = this.contractors.filter(option => option.name ? option.name.toLowerCase().indexOf(filterValue) === 0 : false);
    const choosedContractor = filterdContractor.filter(fc => fc.name.toLowerCase() === filterValue);
    if (choosedContractor.length != 0) {
      this.getContractorbankAccounts(choosedContractor[0].id).then(result => {
        this.contractorBankAccounts = result;
        this.contractorBankAccountsOptions.next(this.contractorBankAccounts);

      });
      this.getInvoices(choosedContractor[0].id).then(result => {
        this.invoices = result;

        this.invoicesOptionsList.forEach(iol => {
          iol.next(this.invoices);
        });
      });
    } else {
      this.clearInvoices();
      this.paymentForm.controls['contractorBankAccount'].markAsDirty();
      this.paymentForm.controls['contractorBankAccount'].patchValue('');
      this.invoicesOptionsList = [];
      const invoicesFormControles = <FormArray>this.paymentForm.controls['paymentsForInvoices'];
      invoicesFormControles.clear();
      this.addInvoice(0);
      invoicesFormControles.controls['0'].markAsDirty();
      this.invoices = null;
      this.contractorBankAccounts = null;
    }
    return filterdContractor;
  }

  filterContractorBankAccounts(value: any): ContractorBankAccountPaymentFormModel[] {
    let filterValue = "";
    if (typeof (value) === "object" && value != null) {
      filterValue = value.bankName.toLowerCase();
    } else {
      filterValue = value.toLowerCase();
    }
    if (this.contractorBankAccounts == null) {
      return [];
    }
    return this.contractorBankAccounts.filter(option => option.bankName ? option.bankName.toLowerCase().indexOf(filterValue) === 0 : false);
  }

  filterInvoices(value: any): InvoicePaymentFormModel[] {
    let filterValue = "";
    if (typeof (value) === "object" && value != null) {
      filterValue = value.number.toLowerCase();
    } else {
      filterValue = value.toLowerCase();
    }
    if (this.invoices == null) {
      return [];
    }
    const filterdInvoices = this.invoices.filter(option => option.number ? this.paymentForm.controls.paymentsForInvoices.value.map(v => v.invoice.number).indexOf(option.number) == -1 : false);
    return filterdInvoices.filter(option => option.number ? option.number.toLowerCase().indexOf(filterValue) === 0 : false);

  }
  filterPaymentCurrency(value: string): any {
    return this.currencyCodes.codes().filter(option => option.toLowerCase().indexOf(value.toLowerCase()) === 0);
  }

  //display autocomplete
  displayPaymentCurrencyAutocomplet(displayValue: string): string {
    return displayValue + ' | ' + getCurrencySymbol(displayValue, 'narrow');
  }
  displayContractorAutocomplet(displayValue) {
    return typeof displayValue == 'object' && displayValue != null ? displayValue.name : displayValue;
  }
  displayContractorBankAccountAutocomplet(displayValue) {
    return typeof displayValue == 'object' && displayValue != null ? displayValue.bankName : displayValue;
  }
  displayInvoiceAutocomplet(displayValue) {
    return typeof displayValue == 'object' && displayValue != null ? displayValue.number : displayValue;
  }

  //helpers
  async getInvoices(contractorId: number) {
    return this._api.get<Invoice[]>(`api/invoice/contractorInvoices/` + this.actualCompanyId, { contractorId: contractorId }).pipe(map(im => im.map(i => new InvoicePaymentFormModel(i)))).toPromise();
  }
  async getContractorbankAccounts(contractorId: number) {
    return this._api.get<ContractorBankAccount[]>(`api/contractor/contractorbankaccounts/` + this.actualCompanyId, { contractorId: contractorId }).pipe(map(im => im.map(i => new ContractorBankAccountPaymentFormModel(i)))).toPromise();
  }

  addInvoice(counter: number, invoicesPaymentFormModel?: PaymentsForInvoicesFormModel, newIndex = this.page) {
    const control = <FormArray>this.paymentForm.controls['paymentsForInvoices'];
    const invoiceControl = this.formBuilder.group({
      id: invoicesPaymentFormModel ? invoicesPaymentFormModel.id : 0,
      invoice: [invoicesPaymentFormModel ? invoicesPaymentFormModel.invoice : '', {
        updateOn: 'change',
        validators: Validators.compose([Validators.required, checkType('object')]),
        asyncValidators: []
      }],
      paymentValueForInvoice: [invoicesPaymentFormModel ? invoicesPaymentFormModel.paymentValueForInvoice : 0,
      {
        updateOn: 'blur',
        validators: [
          Validators.min(0.01),
          this.maxPaymentValueForInvoiceValidator(counter),
          Validators.required
        ],
        asyncValidators: []
      }]
    })
    const invoiceChange = this.invoicesChanges[newIndex][counter];
    if (invoiceChange) {
      if (Object.keys(invoiceChange).length === 1) {
        invoiceControl.disable();
        invoiceControl.controls.id.setValue(invoiceChange['id']);
        invoiceControl.controls.id.markAsDirty();
      } else {
        Object.entries(invoiceChange).forEach(([k, v]) => {
          invoiceControl.controls[k].setValue(v);
          invoiceControl.controls[k].markAsDirty();
        });
      }
    }
    control.insert(counter, invoiceControl);

    invoiceControl.valueChanges.pipe(
    ).subscribe(i => {
      /*      if (!invoiceControl.controls.id.dirty) {
              invoiceControl.controls.id.markAsDirty();
            }*/
      control.controls.forEach((c, index) => {
        if (index != counter) {
          c['controls'].paymentValueForInvoice.setErrors(null, true);
        }
      });
    });
    const observableinvoicesOptions = this.paymentForm.get('paymentsForInvoices')['controls'][counter]['controls']['invoice'].valueChanges.pipe(
      startWith(invoicesPaymentFormModel ? invoicesPaymentFormModel.invoice : ''),
      map(value => {
        return this.filterInvoices(value);
      }));
    this.invoicesOptionsList.push(new BehaviorSubject<InvoicePaymentFormModel[]>([]));
    observableinvoicesOptions.subscribe(this.invoicesOptionsList[counter]);

    this.enabledInvoicesLength = this.paymentForm.controls['paymentsForInvoices']['controls'].filter(c => c.enabled).length;
  }
  removeInvoice(i: number, ifRemoveFromDataBase = false) {
    const control = <FormArray>this.paymentForm.controls['paymentsForInvoices'];
    if (ifRemoveFromDataBase) {
      this.invoicesChanges[this.page][i] = { id: control.at(i).value.id };
      control.controls[i].markAsDirty();
      control.controls[i].disable();
      control.markAsDirty();
    } else {
      this.invoicesOptionsList.splice(i, 1);
      control.removeAt(i);
    }
    this.enabledInvoicesLength = this.paymentForm.controls['paymentsForInvoices']['controls'].filter(c => c.enabled).length;
  }
  changeInvoiceFocus(i: number) {
    this.InvoiceDictionaryIndex = i;
  }
  clearInvoices() {
    const invoicesFormControles = <FormArray>this.paymentForm.controls['paymentsForInvoices'];
    invoicesFormControles.clear();
  }
  getDirtyValues(form: any) {
    let dirtyValues = {};
    if (form instanceof FormArray) {
      dirtyValues = [];
    }
    Object.keys(form.controls)
      .forEach(key => {
        let currentControl = form.controls[key];

        if (currentControl.dirty) {
          if (currentControl.controls) {
            dirtyValues[key] = this.getDirtyValues(currentControl);
          }
          else {
            dirtyValues[key] = currentControl.value;
          }
        }
      });

    return dirtyValues;
  }
  maxPaymentValueForInvoiceValidator(counter): ValidatorFn {
    return (control: AbstractControl): { [key: string]: any } | null => {
      if (control.parent && control.parent.parent) {
        let restOfInvoiceControlsPaymentValues = control.parent.parent.value.map((i, index) => { if (index != counter) { return i.paymentValueForInvoice } }).filter(pvfi => pvfi != undefined);
        if (restOfInvoiceControlsPaymentValues.length != 0) {
          if (restOfInvoiceControlsPaymentValues.length != 1) {
            restOfInvoiceControlsPaymentValues = restOfInvoiceControlsPaymentValues.reduce((a, b) => a + b, 0)
          } else {
            restOfInvoiceControlsPaymentValues = restOfInvoiceControlsPaymentValues[0];
          }
        } else {
          restOfInvoiceControlsPaymentValues = 0;
        }
        const paymentValue = control.parent.parent.parent.controls['paymentValue'].value;
        const actualSumPaymentValues = restOfInvoiceControlsPaymentValues + control.value;
        const returnvalue = actualSumPaymentValues <= paymentValue ? null : { tooMuchPaymentValue: true };
        return returnvalue;
      }
      return null;
    };
  }
}
