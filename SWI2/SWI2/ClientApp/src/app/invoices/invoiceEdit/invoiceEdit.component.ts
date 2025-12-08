import { Component, Inject, ViewChild } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Validators, FormBuilder, FormGroup, FormControl, FormArray } from '@angular/forms';
import { ApiService } from '../../shared/services/api.service';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { merge, Observable, fromEvent, BehaviorSubject } from 'rxjs';
import { debounceTime, distinctUntilChanged, map, startWith } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { AskIfDialog } from '../../shared/components/askIfDialog/askIfDialog.component';
import { Adress } from '../../shared/components/adressMatInput/adressMatInput.component';
import { CodeCoutry } from '../../shared/components/coutryCodeMatInput/codeCoutryMatInput.component';
import { invoiceEditCalculator } from '../../models/invoices/invoiceEditCalculator';
import { Invoice, InvoiceContractor, InvoiceEntries, InvoiceIssuer, SellDateName } from '../../models/invoices/Invoice.model';
import { CustomFormDialog, FormPart } from '../../shared/components/customFormDialog/customFormDialog.component';
import { MailLanguage } from '../../models/contractors/Contractor.model';
import { TableParamsModel } from '../../models/tableParams.model';
import { ContractorBankAccount } from '../../models/contractors/ContractorBankAccount.model';
import { isArray, isString } from 'util';
import { PaymentMethod } from '../../company/payment-method';
import { start } from 'repl';
import { getCurrencySymbol } from '@angular/common';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'swi-invoiceEdit',
  templateUrl: './invoiceEdit.component.html',
  styleUrls: ['./invoiceEdit.component.css']
})

export class InvoiceEditComponent {
  _envUrl: string = environment.urlAddress;

  dataSource = new MatTableDataSource<Invoice>();
  invoiceForm: FormGroup;
  EntrysPriceChanges$;
  FormValueChanges$;
  FormIssuerChanges$;
  FormContractorChanges$;
  FormFocusedEntryChangedChanges$;

  displayedCalculatorColumns: string[] = ['vatRate', 'nettoValue', 'vatValue', 'bruttoValue'];
  calculator = new MatTableDataSource<invoiceEditCalculator>();
  @ViewChild(MatPaginator) paginator: MatPaginator;
  page = 0;

  sellDateName: SellDateName[] = [];
  sellDateNameOptions: Observable<SellDateName[]>;
  contractors: InvoiceContractor[] = [];
  contractorsOptions: Observable<InvoiceContractor[]>;
  entriesDictionaryOptionsList: BehaviorSubject<InvoiceEntries[]>[] = [];
  paymentBankOptions: Observable<PaymentMethod[]>;
  entriesDictionary: InvoiceEntries[] = [];
  companyPaymentMethods: PaymentMethod[] = [];
  entriesDictionaryOptionsIndex = null;

  isChanges = false;
  ifCanAddEntryToDictionary = false;
  ifCanEditEntryToDictionary = false;
  EditedEntryDictionary = {};
  filterdIssuer = {};
  currentContarctor = {};
  filterdContractor = {};
  enabledInvoicesLength = 1;
  invoiceEntriesChanges = [];
  invoiceNumbersChanges = [];
  paymentCurrencyOptions: Observable<string[]>;
  currencyCodes :any;
    currentCompanyId: number;
  constructor(
    private formBuilder: FormBuilder,
    private http: ApiService,
    public _authService: AuthenticationService,
    private _route: ActivatedRoute,
    public dialogRef: MatDialogRef<InvoiceEditComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { invoices: Invoice[]; action: number },
    public dialog: MatDialog
  ) {
    //get data
    this._authService.currentSelectedCompany.subscribe(company => {
      if (this._route.snapshot.params.id)
        this.currentCompanyId = this._route.snapshot.params.id;
      else {
        this.currentCompanyId = company['id'];
      }
    });

    this.dataSource.data = JSON.parse(JSON.stringify(this.data.invoices));
    this.invoiceEntriesChanges = this.data.invoices.map(i => { return i.invoiceEntries.map(ie => { return {} }) });
    this.invoiceNumbersChanges = this.data.invoices.map(i => { return i.number });

    const etrys = this.formBuilder.array([]);

    const firstInvoice = this.dataSource.data[0];

    this.enabledInvoicesLength = firstInvoice.invoiceEntries.length;

    firstInvoice.invoiceEntries.forEach((value) => {
      etrys.push(this.getNewEntry(value));
    })

    this.invoiceForm = this.formBuilder.group({
      id: firstInvoice.id,
      number: firstInvoice.number,
      correcting: [{ value: firstInvoice.correcting, disabled: true }],
      creationPlace: firstInvoice.creationPlace,
      isTransferType: firstInvoice.isTransferType,
      paymentBank: firstInvoice.paymentBank,
      paymentAccountNumber: firstInvoice.paymentAccountNumber,
      paymentCurrency: firstInvoice.paymentCurrency,
      rate: firstInvoice.rate,
      note: firstInvoice.note,
      sellDateName: firstInvoice.sellDateName,
      sellDate: firstInvoice.sellDate,
      paymentDate: firstInvoice.paymentDate,
      bruttoWorth: firstInvoice.bruttoWorth,
      paymentStatus: firstInvoice.paymentStatus,
      invoiceContractor: this.formBuilder.group({
        contractor: { name: firstInvoice.invoiceContractor.contractor.name, id: firstInvoice.invoiceContractor.contractor != null ? firstInvoice.invoiceContractor.contractor.id : 0 },
        name: firstInvoice.invoiceContractor.name,
        adress: new Adress(firstInvoice.invoiceContractor.street ?? '', firstInvoice.invoiceContractor.houseNumber ?? '', firstInvoice.invoiceContractor.apartamentNumber ?? '', firstInvoice.invoiceContractor.city ?? ''),
        codeCoutry: new CodeCoutry(firstInvoice.invoiceContractor.postalcode ?? '', firstInvoice.invoiceContractor.postoffice ?? '', firstInvoice.invoiceContractor.country ?? ''),
        nip: [firstInvoice.invoiceContractor.nip, Validators.compose([Validators.pattern("^([a-z]|[A-z])*[0-9]+$"), Validators.required])]
      }),
      created: firstInvoice.created,
      invoiceEntries: etrys,
    });
    this.invoiceForm.controls.isTransferType.valueChanges.pipe(startWith(this.invoiceForm.controls.isTransferType.value)).subscribe(isTransferType => {
      if (!isTransferType) {
        this.invoiceForm.controls.paymentBank.setValue(null);
        this.invoiceForm.controls.paymentBank.markAsDirty();
        this.invoiceForm.controls.paymentAccountNumber.setValue(null);
        this.invoiceForm.controls.paymentAccountNumber.markAsDirty();
        this.invoiceForm.controls.paymentCurrency.enable();
      } else {
        this.invoiceForm.controls.paymentCurrency.disable();
      }
    });

    //updateDictionarys

    this.http.get<InvoiceContractor[]>(`${this._envUrl}/api/contractor/` + this.currentCompanyId, { query: btoa(JSON.stringify({ pageSize: -1, sort: 'created' + " " + 'asc' } as TableParamsModel)) }).subscribe(result => {
      this.contractors = result;
      if (this.dataSource.data[0].invoiceContractor.contractor.id != 0) {
        this.currentContarctor = result.filter(c => c.id == this.dataSource.data[0].invoiceContractor.contractor.id)[0];
        this.invoiceForm.controls.invoiceContractor.patchValue({
          contractor: this.currentContarctor
        });
      }
      this.contractorsOptions = this.invoiceForm.controls.invoiceContractor['controls'].contractor.valueChanges.pipe(
        startWith(this.currentContarctor['name'] ? this.currentContarctor['name'] : this.invoiceForm.controls.invoiceContractor['controls'].contractor.value),
        map(value => {
          return this.filterContractors(value);
        })
      );
    });

    this.http.get<{ elements: PaymentMethod[], totalCount: number }>(`${this._envUrl}/api/companypanel/` + this.currentCompanyId + `/paymentmethod`).subscribe(result => {
      this.companyPaymentMethods = result.elements;
      this.paymentBankOptions = this.invoiceForm.get('paymentBank').valueChanges.pipe(
        startWith(''),
        map(value => {
          return this.filterPaymentBank(value);
        })
      );
    });

    this.http.get<InvoiceEntries[]>(`${this._envUrl}/api/invoice/etriedictionary?companyId=` + this.currentCompanyId).subscribe(result => {
      this.entriesDictionary = result;
      this.entriesDictionaryOptionsList.forEach((e) => {
        e.next(this.entriesDictionary);
      });
    });

    this.http.get<SellDateName[]>(`${this._envUrl}/api/invoice/selldatename`).subscribe(result => {
      this.sellDateName = result;
      this.sellDateNameOptions = this.invoiceForm.get('sellDateName').valueChanges.pipe(
        startWith(firstInvoice.sellDateName ? firstInvoice.sellDateName.name : ""),
        map(value => this.filterSellDateNames(value))
      );
    });
    this.currencyCodes = require('currency-codes');
    this.paymentCurrencyOptions = this.invoiceForm.get('paymentCurrency').valueChanges.pipe(
      startWith(''),
      map(value => {
        return this.filterPaymentCurrency(value);
      })
    );
  }

  ngAfterViewInit() {

    if (this.data.action == 1 || this.data.action == 2 || this.data.action == 3) {
      this.GenerateNewInvoiceNumbers();
    }

    this.dataSource.paginator = this.paginator;
    this.paginator.page
      .pipe(
        debounceTime(50),
        distinctUntilChanged()
      ).subscribe(
        index => {
          this.getNewPage(index.pageIndex);
        });

    this.invoiceForm.controls.invoiceEntries.valueChanges
      .pipe(startWith(this.invoiceForm.controls.invoiceEntries.value))
      .subscribe(invoiceEntries => {
        if (!this.isChanges && this.invoiceForm.controls.invoiceEntries.dirty) {
          this.isChanges = true;
        }
        this.updateTotalEntrysPrice(invoiceEntries)
      });
  }
  //invoiceEntries
  addEntry(ifInsert?: number, newInvoiceEntry?: InvoiceEntries, pageIndex?: number) {
    const control = <FormArray>this.invoiceForm.controls['invoiceEntries'];
    const newEntry = newInvoiceEntry ? this.getNewEntry(newInvoiceEntry) : this.getNewEntry();
    if (ifInsert != null) {
      const x = this.invoiceEntriesChanges[pageIndex != null ? pageIndex : this.page][ifInsert];
      if (Object.keys(x).length === 1) {
        newEntry.disable();
      }
      control.insert(ifInsert, newEntry);
    } else {
      control.push(newEntry);
    }
    this.enabledInvoicesLength = control.controls.filter(c => c.enabled).length;
  }

  getNewEntry(ie?: InvoiceEntries) {

    const numberPatern = '^[0-9\\-.,]+$';

    const entry = this.formBuilder.group({
      id: [ie != null ? ie.id : 0],
      name: [ie != null ? ie.name : '', Validators.required],
      quantity: [ie != null ? ie.quantity : 1, [Validators.required, Validators.pattern(numberPatern)]],
      price: [ie != null ? ie.price : 0, [Validators.required, Validators.pattern(numberPatern)]],
      vat: [ie != null ? ie.vat : 0, [Validators.required, Validators.pattern(numberPatern/*'^[0-9\\-.,]+%|zw|np$'*/)]],
      gtu: [{ value: ie != null ? ie.gtu : '', disabled: true }]
    });
    //entry dictionary apply
    const dictionaryOptions = entry.controls['name'].valueChanges.pipe(
      startWith(typeof (entry.controls['name'].value) === "object" && entry.controls['name'].value != null ? entry.controls['name'].value.name : entry.controls['name'].value),
      map(value => this.filterEntrie(value, entry))
    );
    const behaviorSubjectDictionaryOptions = new BehaviorSubject<InvoiceEntries[]>([]);
    dictionaryOptions.subscribe(behaviorSubjectDictionaryOptions);
    behaviorSubjectDictionaryOptions.subscribe(result => {

      if (this.entriesDictionaryOptionsIndex != null) {
        if (result.filter(a => a.name == entry['controls']['name'].value).length != 0) {
          this.ifCanEditEntryToDictionary = true;
          this.ifCanAddEntryToDictionary = false;
        } else {
          if (entry['controls']['name'].value != "" && entry['controls']['name'].value != undefined) {
            this.ifCanAddEntryToDictionary = true;
          } else {
            this.ifCanAddEntryToDictionary = false;
          }
          if (this.FormFocusedEntryChangedChanges$.closed) {
            this.FormFocusedEntryChangedChanges$.unsubscribe();
          }
          this.ifCanEditEntryToDictionary = false;
          this.EditedEntryDictionary = {};
        }
      }
    });
    this.entriesDictionaryOptionsList.push(behaviorSubjectDictionaryOptions);
    return entry;
  }

  removeEntry(i: number, removeFromDataBase = false) {
    const control = <FormArray>this.invoiceForm.controls['invoiceEntries'];
    if (removeFromDataBase) {
      this.invoiceEntriesChanges[this.page][i] = { id: control.at(i).value.id };
      control.at(i).disable();
      control.at(i).markAsDirty();
      if (!this.isChanges) {
        this.isChanges = true;
      }
    } else {
      control.removeAt(i);
      this.entriesDictionaryOptionsList.splice(i, 1);
    }

    this.enabledInvoicesLength = control.controls.filter(c => c.enabled).length;
  }

  addEntryToDictionary() {
    let invoiceEntries = {};

    Object.entries(this.invoiceForm.get('invoiceEntries')['controls'][this.entriesDictionaryOptionsIndex].value).forEach(([key, value]) => {
      if (key != 'quantity' && key != 'id') {
        invoiceEntries[key] = value;
      }
    });

    this.http.post<InvoiceEntries>(`${this._envUrl}/api/invoice/etriedictionary/` + this.currentCompanyId, invoiceEntries).subscribe({
      next: data => {
        this.entriesDictionary.push(data);
        this.entriesDictionaryOptionsList[this.entriesDictionaryOptionsIndex].next([data]);
      },
      error: error => {
        console.error('error wile correcting invoice :', error);
      }
    });
  }

  editEntryDictionary() {

    this.http.put<InvoiceEntries>(`${this._envUrl}/api/invoice/etriedictionary/` + this.EditedEntryDictionary['id'], this.EditedEntryDictionary).subscribe({
      next: data => {
        const index = this.entriesDictionary.map(function (e) { return e.id; }).indexOf(data.id);
        this.entriesDictionary[index] = data;
        this.entriesDictionaryOptionsList[this.entriesDictionaryOptionsIndex].next([data]);
      },
      error: error => {
        console.error('error wile correcting invoice :', error);
      }
    });
  }

  //update prices

  updateTotalEntrysPrice(invoiceEntries: InvoiceEntries[]) {
    this.calculator.data = [];
    let vatVal;
    invoiceEntries.forEach((value) => {
      if (value.vat != null) {
        vatVal = value.vat == -100 ? 'zw' : value.vat == -200 ? 'np' : value.vat + "%";

        const calculatorRow = this.calculator.data.filter(c => c.vatRate == vatVal);

        var nettoValue = Number((value.price * value.quantity).toFixed(2));
        var vatValue = Number((Number(value.vat != -100 && value.vat != -200 ? value.vat : 0) / 100 * nettoValue).toFixed(2));
        if (calculatorRow[0] != undefined) {
          calculatorRow[0].nettoValue = Number((calculatorRow[0].nettoValue + nettoValue).toFixed(2))
          calculatorRow[0].vatValue = Number((calculatorRow[0].vatValue + vatValue).toFixed(2));
          calculatorRow[0].bruttoValue = Number((calculatorRow[0].nettoValue + calculatorRow[0].vatValue).toFixed(2));

        }
        else {
          const iec = <invoiceEditCalculator>{ vatRate: vatVal, nettoValue: nettoValue, vatValue: vatValue, bruttoValue: Number((nettoValue + vatValue).toFixed(2)) };
          this.calculator.data.unshift(iec);
        }

      }

      let sumIEC = this.calculator.data.filter(c => c.vatRate == "Suma");
      if (sumIEC[0] != undefined) {
        sumIEC[0].vatValue = Number((sumIEC[0].vatValue + vatValue).toFixed(2));
        sumIEC[0].nettoValue = Number((sumIEC[0].nettoValue + nettoValue).toFixed(2));
        sumIEC[0].bruttoValue = Number((sumIEC[0].bruttoValue + nettoValue + vatValue).toFixed(2));
        if (this.data.invoices[this.paginator.pageIndex].paymentCurrency != "PLN" && this.data.invoices[this.paginator.pageIndex].paymentCurrency != null) {
          let sumPLNIEC = this.calculator.data.filter(c => c.vatRate == "Suma w PLN");
          const rate = this.data.invoices[this.paginator.pageIndex].rate;
          sumPLNIEC[0].vatValue = Number((sumIEC[0].vatValue * rate).toFixed(2));
          sumPLNIEC[0].nettoValue = Number((sumIEC[0].nettoValue * rate).toFixed(2));
          sumPLNIEC[0].bruttoValue = Number((sumIEC[0].bruttoValue * rate).toFixed(2));
        }
      }
      else {
        const newSum = <invoiceEditCalculator>{ vatRate: "Suma", nettoValue: nettoValue, vatValue: vatValue, bruttoValue: Number((nettoValue + vatValue).toFixed(2)) };
        this.calculator.data.push(newSum);
        if (this.data.invoices[this.paginator.pageIndex].paymentCurrency != "PLN" && this.data.invoices[this.paginator.pageIndex].paymentCurrency != null) {
          const rate = this.data.invoices[this.paginator.pageIndex].rate;
          const newPLNSum = <invoiceEditCalculator>{ vatRate: "Suma w PLN", nettoValue: Number((newSum.nettoValue * rate).toFixed(2)), vatValue: Number((newSum.vatValue * rate).toFixed(2)), bruttoValue: Number((Number((newSum.nettoValue + newSum.vatValue).toFixed(2)) * rate).toFixed(2)) };
          this.calculator.data.push(newPLNSum);
        }
      }

      this.calculator._updateChangeSubscription();
    });
  }

  //autocomplete selects

  filterEntrie(value, entrie: FormGroup): InvoiceEntries[] {
    if (value != null) {
      let filterValue = "";
      if (typeof (value) === "object" && value != null) {

        entrie.patchValue({
          name: value.name,
          quantity: 1,
          price: value.price,
          vat: value.vat,
          gtu: value.gtu
        });
        Object.keys(entrie.controls).forEach(e => entrie.controls[e].markAsDirty());
        filterValue = value.name.toLowerCase();
      }
      else {
        filterValue = value.toLowerCase();
      }
      return this.entriesDictionary.filter(option => option.name.toLowerCase().indexOf(filterValue) === 0);
    } else {
      return this.entriesDictionary;
    }
  }

  ChangeEntryFocus(which) {
    if (this.entriesDictionaryOptionsIndex != which) {
      this.entriesDictionaryOptionsIndex = which;
      let dictionary = this.entriesDictionaryOptionsList[which].getValue();
      const focusedEntry = this.invoiceForm.get('invoiceEntries')['controls'][which];
      this.EditedEntryDictionary = {};

      this.FormFocusedEntryChangedChanges$ = focusedEntry.valueChanges.subscribe(e => {
        if (this.ifCanEditEntryToDictionary) {
          Object.entries(e).forEach(([key, value]) => {
            const filterdDictionary = dictionary.filter(ed => ed.name == e.name)[0];
            if (key != "id" && key != "name") {
              if (filterdDictionary.hasOwnProperty(key)) {
                if (filterdDictionary[key] != value) {
                  if (!this.EditedEntryDictionary.hasOwnProperty('id') || this.EditedEntryDictionary['id'] != filterdDictionary.id)
                    this.EditedEntryDictionary['id'] = filterdDictionary.id;
                  this.EditedEntryDictionary[key] = value;
                } else {
                  if (this.EditedEntryDictionary.hasOwnProperty(key)) {
                    delete this.EditedEntryDictionary[key];
                  }
                }
              }
            }
          });
        }
      });

      if (dictionary.filter(a => a.name == focusedEntry['controls']['name'].value).length != 0) {
        this.ifCanEditEntryToDictionary = true;
        this.ifCanAddEntryToDictionary = false;
      } else {
        if (focusedEntry['controls'].name != "" && focusedEntry['controls'].name != undefined) {
          this.ifCanAddEntryToDictionary = true;
        } else {
          this.ifCanAddEntryToDictionary = false;
        }
        if (this.FormFocusedEntryChangedChanges$.closed) {
          this.FormFocusedEntryChangedChanges$.unsubscribe();
        }
        this.EditedEntryDictionary = {};
        this.ifCanEditEntryToDictionary = false;
      }
    }

  }

  filterContractors(value): any[] {
    let filterValue = "";
    if (typeof (value) === "object" && value != null) {
      this.filterdContractor = value;
      filterValue = value.name.toLowerCase();
    } else {
      this.filterdContractor = {};
      filterValue = value.toLowerCase();
    }
    let filterdContracotrs = this.contractors.filter(option => option.name ? option.name.toLowerCase().indexOf(filterValue) === 0 : false);
    return filterdContracotrs;
  }

  contractorAutoChnged(event) {
    const selectedContractor = event;
    this.currentContarctor = selectedContractor;
    this.invoiceForm.controls.invoiceContractor.patchValue({
      name: selectedContractor.name,
      adress: new Adress(selectedContractor.street, selectedContractor.houseNumber, selectedContractor.apartamentNumber, selectedContractor.city),
      codeCoutry: new CodeCoutry(selectedContractor.postalcode, selectedContractor.postoffice, selectedContractor.country),
      nip: selectedContractor.nip
    });
    Object.keys(this.invoiceForm.controls.invoiceContractor['controls']).forEach(k => this.invoiceForm.controls.invoiceContractor['controls'][k].markAsDirty());
  }

  filterPaymentBank(value: string): PaymentMethod[] {
    let filterValue = '';
    if (value != null) {
      filterValue = value;
    }
    else {
      return this.companyPaymentMethods;
    }
    return this.companyPaymentMethods.filter(option => option.name ? option.name.toLowerCase().indexOf(filterValue.toLowerCase()) === 0 : false);
  }

  paymentBankAutoChnged(event) {
    const selectedCompanyPaymentMethods = this.companyPaymentMethods.filter(option => option.name.toLowerCase() === event.toLowerCase());
    this.invoiceForm.patchValue({
      paymentAccountNumber: selectedCompanyPaymentMethods[0].accountNumber,
      paymentCurrency: selectedCompanyPaymentMethods[0].currency
    });
    this.invoiceForm.controls.paymentAccountNumber.markAsDirty();
    this.invoiceForm.controls.paymentCurrency.markAsDirty();
  }

  focusOutContractorSelect() {
    setTimeout(() => {
      if (this.currentContarctor['name'] != this.invoiceForm.controls.invoiceContractor['controls'].contractor.value.name) {
        this.invoiceForm.controls.invoiceContractor.patchValue({
          contractor: this.currentContarctor
        });
      }
    }, 150)

  }

  filterSellDateNames(value): any[] {
    if (value == null) return this.sellDateName;
    let filterValue = "";
    if (typeof (value) === "object" && value != null)
      filterValue = value.name.toLowerCase();
    else
      filterValue = value.toLowerCase();

    return this.sellDateName.filter(option => option.name.toLowerCase().indexOf(filterValue) === 0);
  }

  filterPaymentCurrency(value: string): any {
    return this.currencyCodes.codes().filter(option => option.toLowerCase().indexOf(value.toLowerCase()) === 0);
  }

  displayAutocomplet(displayValue: any): string {
    return typeof displayValue == 'object' && displayValue != null ? displayValue.name : displayValue;
  }
  displayPaymentCurrencyAutocomplet(displayValue: string): string {
    return displayValue+ ' | ' + getCurrencySymbol(displayValue, 'narrow');
  }

  displayVatAutocomplet(displayValue: any): string {
    return displayValue == -100 ? 'zw' : displayValue == -200 ? 'np' : displayValue + '%';
  }

  //helpers

  updateForm(currentPayment: object, form: any) {
    Object.keys(form.controls).forEach(key => {
      const currentControl = form.controls[key];

      if (currentControl.controls && !isArray(currentControl.controls)) {
        this.updateForm(currentPayment[key], currentControl);
      }
      else {
        if (!currentControl.dirty) {
          if (key == 'id') {
            currentControl.patchValue(currentPayment[key]);
          } else if (key == 'adress') {
            currentControl.patchValue(new Adress(currentPayment['street'], currentPayment['houseNumber'], currentPayment['apartamentNumber'], currentPayment['city']));
          }
          else if (key == 'codeCoutry') {
            currentControl.patchValue(new CodeCoutry(currentPayment['postalcode'], currentPayment['postoffice'], currentPayment['country']));
          }
          else {
            currentControl.patchValue(currentPayment[key]);
          }
        }
      }
    });
  }

  updateFormInvoiceNumbers(pageIndex: number) {
    this.invoiceForm.controls.number.patchValue(this.invoiceNumbersChanges[pageIndex]);
  }

  getInvoiceNumbersChanges() {
    const formNumber = this.invoiceForm.controls.number;
    if (formNumber.dirty) {
      this.invoiceNumbersChanges[this.page] = formNumber.value;
    }
  }

  updateFormInvoiceEntries(pageIndex: number) {
    for (let i = 0; i < this.invoiceEntriesChanges[this.page].length; i++) {
      this.removeEntry(0);
    }

    Object.entries(this.dataSource.data[pageIndex].invoiceEntries).forEach(([_, v], index) => {
      let changedInvoiceEntrie = v;
      Object.entries(this.invoiceEntriesChanges[pageIndex][index]).forEach(([k, v]) => {
        changedInvoiceEntrie[k] = v;
      });
      this.addEntry(index, v, pageIndex);
    });
  }

  getInvoiceEntriesChanges() {
    const entriesChanges = this.invoiceEntriesChanges[this.page];
    const entriesForm = this.invoiceForm.controls.invoiceEntries['controls'];

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
          if (entriesChanges[arraykey] && !entriesChanges[arraykey].hasOwnProperty("id")) {
            entriesChanges[arraykey]["id"] = currentEntrie.controls.id.value;
          }
        }
      }
    });
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
            if (!isArray(dirtyValues)) {
              dirtyValues[key] = this.getDirtyValues(currentControl);
            } else if (Number(key) > this.invoiceEntriesChanges[this.page].length - 1) {
              dirtyValues[key] = this.getDirtyValues(currentControl);
            }
          }
          else {
            dirtyValues[key] = currentControl.value;
          }
        }
      });
    if (form.dirty && form.controls.hasOwnProperty("id")) {
      dirtyValues["id"] = form.controls.id.value;
    }
    return dirtyValues;
  }

  getIvoiceChnagesToSend(pageIndex: number) {
    let changes = this.getDirtyValues(this.invoiceForm);
    if (changes["invoiceEntries"]) {
      Object.entries(this.invoiceEntriesChanges[pageIndex]).forEach(([k, v]) => {
        changes["invoiceEntries"][k] = v;
      });
      let counter = 0;
      Object.entries(changes["invoiceEntries"]).forEach(([k, v]) => {
        if (Object.keys(v).length === 0) {
          changes["invoiceEntries"][k] = null;
          counter++;
        }
      });
      if (counter == changes["invoiceEntries"].length) {
        delete changes["invoiceEntries"];
      } else {
        let changedBruttoWorth = 0, changedNettoWorth = 0;
        changes["invoiceEntries"].forEach((ie, index) => {
          const ieValue = ie ? ie : this.dataSource.data[pageIndex].invoiceEntries[index];
          if (Object.keys(ieValue).length != 1) {
            const nettoValue = Number((ieValue.price * ieValue.quantity).toFixed(2));
            const vatValue = Number((Number(ieValue.vat != -100 && ieValue.vat != -200 ? ieValue.vat : 0) / 100 * nettoValue).toFixed(2));
            changedNettoWorth = Number((changedNettoWorth + nettoValue).toFixed(2));
            changedBruttoWorth = Number((changedBruttoWorth + nettoValue + vatValue).toFixed(2));
          }
        });
        changes['bruttoWorth'] = changedBruttoWorth;
        changes['nettoWorth'] = changedNettoWorth;
      }
    }
    const invoiceContractor = changes['invoiceContractor'];
    if (invoiceContractor) {
      if (invoiceContractor['adress']) {
        Object.entries(invoiceContractor['adress']).forEach(([k, v]) => {
          invoiceContractor[k] = v;
        });
      }
      if (invoiceContractor['codeCoutry']) {
        Object.entries(invoiceContractor['codeCoutry']).forEach(([k, v]) => {
          invoiceContractor[k] = v;
        });
      }
      if (invoiceContractor['contractor']) {
        invoiceContractor['contractor'] = { id: invoiceContractor['contractor']['id'] };
      }
    }
    if (changes['number']) {
      changes['number'] = this.invoiceNumbersChanges[pageIndex];
    }
    if (changes['paymentStatus']) {
      delete changes['paymentStatus'];
    }
    let changedSellDateName = changes['sellDateName'];
    if (changedSellDateName) {
      if (typeof changedSellDateName === 'string') {
        changedSellDateName = { id: 0, name: changedSellDateName };
      } else {
        delete changedSellDateName.name;
      }
    }

    return changes;
  }

  getNewPage(pageIndex: number) {
    this.updateForm(this.dataSource.data[pageIndex], this.invoiceForm);
    this.getInvoiceNumbersChanges();
    this.getInvoiceEntriesChanges();
    this.updateFormInvoiceEntries(pageIndex);
    this.updateFormInvoiceNumbers(pageIndex);
    this.page = pageIndex;
  }

  HttpEndDialog(errors, infoText) {
    this.isChanges = false;
    this.invoiceForm.markAsPristine();
    this.dialog.open(AskIfDialog, {
      disableClose: true,
      data: { data: infoText, close: "CLOSE", ok: null }
    });
  }

  async ChangeAdressDataTOSend(invoice: Invoice): Promise<Invoice> {
    if (invoice.invoiceContractor != null) {
      //invoice.invoiceContractor['id'] = 0;
      if (this.currentContarctor['name'] == "") {
        invoice = await this.openContractorDialog(invoice, true);
        return invoice;
      } else if (invoice.invoiceContractor.name && invoice.invoiceContractor.name != this.currentContarctor['name']) {
        invoice = await this.openContractorDialog(invoice, false);
        return invoice;
      }
    }
    return invoice;
  }

  async openContractorDialog(invoice: Invoice, isNew: boolean): Promise<Invoice> {
    const dialogRefUpdateInfo = this.dialog.open(CustomFormDialog, {
      disableClose: true,
      width: '1250px',
      minWidth: '1250px',
      data: {
        description: !isNew ? "INVOICES.INVOICE_EDIT.AS_IF_ADD_NEW_CONTRACTOR" : "INVOICES.INVOICE_EDIT.ADD_NEW_CONTRACTOR",
        close: !isNew ? "NO" : null,
        ok: !isNew ? "YES" : "ADD",
        form: [
          { description: "EMAIL", name: "email", value: "", regEx: "^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$" } as FormPart,
          { description: "NAME", name: "name", value: invoice.invoiceContractor.name, regEx: "" } as FormPart,
          { description: "ADRESS", name: "adress", value: new Adress(invoice.invoiceContractor.street != null ? invoice.invoiceContractor.street : "", invoice.invoiceContractor.houseNumber != null ? invoice.invoiceContractor.houseNumber : "", invoice.invoiceContractor.apartamentNumber != null ? invoice.invoiceContractor.apartamentNumber : "", invoice.invoiceContractor.city != null ? invoice.invoiceContractor.city : ""), regEx: "" } as FormPart,
          { description: "CODECOUNTRY", name: "codeCoutry", value: new CodeCoutry(invoice.invoiceContractor.postalcode != null ? invoice.invoiceContractor.postalcode : "", invoice.invoiceContractor.postoffice != null ? invoice.invoiceContractor.postoffice : "", invoice.invoiceContractor.country != null ? invoice.invoiceContractor.country : ""), regEx: "" } as FormPart,
          { description: "NIP", name: "nip", value: invoice.invoiceContractor.nip, regEx: "^[0-9]*$" } as FormPart,
          { description: "MAIL_LANGUAGE", name: "mailLanguage", value: [null, MailLanguage], regEx: "" } as FormPart,
          { description: "Konto WN", name: "wnAccount", value: "", regEx: "" } as FormPart
        ]
      }
    });

    return dialogRefUpdateInfo.afterClosed().toPromise().then(nc => {
      if (nc != null && this.contractors.filter(c => c.name == nc.name).length == 0) {
        const newContractor = {
          name: nc.name,
          postoffice: nc.codeCoutry.postoffice,
          street: nc.adress.street,
          houseNumber: nc.adress.houseNumber,
          apartamentNumber: nc.adress.apartamentNumber,
          postalcode: nc.codeCoutry.postalcode,
          city: nc.adress.city,
          nip: nc.nip,
          mailLanguage: nc.mailLanguage,
          wnAccount: nc.wnAccount,
          id: 0,
          oldId: 0,
          email: nc.email,
        };

        return new Promise((resolve, reject) => {
          this.http.post<InvoiceContractor>(`${this._envUrl}/api/contractor/` + this.currentCompanyId, newContractor)
            .toPromise()
            .then(
              contractor => {
                invoice.invoiceContractor['contractor'] = { id: contractor.id, name: contractor.name };
                delete invoice.invoiceContractor['contractor'].name;
                this.currentContarctor = contractor;
                this.contractors.push(contractor);
                this.invoiceForm.controls.invoiceContractor.patchValue({
                  contractor: contractor.name
                });
                resolve(invoice);
              },
              msg => {
                reject(msg);
              }
            );
        });

      } else {
        return Promise.resolve(invoice);
      }
    });
  }

  GenerateNewInvoiceNumbers() {
    let today = new Date();
    let montAndYear = '/' + today.getMonth() + '/' + today.getFullYear();
    this.http.get(`${this._envUrl}/api/invoice/` + this.currentCompanyId + '/lastindex').toPromise().then(index => {
      if (typeof (index) == 'number') {
        let newIndex = index + 1;
        Object.keys(this.invoiceNumbersChanges).forEach((k, index) => { this.invoiceNumbersChanges[k] = (newIndex + index) + montAndYear });
        const formNumber = this.invoiceForm.controls.number;
        formNumber.patchValue(this.invoiceNumbersChanges[this.paginator.pageIndex]);
        formNumber.markAsDirty();
        this.isChanges = true;
      }
    });
  }


  //dialog actions 

  async InsertInvoices(asNew = false) {
    this.getInvoiceEntriesChanges();

    let countRespons = 0;
    let isAllInsert = [];
    const currentPage = this.page;

    for (let index = 0; index < this.paginator.length; index++) {
      this.getNewPage(index);

      let invoiceToSend = new Invoice(this.invoiceForm.value);
      invoiceToSend = await this.ChangeAdressDataTOSend(invoiceToSend);
      invoiceToSend.id = 0;
      invoiceToSend.invoiceContractor.id = this.dataSource.data[index].invoiceContractor.id;
      invoiceToSend.invoiceEntries.forEach(ie => ie.id = 0);

      if (this.data.action == 1) {
        invoiceToSend.correcting = this.dataSource.data[index].number;
      }

      if ((typeof invoiceToSend.sellDateName) === 'string') {
        invoiceToSend.sellDateName = { id: 0, name: String(invoiceToSend.sellDateName) };
      } else {
        delete invoiceToSend.sellDateName.name;
      }

      let changedBruttoWorth = 0, changedNettoWorth = 0;
      invoiceToSend.invoiceEntries.forEach(ie => {
        const nettoValue = Number((ie.price * ie.quantity).toFixed(2));
        const vatValue = Number((Number(ie.vat != -100 && ie.vat != -200 ? ie.vat : 0) / 100 * nettoValue).toFixed(2));
        changedNettoWorth = Number((changedNettoWorth + nettoValue).toFixed(2));
        changedBruttoWorth = Number((changedBruttoWorth + nettoValue + vatValue).toFixed(2));
      });
      invoiceToSend.bruttoWorth = changedBruttoWorth;
      invoiceToSend.nettoWorth = changedNettoWorth;


      this.http.post<Invoice>(`${this._envUrl}/api/invoice/` + this.currentCompanyId, invoiceToSend).subscribe(
        res => {
          if (asNew) this.data[countRespons] = res;
          this.dataSource.data[countRespons] = res;

          if (++countRespons >= this.dataSource.data.length) {
            this.HttpEndDialog(isAllInsert, countRespons > 1 ? 'INVOICES.INVOICE_EDIT.EDIT_SUCCESS_MENY' : 'INVOICES.INVOICE_EDIT.EDIT_SUCCESS');
          }
        },
        error => {
          isAllInsert.push(invoiceToSend.number);
          if (++countRespons >= this.dataSource.data.length) {
            this.HttpEndDialog(isAllInsert, countRespons > 1 ? 'INVOICES.INVOICE_EDIT.ADD_ERROR_MENY' : 'INVOICES.INVOICE_EDIT.ADD_ERROR');
          }
        }
      );
    }
    this.getNewPage(currentPage);
  }

  async UpdateInvoices() {
    this.getInvoiceEntriesChanges();

    let countRespons = 0;
    let isAllUpdated = [];
    const currentPage = this.page;


    for (let index = 0; index < this.paginator.length; index++) {
      let invoiceToSend = new Invoice(this.getIvoiceChnagesToSend(index));
      invoiceToSend = await this.ChangeAdressDataTOSend(invoiceToSend);
      if (invoiceToSend.invoiceContractor) {
        invoiceToSend.invoiceContractor.id = this.dataSource.data[index].invoiceContractor.id;
      }
      delete invoiceToSend.id;
      this.http.put<Invoice>(`${this._envUrl}/api/invoice/` + this.dataSource.data[index].id, invoiceToSend).subscribe(
        res => {
          var index = this.dataSource.data.map(function (e) { return e.id; }).indexOf(res.id);
          this.data.invoices[index] = res;
          this.dataSource.data[index] = res;
          if (++countRespons >= this.dataSource.data.length) {
            this.HttpEndDialog(isAllUpdated, countRespons > 1 ? 'INVOICES.INVOICE_EDIT.EDIT_SUCCESS_MENY' : 'INVOICES.INVOICE_EDIT.EDIT_SUCCESS');
          }
        },
        error => {
          isAllUpdated.push(invoiceToSend.number);
          if (++countRespons >= this.dataSource.data.length) {
            this.HttpEndDialog(isAllUpdated, countRespons > 1 ? 'INVOICES.INVOICE_EDIT.EDIT_ERROR_MENY' : 'INVOICES.INVOICE_EDIT.EDIT_ERROR');
          }
        }
      );
    }
    this.getNewPage(currentPage);

  }

}
/*  GetValuesInObjectDeep(property, fromObject): any {
    if (fromObject.hasOwnProperty(property)) {
      return fromObject[property];
    } else {
      Object.entries(fromObject).forEach(([key, value]) => {
        if (typeof (value) != 'object' || value != null) {
          const returnValue = this.GetValuesInObjectDeep(property, value);
          if (returnValue != null)
            return returnValue;
        }
      });
    }
    return null;
  }*/
