import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, Inject, Renderer2, ViewChild } from '@angular/core';
import { MatDialog, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Invoice, InvoiceEntries, InvoiceSendeds } from '../../models/invoices/Invoice.model';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { count, debounceTime, distinctUntilChanged, startWith } from 'rxjs/operators';
import { invoiceEditCalculator } from '../../models/invoices/invoiceEditCalculator';
import html2canvas from 'html2canvas';
import JSPdf from 'jspdf';
import * as JSZip from 'jszip';
import { saveAs } from 'file-saver';
import { ApiService } from '../../shared/services/api.service';
import { AskIfDialog } from '../../shared/components/askIfDialog/askIfDialog.component';
import { HttpHeaders } from '@angular/common/http';

@Component({
  selector: 'swi-invoicePdf',
  templateUrl: './invoicePdf.component.html',
  styleUrls: ['./invoicePdf.component.css']
})

export class InvoicePdf {
  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild('Invoice', { static: false }) invoice: ElementRef;
  isLoadingResults = false;

  dataSource = new MatTableDataSource<Invoice>();
  displayedCalculatorColumns: string[] = ['vatRate', 'nettoValue', 'vatValue', 'bruttoValue'];
  calculator = new MatTableDataSource<invoiceEditCalculator>();
  page = 0;
  invoicePage = 1;
  invoicePages = 1;
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: Invoice[],
    private http: ApiService,
    private renderer: Renderer2,
    public dialog: MatDialog, private cdRef: ChangeDetectorRef) {
    this.dataSource.data = this.data;
  };
  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.paginator.page
      .pipe(
        debounceTime(50),
        distinctUntilChanged())
      .subscribe(index => {
        this.page = index.pageIndex;
        this.updateTotalEntrysPrice(this.data[this.page].invoiceEntries);
      });
    this.dataSource.data.forEach(i => {
      if (i.sellDateName == null)
        i.sellDateName = { id: 0, name: "brak" }
    })

    this.updateTotalEntrysPrice(this.data[this.page].invoiceEntries);

    setTimeout(() => {
      this.invoicePages = Math.round(this.invoice.nativeElement.scrollWidth / this.invoice.nativeElement.clientWidth);
    }, 100)

  }

  DownloadInvoicesInOneFile() {
    const margin = 10;
    let counter = 0;
    this.isLoadingResults = true;
    let pdf = new JSPdf('p', 'pt', 'a4');
    const initialPage = this.page;

    for (let p = 0; p < this.data.length; p++) {
      this.page = p;
      this.cdRef.detectChanges();
      for (let pc = 0; pc < this.invoicePages; pc++) {
        this.TurnPage(pc + 1);
        html2canvas(this.invoice.nativeElement, { allowTaint: true }).then(canvas => {
          canvas.getContext('2d');
          let imgData = canvas.toDataURL("image/jpeg", 1.0);
          pdf.addPage('a4', 'p');
          pdf.addImage(imgData, 'JPG', margin, margin, pdf.internal.pageSize.getWidth() - margin * 2, pdf.internal.pageSize.getHeight() - margin * 4);
          pdf.text(pc.toString(), pdf.internal.pageSize.getWidth() / 2, pdf.internal.pageSize.getHeight() * 0.98);

          if (pc + 1 == this.invoicePages) {

            if (++counter == this.data.length) {
              pdf.deletePage(1);
              pdf.save("zestawienieFaktur.pdf");
              this.TurnPage(1);
              this.page = initialPage;
              this.isLoadingResults = false;
            }
          }
        });
      }
    }
  }

  DownloadInvoicesInZip() {
    var zip = new JSZip();
    const margin = 10;
    let counter = 0;
    this.isLoadingResults = true;
    const initialPage = this.page;

    for (let p = 0; p < this.data.length; p++) {
      let pdf = new JSPdf('p', 'pt', 'a4');
      this.page = p;
      this.cdRef.detectChanges();
      for (let pc = 0; pc < this.invoicePages; pc++) {
        this.TurnPage(pc + 1);
        html2canvas(this.invoice.nativeElement, { allowTaint: true }).then(canvas => {
          canvas.getContext('2d');
          let imgData = canvas.toDataURL("image/jpeg", 1.0);
          pdf.addPage('a4', 'p');
          pdf.addImage(imgData, 'JPG', margin, margin, pdf.internal.pageSize.getWidth() - margin * 2, pdf.internal.pageSize.getHeight() - margin * 4);
          pdf.text(pc.toString(), pdf.internal.pageSize.getWidth() / 2, pdf.internal.pageSize.getHeight() * 0.98);

          if (pc + 1 == this.invoicePages) {
            pdf.deletePage(1);
            try {
              let re = /\//gi;
              zip.file(this.data[p].number.replace(re, "_",) + '.pdf', pdf.output('blob'));
            }
            catch {
              console.error('Something went wrong adding pdf to zip :' + this.data[p].number);
            }

            if (++counter == this.data.length) {
              zip.generateAsync({ type: 'blob' }).then(function (content) {
                saveAs(content, 'invoices.zip');
              });
              this.TurnPage(1);
              this.page = initialPage;
              this.isLoadingResults = false;
            }
          }
        });
      }
    }
  }

  SendInvoices() {
    const margin = 10;
    let counter = 0;
    this.isLoadingResults = true;
    let pdf = new JSPdf('p', 'pt', 'a4');
    const initialPage = this.page;
    let errors = [];
    let noContractor = [];

    for (let p = 0; p < this.data.length; p++) {
      let pdf = new JSPdf('p', 'pt', 'a4');
      this.page = p;
      if (this.data[p].invoiceContractor.contractor != null) {
        this.cdRef.detectChanges();

        for (let pc = 0; pc < this.invoicePages; pc++) {
          this.TurnPage(pc + 1);
          html2canvas(this.invoice.nativeElement, { allowTaint: true }).then(canvas => {
            canvas.getContext('2d');
            let imgData = canvas.toDataURL("image/jpeg", 1.0);
            pdf.addPage('a4', 'p');
            pdf.addImage(imgData, 'JPG', margin, margin, pdf.internal.pageSize.getWidth() - margin * 2, pdf.internal.pageSize.getHeight() - margin * 4);
            pdf.text(pc.toString(), pdf.internal.pageSize.getWidth() / 2, pdf.internal.pageSize.getHeight() * 0.98);

            if (pc + 1 == this.invoicePages) {
              pdf.deletePage(1);
              this.http.post<boolean>('api/invoice/' + this.data[p].id + '/email', JSON.stringify({ pdf: btoa(pdf.output()) }), null, new HttpHeaders({ 'Content-Type': 'application/json; charset=utf-8' })).subscribe(isr => {
                if (!isr) errors.push(this.data[p].number);
                if (++counter == this.data.length) {
                  this.TurnPage(1);
                  this.page = initialPage;
                  this.isLoadingResults = false;
                  this.dialog.open(AskIfDialog, {
                    disableClose: true,
                    data: { data: errors.length == 0 ? "Faktury wysłono pomyślnie" : "Wystąpił błąd przy wysyłaniu faktur :" + errors.toString() + " </br>" + (noContractor.length > 0 ? "Faaktury nie posiadające przypisanych konttrachentów" + noContractor.toString() : ""), close: "Zamknij", ok: null }
                  });
                }
              });
            }
          });
        }
      }else {
        noContractor.push(this.data[p].number);

        if (++counter == this.data.length) {
          this.dialog.open(AskIfDialog, {
            disableClose: true,
            data: { data: errors.length == 0 ? "Faktury wysłono pomyślnie" : "Wystąpił błąd przy wysyłaniu faktur :" + errors.toString() + " </br>" + (noContractor.length > 0 ? "Faaktury nie posiadające przypisanych konttrachentów" + noContractor.toString() : ""), close: "Zamknij", ok: null }
          });
        }
      }
    }
  }

  TurnPage(wichPage: number) {
    this.invoicePage = wichPage;
    const scrollChange = wichPage == 1 || wichPage == this.invoicePages ? (this.invoice.nativeElement.scrollWidth / this.invoicePages * (wichPage - 1)) + 1 : (this.invoice.nativeElement.scrollWidth / this.invoicePages * (wichPage - 1));
    this.invoice.nativeElement.scrollLeft = scrollChange;
  }

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
          calculatorRow[0].bruttoValue = calculatorRow[0].nettoValue + calculatorRow[0].vatValue;

        }
        else {
          const iec = <invoiceEditCalculator>{ vatRate: vatVal, nettoValue: nettoValue, vatValue: vatValue, bruttoValue: nettoValue + vatValue };
          this.calculator.data.unshift(iec);
        }
      }

      let sumIEC = this.calculator.data.filter(c => c.vatRate == "Suma");
      if (sumIEC[0] != undefined) {
        sumIEC[0].vatValue = Number((sumIEC[0].vatValue + vatValue).toFixed(2));
        sumIEC[0].nettoValue = Number((sumIEC[0].nettoValue + nettoValue).toFixed(2));
        sumIEC[0].bruttoValue = Number((sumIEC[0].bruttoValue + nettoValue + vatValue).toFixed(2));
        if (this.data[this.page].paymentCurrency != "PLN" && this.data[this.paginator.pageIndex].paymentCurrency != null) {
          let sumPLNIEC = this.calculator.data.filter(c => c.vatRate == "Suma w PLN");
          const rate = this.data[this.paginator.pageIndex].rate;
          sumPLNIEC[0].vatValue = Number((sumIEC[0].vatValue * rate).toFixed(2));
          sumPLNIEC[0].nettoValue = Number((sumIEC[0].nettoValue * rate).toFixed(2));
          sumPLNIEC[0].bruttoValue = Number((sumIEC[0].bruttoValue * rate).toFixed(2));
        }
      }
      else {
        const newSum = <invoiceEditCalculator>{ vatRate: "Suma", nettoValue: nettoValue, vatValue: vatValue, bruttoValue: Number((nettoValue + vatValue).toFixed(2)) };
        this.calculator.data.push(newSum);

        if (this.data[this.page].paymentCurrency != "PLN") {
          const rate = this.data[this.paginator.pageIndex].rate;
          const newPLNSum = <invoiceEditCalculator>{ vatRate: "Suma w PLN", nettoValue: Number((newSum.nettoValue * rate).toFixed(2)), vatValue: Number((newSum.vatValue * rate).toFixed(2)), bruttoValue: Number((Number((newSum.nettoValue + newSum.vatValue).toFixed(2)) * rate).toFixed(2)) };
          this.calculator.data.push(newPLNSum);
        }
      }
    });
    this.calculator._updateChangeSubscription();
  }
}
