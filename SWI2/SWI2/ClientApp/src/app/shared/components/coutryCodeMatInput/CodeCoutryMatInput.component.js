/*import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { InvoiceHeader } from '../../interfaces/InvoiceHeader.model';
import { Validators, FormBuilder, FormGroup, FormControl, FormArray } from '@angular/forms';
import { CurrencyPipe } from '@angular/common';
import { HttpClient, HttpHeaders, HttpResponse, HttpErrorResponse } from '@angular/common/http';
import { first } from 'rxjs/operators';
import { MatTableDataSource } from '@angular/material/table';
import { invoiceEditCalculator } from '../../interfaces/invoiceEditCalculator';

@Component({
  selector: 'swi-invoiceEdit',
  templateUrl: './invoiceEdit.component.html',
  styleUrls: ['./invoiceEdit.component.css']
})

export class InvoiceEditComponent {
  name = 'Angular 10 reactive form with dynamic fields and validations example';
  invoiceForm: FormGroup;
  totalSum: number = 0;
  myFormValueChanges$;
  displayedCalculatorColumns: string[] = ['vatRate', 'nettoValue', 'vatValue', 'bruttoValue'];
  calculator = new MatTableDataSource<invoiceEditCalculator>();

  constructor(
    private formBuilder: FormBuilder,
    private http: HttpClient,
    public dialogRef: MatDialogRef<InvoiceEditComponent>,
    @Inject(MAT_DIALOG_DATA) public data: InvoiceHeader[]
  ) { }

  ngOnInit() {
    this.invoiceForm = this.formBuilder.group({
      number: [''],
      creationPlace: [''],
      invoiceContractor: this.formBuilder.group({
        name: [''],
        street: [''],
        housenumber: [''],
        apartamentNumber: [''],
        postalcode: [''],
        city: [''],
        nip: ['']
      }),
      invoiceIssuer: [''],
      paymentAccountNumber: [''],
      paymentBank: [''],
      note: [''],
      sellDateName: [''],
      sellDate: [''],
      paymentDate: [''],
      invoiceEntries: this.formBuilder.array([
        this.getEntry()
        ])
    });

    this.myFormValueChanges$ = this.invoiceForm.controls['invoiceEntries'].valueChanges;
    this.myFormValueChanges$.subscribe(invoiceEntries => this.updateTotalEntrysPrice(invoiceEntries));

  }

  ngOnDestroy(): void {
    this.myFormValueChanges$.unsubscribe();
  }
  save() {
  }

  private getEntry() {
    const numberPatern = '^[0-9.,]+$';
    return this.formBuilder.group({
      name: ['', Validators.required],
      quantity: [1, [Validators.required, Validators.pattern(numberPatern)]],
      netto: ['', [Validators.required, Validators.pattern(numberPatern)]],
      vat: ['', [Validators.required, Validators.pattern(numberPatern)]],
      gtu: [{ value: '', disabled: true }]
    });
  }
  //invoiceEntries
  addEntry() {
    const control = <FormArray>this.invoiceForm.controls['invoiceEntries'];
    control.push(this.getEntry());
  }

  removeEntry(i: number) {
    const control = <FormArray>this.invoiceForm.controls['invoiceEntries'];
    control.removeAt(i);
  }

  clearAllEntry() {
    const control = <FormArray>this.invoiceForm.controls['invoiceEntries'];
    while (control.length) {
      control.removeAt(control.length - 1);
    }
    control.clearValidators();
    control.push(this.getEntry());
  }
  //update prices
  private updateTotalEntrysPrice(invoiceEntries: any) {
    const control = <FormArray>this.invoiceForm.controls['invoiceEntries'];
    this.totalSum = 0;
    for (let i in invoiceEntries) {

    }
  }
}
*/
//# sourceMappingURL=invoiceEdit.component.js.map
