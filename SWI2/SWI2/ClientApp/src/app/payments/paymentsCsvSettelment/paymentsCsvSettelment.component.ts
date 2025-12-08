import { HttpEvent, HttpEventType, HttpParams, HttpResponse } from "@angular/common/http";
import { Component, ElementRef, Inject } from "@angular/core";
import { AbstractControl, FormArray, FormBuilder, FormControl, FormGroup, ValidatorFn, Validators } from "@angular/forms";
import { MatDialog, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { number } from "currency-codes";
import { forEach } from "jszip";
import * as moment from "moment";
import { resolve } from "path";
import { BehaviorSubject, pipe } from "rxjs";
import { filter, map, startWith, tap } from "rxjs/operators";
import { isArray } from "util";
import { Contractor } from "../../models/contractors/Contractor.model";
import { ContractorBankAccount } from "../../models/contractors/ContractorBankAccount.model";
import { Invoice } from "../../models/invoices/Invoice.model";
import { ContractorBankAccountPaymentFormModel, ContractorBankAccountPaymentMachingModel, ContractorPaymentFormModel, ContractorPaymentMachingModel, InvoicePaymentFormModel, PaymentFormModel, PaymentMachingModel, PaymentModel, PaymentsForInvoicesFormModel } from "../../models/payments/Payment.model";
import { TableParamsModel } from "../../models/tableParams.model";
import { AskIfDialog } from "../../shared/components/askIfDialog/askIfDialog.component";
import { requiredFileType } from "../../shared/custom-validators/requiredFileType.service";
import { ApiService } from "../../shared/services/api.service";
import { AuthenticationService } from "../../shared/services/authentication.service";
import { ErrorDialogService } from "../../shared/services/error-dialog-service";

@Component({
  selector: 'swi-paymentsCsvSettelment',
  templateUrl: './paymentsCsvSettelment.component.html',
  styleUrls: ['./paymentsCsvSettelment.component.css']
})

export class paymentsCsvSettelment {

  actualCompanyId: number;
  paymentsMachingSpecificationForm: FormGroup;
  matchedPaymentsForm: FormGroup;
  progerssValue = 0;
  progressBufferValue = 10;
  percentDone: number;
  isMachingInvoicesFound = false;
  lines = [];
  linesR = [];

  currencyCodes = require('currency-codes');
  currencyOptions: BehaviorSubject<string[]>;

  contractorList: Contractor[] = [];
  contractorOptions: BehaviorSubject<Contractor[]>;

  bankAccountList: ContractorBankAccountPaymentFormModel[] = [];
  bankAccountOptions: BehaviorSubject<ContractorBankAccountPaymentFormModel[]>;

  invoiceList: InvoicePaymentFormModel[] = [];
  invoiceOptions: BehaviorSubject<InvoicePaymentFormModel[]>;

  currency: string = null;
  companyAccountNumber: string = null;
  csvRaportStartDate: string = null;
  csvRaportEndDate: string = null;
  csvPaymentSendingStage = 0;
  paymentFocus: number;

  constructor(public dialog: MatDialog,
    private formBuilder: FormBuilder,
    private _api: ApiService,
    public _authService: AuthenticationService,
    private errorD: ErrorDialogService) {
    this._authService.currentSelectedCompany.subscribe(c => { this.actualCompanyId = c.id; });

    this.paymentsMachingSpecificationForm = this.formBuilder.group({
      file: [null, Validators.compose([Validators.required, requiredFileType('csv')])],
      ifSeriesSettelement: [true, Validators.required],
      ifFromTheLastSetteld: [false, Validators.nullValidator],
      byTitle: [null, Validators.nullValidator],
      byValue: [null, Validators.nullValidator],
      byAddressee: [null, Validators.nullValidator],
      ifAddNewContractorBankAccounts: [true, Validators.nullValidator
      ]
    });
    this.matchedPaymentsForm = this.formBuilder.group({
      payments: formBuilder.array([])
    });
  }

  ngAfterViewInit() {

    this._api.get<Contractor[]>(`api/contractor/` + this.actualCompanyId, { query: btoa(JSON.stringify({ pageSize: -1, sort: 'created' + " " + 'asc' } as TableParamsModel)) }).subscribe(result => {
      this.contractorList = result;
    });
  }
  readCsv() {
    let file = this.paymentsMachingSpecificationForm.controls.file.value;
    //const headers = ['Data operacji'	,'Data księgowania'	,'Opis operacji',	'Tytuł'	,'Nadawca / Odbiorca'	,'Numer konta','Kwota']
    let rows: string[][] = [];
    let reader: FileReader = new FileReader();
    reader.readAsText(file, "UTF-8");
    reader.onload = (e) => {
      let allTextLines = (reader.result as string).split(/\r|\n|\r/);
      let ifTable = false;
      let ifTableEnd = false;
      let separatingSign = '';
      let accountNumberIndex: any = [];
      let valueIndex: any = [];
      let topicIndex: any = [];
      let contractorIndex: any = [];

      for (let i = 0; i < 1; i++) {
        const commaSignlength = allTextLines[i].split(',').length;
        const semicolonSignlength = allTextLines[i].split(';').length;
        if (commaSignlength > semicolonSignlength) {
          separatingSign = ',';
        }
        else {
          separatingSign = ';';
        }
      }


      for (let i = 0; i < allTextLines.length; i++) {
        if (allTextLines[i].length != 0) {
          let cells: string[] = allTextLines[i].split(separatingSign);

          let operationDate = moment(cells[0], ["MM-DD-YYYY", "YYYY-MM-DD", "DD.MM.YYYY"]);
          let accountingDate = moment(cells[1], ["MM-DD-YYYY", "YYYY-MM-DD", "DD.MM.YYYY"]);

          if ((cells.length > 2 && cells[2] != '') && !ifTableEnd && operationDate.isValid() && accountingDate.isValid()) {
            if (!ifTable) {
              if (cells.length > 2 && cells[2] != '') {
                if (this.companyAccountNumber === null) {
                  cells.forEach((cell, index) => {
                    if (cell.length != 0) {
                      /*                      if (cell[cell.length - 1] == " ") {
                                             cell = cell.slice(0,cell.length - 2);
                                            }*/
                      if (index != 0 && index != 1 && cell.split(" ").length == 7 && (cell.length == 33 || cell[0] === "'")) {
                        this.companyAccountNumber = cell;
                      } else if (this.currencyCodes.codes().filter(option => option.indexOf(cell) === 0).length == 1) {
                        this.currency = cell;
                      }
                    }
                  });
                }
                if (!this.csvRaportStartDate && !this.csvRaportEndDate) {
                  this.csvRaportStartDate = operationDate.format("YYYY-MM-DD");
                  this.csvRaportEndDate = accountingDate.format("YYYY-MM-DD");
                }
              }
              i--;
              ifTable = true;
            } else {
              let separatedCell = '';
              let newCells = [];
              cells.forEach((cell, index) => {
                if (cell.length > 0) {
                  if (separatedCell.length != 0) {
                    if (cell[cell.length - 1] === '"') {
                      newCells.push(separatedCell + separatingSign + cell.slice(0, cell.length - 1));
                      separatedCell = '';
                    } else {
                      separatedCell += separatingSign + cell;
                    }
                  } else {
                    if (cell[0] === '"' || cell[0] === "'") {
                      if (cell[cell.length - 1] === '"' || cell[cell.length - 1] === "'") {
                        newCells.push(cell.slice(1, cell.length - 1));
                      } else {
                        separatedCell = cell.slice(1);
                      }
                    } else {
                      newCells.push(cell);
                    }
                  }
                } else {
                  newCells.push(cell);
                }
              });
              rows.push(newCells);
            }
          } else {
            if (ifTable) {
              ifTable = false;
              ifTableEnd = true;
            } else {

              if (this.companyAccountNumber === null) {
                cells.forEach((cell, index) => {
                  if (cell.length != 0) {
                    if (cell.split(" ").length == 7 && (cell.length == 33 || cell[0] === '\'')) {
                      this.companyAccountNumber = cell;
                    } else if (this.currencyCodes.codes().filter(option => option.indexOf(cell) === 0).length == 1) {
                      this.currency = cell;
                    }
                  }
                });
              }
              if (!this.csvRaportStartDate && !this.csvRaportEndDate && operationDate.isValid() && accountingDate.isValid()) {
                this.csvRaportStartDate = operationDate.format("YYYY-MM-DD");
                this.csvRaportEndDate = accountingDate.format("YYYY-MM-DD");
              }
            }
          }
        }
      }

      let checkLength = 100;
      let rowsToRemove = [];
      const accountNumberRegEx = /^(([A-Z| ]{0,10})(?:[0-9]{26}|[0-9]{2}( [0-9]{4}){6})|[0-9]{2}(-[0-9]{4}){6}|[0-9]{2}( [0-9]{3}){8})$/;
      if (rows.length < checkLength) {
        checkLength = rows.length;
      }
      for (let i = 0; i < checkLength; i++) {
        const row = rows[i];

        let suspectedPaymentValue: any = null;
        let ifNumberValue = null;
        let stringToNumberPreParsing = '';
        let ifAccountNumber = false;
        row.forEach((cell, index) => {
          if (index != 0 && index != 1) {
            stringToNumberPreParsing = cell.replace(" ", "").replace(",", ".");
            ifNumberValue = parseFloat(stringToNumberPreParsing);
            const indexOfComma = stringToNumberPreParsing.indexOf(".");

            if (accountNumberRegEx.test(cell)) {
              accountNumberIndex.push(index);
              topicIndex.push(index - 2);
              contractorIndex.push(index - 1);
              /*              ifAccountNumber = true;*/
            } else if (ifNumberValue && ifNumberValue.toFixed(indexOfComma != -1 ? stringToNumberPreParsing.slice(indexOfComma + 1).length : 0).toString().length == stringToNumberPreParsing.length && suspectedPaymentValue != 0) {
              if (ifNumberValue < 0) {
                /*                rowsToRemove.push(i);*/
                suspectedPaymentValue = 0;
              } else if (suspectedPaymentValue == null) {
                suspectedPaymentValue = [ifNumberValue, index];
              } else {
                if (ifNumberValue > suspectedPaymentValue[0]) {
                  valueIndex.push(suspectedPaymentValue[1]);
                  suspectedPaymentValue = 0;
                }
              }
              ifNumberValue = 0;
            }
          }
        });
        if (suspectedPaymentValue != null && suspectedPaymentValue != 0) {
          valueIndex.push(suspectedPaymentValue[1]);
        }

      };
      accountNumberIndex = this.takeMostFrequendElement(accountNumberIndex);
      topicIndex = this.takeMostFrequendElement(topicIndex);
      contractorIndex = this.takeMostFrequendElement(contractorIndex);
      valueIndex = this.takeMostFrequendElement(valueIndex);
      if (this.companyAccountNumber[0] == "'") {
        this.companyAccountNumber = this.companyAccountNumber.slice(1);
      }
      if (accountNumberIndex && topicIndex && contractorIndex && valueIndex && this.companyAccountNumber && this.currency && this.csvRaportStartDate && this.csvRaportEndDate) {
        let payments = <FormArray>this.matchedPaymentsForm.controls.payments;
        rows.forEach(r => {
          const paymentValue = parseFloat(r[valueIndex].replace(' ', '').replace(',', '.'))
          if (r[accountNumberIndex] != null && r[accountNumberIndex] != '' && r[accountNumberIndex] != null && paymentValue != null && paymentValue > 0) {
            const newPayment = new PaymentMachingModel();
            newPayment.contractorBankAccount = this.formBuilder.group({
              id: 0,
              accountNumber: r[accountNumberIndex],
              contractor: { id: 0, name: ' ' } as ContractorPaymentMachingModel
            } as ContractorBankAccountPaymentMachingModel);
            newPayment.paymentsForInvoices = this.formBuilder.array([]);
            newPayment.addressee = r[contractorIndex];
            newPayment.topic = r[topicIndex];
            newPayment.paymentValue = paymentValue;
            newPayment.currency = this.currency;
            newPayment.paymentDate = moment(r[0], ["MM-DD-YYYY", "YYYY-MM-DD", "DD.MM.YYYY"]).toDate();
            payments.push(this.formBuilder.group(newPayment));
          }

        });

        this.csvPaymentSendingStage = 1;
      } else {
        this.csvPaymentSendingStage = -1;
      }
    }

  }

  removePayment(index: number) {
    (<FormArray>this.matchedPaymentsForm.controls.payments).removeAt(index);
  }
  findMachingInvoices() {
    let matchedPaymentsForm = this.matchedPaymentsForm.value;
    const paymentsMachingSpecificationForm = this.paymentsMachingSpecificationForm.value;
    matchedPaymentsForm = matchedPaymentsForm.payments.map(mf => { delete mf.paymentsForInvoices; return mf });
    let params = new HttpParams()
      .set('ifSeriesSettelement', paymentsMachingSpecificationForm.ifSeriesSettelement)
      .set('ifFromTheLastSetteld', paymentsMachingSpecificationForm.ifFromTheLastSetteld)
      .set('accountNumber', this.companyAccountNumber)
      .set('currency', this.currency);
    if (!paymentsMachingSpecificationForm.ifSeriesSettelement) {
      if (paymentsMachingSpecificationForm.byTitle) {
        params = params.set('byTitle', paymentsMachingSpecificationForm.byTitle);
      }
      if (paymentsMachingSpecificationForm.byAddressee) {
        params = params.set('byAddressee', paymentsMachingSpecificationForm.byAddressee);
      }
      if (paymentsMachingSpecificationForm.byValue) {
        params = params.set('byValue', paymentsMachingSpecificationForm.byValue);
      }
    }
    this._api.post<PaymentMachingModel[]>("/api/invoice/findmachinginvoices/" + this.actualCompanyId, matchedPaymentsForm, params).subscribe(response => {
      let payments = <FormArray>this.matchedPaymentsForm.controls.payments;
      payments.clear();

      response.forEach((p, index) => {
        const contractorBankAccount = p.contractorBankAccount ? (p.contractorBankAccount as ContractorBankAccountPaymentMachingModel) : { id: 0, bankName: '', accountNumber: '', contractor: { id: 0, name: '' } as ContractorPaymentMachingModel } as ContractorBankAccountPaymentMachingModel;
        const contractor = contractorBankAccount.contractor ? contractorBankAccount.contractor as ContractorPaymentMachingModel : { id: 0, name: '' } as ContractorPaymentMachingModel;
        const paymentsForInvoices = p.paymentsForInvoices ? (p.paymentsForInvoices as PaymentsForInvoicesFormModel[]) : [];

        p.contractorBankAccount = this.formBuilder.group({
          id: contractorBankAccount.id ? contractorBankAccount.id : 0,
          accountNumber: { value: contractorBankAccount.accountNumber, disabled: contractor.id == 0 ? true : false },
          contractor: { id: contractor.id, name: contractor.name } as ContractorPaymentMachingModel
        });
        p.paymentsForInvoices = this.formBuilder.array([]);
        if (paymentsForInvoices.length != 0) {
          paymentsForInvoices.forEach((pfi, index) => {
            const invoiceControl = this.formBuilder.group({
              id: pfi.id,
              invoice: pfi.invoice,
              paymentValueForInvoice: [pfi.paymentValueForInvoice,
              {
                updateOn: 'blur',
                validators: [
                  Validators.min(0.01),
                  this.maxPaymentValueForInvoiceMaxValueValidator(),
                  this.maxPaymentValueForInvoiceValidator(index),
                  Validators.required
                ],
                asyncValidators: []
              }]
            });
            (p.paymentsForInvoices as FormArray).push(invoiceControl);
          });
        }
        payments.push(this.formBuilder.group(p));
      });
      this.csvPaymentSendingStage = 2;
    });
  }

  async sendPayments() {
    let payments = this.matchedPaymentsForm.controls.payments.value.filter(p => p.contractorBankAccount.contractor && p.contractorBankAccount.contractor.id != 0);
    let isAllAdded = [];
    let countRespons = 0;
    if (this.paymentsMachingSpecificationForm.controls.ifAddNewContractorBankAccounts.value) {
      let groupedUncreatedContractorBankAccount = payments.filter(p => {
        const contractorBankAccount = <ContractorBankAccountPaymentMachingModel>p.contractorBankAccount;
        const contractor = <ContractorPaymentMachingModel>contractorBankAccount.contractor;
        return contractor != null && contractor.id != 0 && contractorBankAccount.id == 0
      }).map(p => { return p.contractorBankAccount; }).reduce((rv, x) => {
        if (!rv[x.accountNumber]) {
          rv[x.accountNumber] = []
        }
        (rv[x.accountNumber][x.contractor.id] = rv[x.accountNumber][x.contractor.id] || []).push(x);
        return rv;
      }, {});

      const bankAccountGrup = Object.keys(groupedUncreatedContractorBankAccount);
      for (let i = 0; i < bankAccountGrup.length; i++) {
        const accountNumber = bankAccountGrup[i];
        const contractorBankAccountGrup = Object.keys(groupedUncreatedContractorBankAccount[accountNumber]);
        for (let j = 0; j < contractorBankAccountGrup.length; j++) {
          const contarctorId = contractorBankAccountGrup[j];
          const newBankAccount = await this.AddNewBankAccount({
            id: 0,
            contractor: { id: Number(contarctorId) },
            bankName: accountNumber,
            accountNumber: accountNumber
          } as ContractorBankAccountPaymentMachingModel
          );
          payments = payments.map(p => {
            if (p.contractorBankAccount.contractor && p.contractorBankAccount.accountNumber == accountNumber && p.contractorBankAccount.contractor.id == contarctorId) {
              p.contractorBankAccount = newBankAccount;
            }
            return p;
          });
        }
      }
    } else {
      payments = payments.map(p => {
        if (p.contractorBankAccount.contractor && typeof p.contractorBankAccount.accountNumber == 'object') {
          p.contractorBankAccount = this.contractorList.filter(c => c.id === p.contractorBankAccount.contractor.id)[0].contractorBankAccounts.filter(cba => cba.bankName == 'unknown')[0];
        }
        return p;
      });
    }

    for (let i = 0; i < payments.length; i++) {
      const p = payments[i];
      const contractorBankAccount = <ContractorBankAccountPaymentMachingModel>p.contractorBankAccount;
      if (p.paymentsForInvoices.length == 0) {
        delete p.paymentsForInvoices;
      }
      const newPayment = new PaymentModel(p);
      this._api.post<PaymentFormModel>(`/api/payment/` + this.actualCompanyId, newPayment).subscribe(
        res => {
        },
        error => {
          this.errorD.showDialog(error);
          isAllAdded.push(newPayment.paymentsForInvoices.toString());
        },
        () => {
          countRespons++;
          if (countRespons >= payments.length) {
            this.dialog.open(AskIfDialog, {
              disableClose: true,
              data: { data: isAllAdded.length == 0 ? "PAYMENTS.PAYMENT_CSV.PAYMENT_ADDED_SUCCES" : "PAYMENTS.PAYMENT_CSV.PAYMENT_ADDED_ERROR", close: "CLOSE", ok: null }
            });
          }
        }
      );
    };
  }

  //helpres
  toFormData<T>(formValue: T) {
    const formData = new FormData();

    for (const key of Object.keys(formValue)) {
      const value = formValue[key];
      formData.append(key, value);
    }

    return formData;
  }

  uploadProgress<T>(cb: (progress: number) => void) {
    return tap((event: HttpEvent<T>) => {
      if (event.type === HttpEventType.UploadProgress) {
        cb(Math.round((100 * event.loaded) / event.total));
      }
    });
  }
  toResponseBody<T>() {
    return pipe(
      filter((event: HttpEvent<T>) => event.type === HttpEventType.Response),
      map((res: HttpResponse<T>) => res.body)
    );
  }
  takeMostFrequendElement(array: any[]) {
    return Object.entries(
      array.reduce((a, v) => {
        a[v] = a[v] ? a[v] + 1 : 1;
        return a;
      }, {})
    ).reduce((a, v) => (v[1] >= a[1] ? v : a), [null, 0])[0];
  }

  filterContractor(value: string | Contractor): Contractor[] {
    let filterValue = '';
    if (value != null) {
      if (typeof value === 'object') {
        filterValue = value.name;
      } else {
        filterValue = value;
      }
    }
    else {
      return this.contractorList;
    }
    return this.contractorList.filter(option => option.name ? option.name.toLowerCase().indexOf(filterValue.toLowerCase()) === 0 : false);
  }
  filterBankAccount(value: string | ContractorBankAccountPaymentMachingModel): ContractorBankAccountPaymentFormModel[] {
    let filterValue = '';
    if (value != null) {
      if (typeof value === 'object') {
        filterValue = value.accountNumber;
      } else {
        filterValue = value;
      }
    }
    else {
      return this.bankAccountList;
    }
    return this.bankAccountList.filter(option => option.accountNumber ? option.accountNumber.toLowerCase().indexOf(filterValue.toLowerCase()) === 0 || option.bankName.toLowerCase().indexOf(filterValue.toLowerCase()) === 0 : false);
  }
  filterInvoice(value: string | Invoice): InvoicePaymentFormModel[] {
    let filterValue = '';
    if (value != null) {
      if (typeof value === 'object') {
        filterValue = value.number;
      } else {
        filterValue = value;
      }
    }
    else {
      return this.invoiceList;
    }
    return this.invoiceList.filter(option => option.number ? option.number.toLowerCase().indexOf(filterValue.toLowerCase()) === 0 : false);
  }
  filterCurrency(value: string): string[] {
    let filterValue = '';
    if (value != null) {
      filterValue = value;
    }
    else {
      return this.currencyCodes.codes();
    }
    return this.currencyCodes.codes().filter(option => option ? option.toLowerCase().indexOf(filterValue.toLowerCase()) === 0 : false);
  }

  ChangePaymentFocus(index: number) {
    this.paymentFocus = index;
    const payment = <FormGroup>(<FormArray>this.matchedPaymentsForm.controls.payments).at(index);
    const contractorBankAccount = (<FormGroup>payment.controls.contractorBankAccount);
    if (this.contractorOptions != null) {
      this.contractorOptions.unsubscribe();
    }
    if (this.bankAccountOptions != null) {
      this.bankAccountOptions.unsubscribe();
    }
    if (this.currencyOptions != null) {
      this.currencyOptions.unsubscribe();
    }
    const contractorOptions = contractorBankAccount.controls.contractor.valueChanges.pipe(
      startWith(contractorBankAccount.controls.contractor.value),
      map(value => {
        return this.filterContractor(value);
      })
    );
    this.contractorOptions = new BehaviorSubject<Contractor[]>([]);
    contractorOptions.subscribe(this.contractorOptions);

    if (typeof contractorBankAccount.controls.contractor.value == 'object' && contractorBankAccount.controls.contractor.value.id != 0) {
      this.bankAccountList = this.contractorList.filter(c => c.id === contractorBankAccount.controls.contractor.value.id)[0].contractorBankAccounts.map(cba => new ContractorBankAccountPaymentFormModel(cba));
      this.LoadInvoiceOptions(contractorBankAccount.controls.contractor.value.id);
    }
    const bankAccountOptions = contractorBankAccount.controls.accountNumber.valueChanges.pipe(
      startWith(contractorBankAccount.controls.accountNumber.value),
      map(value => {
        return this.filterBankAccount(value);
      })
    );
    this.bankAccountOptions = new BehaviorSubject<ContractorBankAccountPaymentFormModel[]>([]);
    bankAccountOptions.subscribe(this.bankAccountOptions);

    const currencyOptions = (<FormControl>payment.controls.currency).valueChanges.pipe(
      startWith(payment.controls.currency.value),
      map(value => {
        return this.filterCurrency(value);
      })
    );
    this.currencyOptions = new BehaviorSubject<string[]>([]);
    currencyOptions.subscribe(this.currencyOptions);
  }
  LoadInvoiceOptions(contractorId) {
    return this._api.get<Invoice[]>(`api/invoice/contractorInvoices/` + this.actualCompanyId, new HttpParams().set('contractorId', contractorId.toString())).toPromise();
    /*      .subscribe(result => {
          this.invoiceList = result;
        });*/
  }

  async AddNewBankAccount(bankAccount: ContractorBankAccountPaymentMachingModel): Promise<ContractorBankAccountPaymentMachingModel> {
    return new Promise((resolve, reject) => {
      this._api.post<ContractorBankAccountPaymentMachingModel>(`api/contractor/bankaccount/` + (<ContractorPaymentMachingModel>bankAccount.contractor).id, bankAccount).toPromise().then(
        res => {
          resolve(res);
        },
        msg => {
          reject(msg);
        }
      );
    });
  }

  ChangeInvoiceFocus(paymentIndex: number, invoiceIndex) {
    const paymet = (<FormGroup>(<FormArray>this.matchedPaymentsForm.controls.payments).at(paymentIndex));
    const paymentForInvoices = <FormGroup>(<FormArray>paymet.controls.paymentsForInvoices).at(invoiceIndex);
    if (paymentIndex != this.paymentFocus && typeof paymet.value.contractorBankAccount.contractor == 'object' && this.invoiceList.length == 0) {
      this.LoadInvoiceOptions(paymet.value.contractorBankAccount.contractor.id).then(result => {
        this.invoiceList = result;
        this.paymentFocus = paymentIndex;
        const invoicesOptions = paymentForInvoices.controls.invoice.valueChanges.pipe(
          startWith(paymentForInvoices.controls.invoice.value),
          map(value => {
            return this.filterInvoice(value);
          })
        );
        this.invoiceOptions = new BehaviorSubject<InvoicePaymentFormModel[]>([]);
        invoicesOptions.subscribe(this.invoiceOptions);
      });
    } else {
      this.paymentFocus = paymentIndex;
      const invoicesOptions = paymentForInvoices.controls.invoice.valueChanges.pipe(
        startWith(paymentForInvoices.controls.invoice.value),
        map(value => {
          return this.filterInvoice(value);
        })
      );
      this.invoiceOptions = new BehaviorSubject<InvoicePaymentFormModel[]>([]);
      invoicesOptions.subscribe(this.invoiceOptions);
    }
  }
  displayAccountNumberAutocomplet(bankAccount) {
    return typeof bankAccount == 'object' ? bankAccount.bankName : bankAccount;
  }
  displayContractorAutocomplet(contractor) {
    return typeof contractor == 'object' ? contractor.name : contractor;
  }

  invoiceAutocomplet(invoice) {
    return typeof invoice == 'object' ? invoice.number : invoice;
  }

  contarctorAutoChnged(index, event) {
    const accountNumber = (<FormGroup>(<FormGroup>(<FormArray>this.matchedPaymentsForm.controls.payments).at(index)).controls.contractorBankAccount).controls.accountNumber;
    if (accountNumber.disabled) {
      accountNumber.enable();
    }
    this.bankAccountList = event.option.value.contractorBankAccounts.map(cba => new ContractorBankAccountPaymentFormModel(cba));
    this.LoadInvoiceOptions(event.option.value.id);
  }

  AddDefaultPaymentForInvoice(paymentIndex) {
    const paymentForInvoices = (<FormArray>(<FormGroup>(<FormArray>this.matchedPaymentsForm.controls.payments).at(paymentIndex)).controls.paymentsForInvoices);
    const invoiceControl = this.formBuilder.group({
      id: 0,
      invoice: '',
      paymentValueForInvoice: [0,
        {
          updateOn: 'blur',
          validators: [
            Validators.min(0.01),
            this.maxPaymentValueForInvoiceMaxValueValidator(),
            this.maxPaymentValueForInvoiceValidator(paymentForInvoices.length),
            Validators.required
          ],
          asyncValidators: []
        }]
    });
    paymentForInvoices.push(invoiceControl);
  }

  bankAccountAutoChnged(paymentIndex, event) {
    const paymentForInvoices = (<FormGroup>(<FormGroup>(<FormArray>this.matchedPaymentsForm.controls.payments).at(paymentIndex)).controls.contractorBankAccount);
    paymentForInvoices.patchValue({
      id: event.option.value.id
    });
  }

  ContractorChange(paymentIndex) {
    const paymet = (<FormGroup>(<FormArray>this.matchedPaymentsForm.controls.payments).at(paymentIndex));
    const paymentForInvoices = <FormArray>paymet.controls.paymentsForInvoices;
    paymentForInvoices.clear();
  }

  isObject(value) {
    return typeof value == 'object';
  }
  maxPaymentValueForInvoiceMaxValueValidator(): ValidatorFn {
    return (control: AbstractControl): { [key: string]: any } | null => {
      if (control.parent) {
        const invoice = (<FormGroup>control.parent).controls.invoice.value;
        if (typeof invoice == 'object') {
          if (invoice.bruttoWorth < control.value) {
            return { exceededMaxInvoiceValue: invoice.bruttoWorth - control.value };
          }
        }
      }
      return null;
    };
  }

  maxPaymentValueForInvoiceValidator(counter): ValidatorFn {
    return (control: AbstractControl): { [key: string]: any } | null => {
      if (control.parent && control.parent.parent) {
        (<FormArray>control.parent.parent).controls.forEach(i => {
          if (i != control.parent) {
            i['controls'].paymentValueForInvoice.setErrors(null, true);
          }
        });
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
