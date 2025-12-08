import { CollectionViewer, DataSource, SelectionModel } from '@angular/cdk/collections';
import { AfterViewInit, Component, ElementRef, ViewChild } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { BehaviorSubject, fromEvent, merge, Observable, of } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, finalize, map, startWith, switchMap, tap } from 'rxjs/operators';
import { HttpParams } from "@angular/common/http";
import { FormControl, FormGroup, FormsModule } from '@angular/forms';
import { ActivatedRoute, Data } from '@angular/router';
import { DatePipe, DecimalPipe } from '@angular/common';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { InvoiceEditComponent } from '../invoiceEdit/invoiceEdit.component';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { ApiService } from '../../shared/services/api.service';
import { InvoiceCalculator } from '../../models/invoices/InvoiceCalculator.model';
import { Invoice, InvoiceSendeds, InvoiceTable } from '../../models/invoices/Invoice.model';
import { InvoicePdf } from '../invoicePdf/invoicePdf.component';
import { saveAs } from 'file-saver';
import { InvoiceSend } from '../InvoiceSend/invoiceSend.component';
import { Company } from '../../company/company.model';
import { isMoment } from 'moment';
import * as moment from 'moment';
import { AskIfDialog } from '../../shared/components/askIfDialog/askIfDialog.component';
import { environment } from '../../../environments/environment';
import { InvoiceMailTemplateComponent } from '../invoice-mail-template/invoice-mail-template.component';



@Component({
  selector: 'swi-invoiceTable',
  templateUrl: './invoiceTable.component.html',
  styleUrls: ['./invoiceTable.component.css']
})

export class invoiceTableComponent implements AfterViewInit {
  invoicesTotalCount: number;
  isLoadingResults = true;
  _envUrl: string = environment.urlAddress;
  currentCompanyId:number;
  displayedCalculatorColumns: string[] = ['paymentCurrency', 'nettoWorth', 'bruttoWorth', 'vat', 'paymentValue', 'paymentStatus'];
  calculator = new MatTableDataSource<InvoiceCalculator>();
  displayedInvoiceColumns: string[] = ['select', 'Number', 'NettoWorth', 'BruttoWorth', 'Correcting', 'PaymentStatus', 'PaymentCurrency', 'Created'];
  dataSource = new MatTableDataSource<Invoice>();
  @ViewChild(MatSort) sort: MatSort;
  @ViewChild(MatPaginator) paginator: MatPaginator;

  selection = new SelectionModel<Invoice>(true, []);
  filter: FormGroup;
  numericFilter: FormGroup;
  dateFilter: FormGroup;

  constructor(private _httpClient: ApiService,
    public dialog: MatDialog,
    private _route: ActivatedRoute,
    public _authService: AuthenticationService) {
    this.numericFilter = new FormGroup({
      NettoWorthFrom: new FormControl(),
      BruttoWorthFrom: new FormControl(),
      NettoWorthTo: new FormControl(),
      BruttoWorthTo: new FormControl()
    });
    this.dateFilter = new FormGroup({
      CreatedStart: new FormControl(),
      CreatedEnd: new FormControl()
    });
    this.filter = new FormGroup({
      Number: new FormControl(),
      Correcting: new FormControl(),
      PaymentStatus: new FormControl(),
      PaymentCurrency: new FormControl()
    });
    this._authService.currentSelectedCompany.subscribe(company => {
      if (this._route.snapshot.params.id)
        this.currentCompanyId = this._route.snapshot.params.id;
      else {
        this.currentCompanyId = company['id'];
      }
    });

  }

  ngAfterViewInit() {
    //this.invoiceDataSource = new InvoiceDataSource(this._httpClient);
    this.selection.changed.subscribe(s => {
      s.added.forEach(a => { this.calculatorChangeRow(a, true); });
      s.removed.forEach(r => { this.calculatorChangeRow(r, false); });
    });
    merge(this.sort.sortChange, this.paginator.page, this.numericFilter.valueChanges, this.filter.valueChanges, this.dateFilter.valueChanges)
      .pipe(
        debounceTime(600),
        distinctUntilChanged(),
        startWith({}),
        switchMap(() => {
          this.isLoadingResults = true;
          return this.loadInvoices(
            this.createFilter(this.filter),
            this.createFilter(this.numericFilter),
            this.sort.active + " " + this.sort.direction,
            this.dateFilter.get("CreatedStart").value,
            this.dateFilter.get("CreatedEnd").value,
            this.paginator.pageIndex,
            this.paginator.pageSize
          );
        }),
        map(data => {
          this.isLoadingResults = false;
          this.invoicesTotalCount = data.totalCount;
          //this.paginator.pageIndex = 0;
          this.selection.clear();
          return data.invoice;
        }),
        catchError((err) => {
          this.isLoadingResults = false;
          return of([err]);
        })
      ).subscribe(data => {
        data.forEach(d => {
          d.created = new Date(d.created);
          d.sellDate = new Date(d.sellDate);
          d.paymentDate = new Date(d.paymentDate);
        });
        this.dataSource.data = data;
      });
  }
  createFilter(filter: FormGroup): string {
    var filterRes = "";
    Object.keys(filter.controls).forEach((key: string) => {
      if (filter.get(key).value && !(filter.get(key).value instanceof Date))
        filterRes += key + "!=!" + filter.get(key).value + "|";
    })
    filterRes = filterRes.slice(0, -1)
    return filterRes;
  }
  //dialog
  openEdit(action: number, row?): void {
    const dialogRef = this.dialog.open(InvoiceEditComponent, {
      disableClose: true,
      maxHeight: '96vh',
      height: 'auto',
      width: '90vw',
      data: { invoices: (row != null ? [row] : this.selection.selected), action: action }
    });

    dialogRef.afterClosed().subscribe(result => {
      this.isLoadingResults = true;

      this.loadInvoices(
        this.createFilter(this.filter),
        this.createFilter(this.numericFilter),
        this.sort.active + " " + this.sort.direction,
        this.dateFilter.get("CreatedStart").value,
        this.dateFilter.get("CreatedEnd").value,
        this.paginator.pageIndex,
        this.paginator.pageSize
      ).pipe(
        map(data => {
          this.isLoadingResults = false;
          this.invoicesTotalCount = data.totalCount;
          this.selection.clear();
          return data.invoice;
        })
      ).subscribe(data => {
        data.forEach(d => {
          d.created = new Date(d.created);
          d.sellDate = new Date(d.sellDate);
          d.paymentDate = new Date(d.paymentDate);
        });
        this.dataSource.data = data;
      });
    });
  }

  openAdd(): void {
    const dialogRef = this.dialog.open(InvoiceEditComponent, {
      disableClose: true,
      maxHeight: '96vh',
      height:'auto',
      width: '90vw',
      data: { invoices: [new Invoice()], action: 3 }
    });

    dialogRef.afterClosed().subscribe(result => {
      this.isLoadingResults = true;
      this.UpdateInvoices();

    });
  }
  openSerch(): void {
    this.dialog.open(InvoicePdf, {
      disableClose: true,
      maxHeight: '96vh',
      height: 'auto',
      width: '1650px',
      data: this.selection.selected
    });

  }
  openSendByMail(): void {
    let invoiceSendeds = [];

    this.selection.selected.forEach(i => {
      invoiceSendeds.push({ invoiceSendeds: i.invoiceSendeds, number: i.number, id: i.id });
    });
    this.dialog.open(InvoiceSend, {
      disableClose: true,
      maxHeight: '96vh',
      height: 'auto',
      width: '90vw',
      data: invoiceSendeds
    });
  }

/*  OpenChoosenPayments(): void{
    this.selection.selected.forEach(i => {
      invoiceSendeds.push({ invoiceSendeds: i.invoiceSendeds, number: i.number, id: i.id });
    });

    this.dialog.open(InvoiceSend, {
      disableClose: true,
      height: 'auto',
      width: '90vw',
      data: invoiceSendeds
    });
  }*/
  //actions
  oetJPK(): void {
    //change string to dates ?? why the dont instanty transfer to date format


    const selectedCreationDates = this.selection.selected.map(function (e) { return e.created; })
    this._httpClient.get<Company>('api/companypanel/' + this.currentCompanyId).subscribe(currentCompany => {
      var text = '<?xml version="1.0" encoding="utf-8"?>' +
        '<tns:JPK xmlns:tns="http://jpk.mf.gov.pl/wzor/2019/09/27/09271/" xmlns:etd="http://crd.gov.pl/xml/schematy/dziedzinowe/mf/2018/08/24/eD/DefinicjeTypy/">' +
        '<tns:Naglowek>' +
        '<tns:KodFormularza kodSystemowy="JPK_FA (3)" wersjaSchemy="1-0">JPK_FA</tns:KodFormularza>' +
        '<tns:WariantFormularza>3</tns:WariantFormularza>' +
        '<tns:CelZlozenia>1</tns:CelZlozenia>' +
        '<tns:DataWytworzeniaJPK>' + this.toISOSDateString(new Date()) + '</tns:DataWytworzeniaJPK>' +
        '<tns:DataOd>' + this.toISOSDateString(new Date(Math.min.apply(null, selectedCreationDates))) + '</tns:DataOd>' +
        '<tns:DataDo>' + this.toISOSDateString(new Date(Math.max.apply(null, selectedCreationDates))) + '</tns:DataDo>' +
        '<tns:KodUrzedu></tns:KodUrzedu>' +
        '</tns:Naglowek>' +

        '<tns:Podmiot1>' +
        '<tns:IdentyfikatorPodmiotu>' +
        '<etd:NIP>' + currentCompany.nip + '</etd:NIP>' +
        '<etd:PelnaNazwa>' + currentCompany.name + '</etd:PelnaNazwa>' +
        '</tns:IdentyfikatorPodmiotu>' +
        '<tns:AdresPodmiotu>' +
        '<etd:KodKraju>' + currentCompany.country + '</etd:KodKraju>' +
        '<etd:Wojewodztwo></etd:Wojewodztwo>' +
        '<etd:Powiat></etd:Powiat>' +
        '<etd:Gmina></etd:Gmina>' +
        '<etd:Ulica>' + currentCompany.nip + '</etd:Ulica>' +
        '<etd:NrDomu>' + currentCompany.apartamentNumber + '</etd:NrDomu>' +
        (currentCompany.housenumber != null ? '<etd:NrLokalu>' + currentCompany.housenumber + '</etd:NrLokalu>' : '') +
        '<etd:Miejscowosc>' + currentCompany.city + '</etd:Miejscowosc>' +//city czy postoffice
        '<etd:KodPocztowy>' + currentCompany.postalcode + '</etd:KodPocztowy>' +
        '</tns:AdresPodmiotu>' +
        '</tns:Podmiot1>';
      let invoiceWorth = 0;
      let entriesWorth = 0;
      let entriesCount = 0;
      let invoiceCount = 0;
      let entriesXML = '';
      let entrieWorth = 0;
      let contractorAdress = '';
      let issuerAdress = '';
      this.selection.selected.forEach(i => {
        contractorAdress = i.invoiceContractor.street + ' ' + (i.invoiceContractor.apartamentNumber ? (i.invoiceContractor.apartamentNumber + '/') : '') + i.invoiceContractor.houseNumber + ', ' + i.invoiceContractor.city + ', ' + i.invoiceContractor.postalcode + ' ' + i.invoiceContractor.postoffice + ', ' + i.invoiceContractor.country;
        issuerAdress = i.invoiceIssuer.street + ' ' + (i.invoiceIssuer.apartamentNumber ? (i.invoiceIssuer.apartamentNumber + '/') : '') + i.invoiceIssuer.houseNumber + ', ' + i.invoiceIssuer.city + ', ' + i.invoiceIssuer.postalcode + ' ' + i.invoiceIssuer.postoffice + ', ' + i.invoiceIssuer.country;

        text +=
          '<tns:Faktura>' +
          '<tns:KodWaluty>' + i.paymentCurrency + '</tns:KodWaluty>' +
          '<tns:P_1>' + this.toISOSDateString(i.created) + '</tns:P_1>' +
          '<tns:P_2A>' + i.number + '</tns:P_2A>' +
          '<tns:P_3A>' + i.invoiceContractor.name + '</tns:P_3A>' +
          '<tns:P_3B>' + contractorAdress + '</tns:P_3B>' +
          '<tns:P_3C>' + i.invoiceIssuer.name + '</tns:P_3C>' +
          '<tns:P_3D>' + issuerAdress + '</tns:P_3D>' +
          '<tns:P_4A>' + i.invoiceIssuer.country + '</tns:P_4A>' +
          '<tns:P_4B>' + i.invoiceIssuer.nip + '</tns:P_4B>' +
          '<tns:P_5A>' + i.invoiceContractor.country + '</tns:P_5A>' +
          '<tns:P_5B>' + i.invoiceContractor.nip + '</tns:P_5B>' +
          '<tns:P_6>' + this.toISOSDateString(i.sellDate) + '</tns:P_6>' +
          '<tns:P_13_1>' + i.nettoWorth + '</tns:P_13_1>' +
          '<tns:P_14_1>' + Number(i.bruttoWorth - i.nettoWorth).toFixed(2) + '</tns:P_14_1>' +
          '<tns:P_15>' + i.bruttoWorth + '</tns:P_15>' +
          '<tns:P_16>false</tns:P_16>' +
          '<tns:P_17>false</tns:P_17>' +
          '<tns:P_18>false</tns:P_18>' +
          '<tns:P_18A>false</tns:P_18A>' +
          '<tns:P_19>false</tns:P_19>' +
          '<tns:P_20>false</tns:P_20>' +
          '<tns:P_21>false</tns:P_21>' +
          '<tns:P_22>false</tns:P_22>' +
          '<tns:P_23>false</tns:P_23>' +
          '<tns:P_106E_2>false</tns:P_106E_2>' +
          '<tns:P_106E_3>false</tns:P_106E_3>' +
          '<tns:RodzajFaktury>VAT</tns:RodzajFaktury>' +
          '</tns:Faktura>';


        i.invoiceEntries.forEach(e => {
          entrieWorth = (e.quantity * e.price);
          entriesXML +=
            '<tns:FakturaWiersz>' +
            '<tns:P_2B>' + i.number + '</tns:P_2B>' +
            '<tns:P_7>' + e.name + ' </tns:P_7>' +
            '<tns:P_8A>SZT</tns:P_8A>' +
            '<tns:P_8B>' + e.quantity + '</tns:P_8B>' +
            '<tns:P_9A>' + entrieWorth + '</tns:P_9A>' +
            '<tns:P_9B>' + Number(entrieWorth + (entrieWorth * (e.vat / 100))).toFixed(2) + '</tns:P_9B>' +
            '<tns:P_11>' + entrieWorth + '</tns:P_11>' +
            '<tns:P_11A>' + Number(entrieWorth + (entrieWorth * (e.vat / 100))).toFixed(2) + '</tns:P_11A>' +
            '<tns:P_12>' + e.vat + '</tns:P_12>' +//ogarnac zw i np
            '</tns:FakturaWiersz>';
          entriesWorth += entrieWorth;
          entriesCount++;
        });

        invoiceWorth += i.bruttoWorth;
        invoiceCount++;

      });

      text +=
        '<tns:FakturaCtrl>' +
        '<tns:LiczbaFaktur>' + invoiceCount + '</tns:LiczbaFaktur>' +
        '<tns:WartoscFaktur>' + invoiceWorth + '</tns:WartoscFaktur>' +
        '</tns:FakturaCtrl>';

      text += entriesXML;

      text +=
        '<tns:FakturaWierszCtrl>' +
        '<tns:LiczbaWierszyFaktur>' + entriesCount + '</tns:LiczbaWierszyFaktur>' +
        '<tns:WartoscWierszyFaktur>' + entriesWorth + '</tns:WartoscWierszyFaktur>' +
        '</tns:FakturaWierszCtrl>';

      text += '</tns:JPK>';
      var blob = new Blob([text], { type: "text/xml;charset=utf8", });
      saveAs(blob, "jpk_fa_" + currentCompany.name + "_" + this.toISOSDateString(new Date()) + ".xml");
    });
  }
  async delate() {
    const dialogRefIfDelate = this.dialog.open(AskIfDialog, {
      disableClose: true,
      data: { data: "Czy na pewno chesz usunąć wybrane faktury ?", close: "Nie", ok: "Tak" }
    });

    dialogRefIfDelate.afterClosed().subscribe(result => {
      if (result) {
        let isAllDelated = [];
        let countRespons = 0;
        this.selection.selected.forEach((invoice) => {
          this._httpClient.delete<number>(`${this._envUrl}/api/invoice/` + invoice.id).pipe(
            finalize(() => {
              if (++countRespons >= this.selection.selected.length) {
                //po wszystkich elementach
                const dialogRefDelateInfo = this.dialog.open(AskIfDialog, {
                  disableClose: true,
                  data: { data: isAllDelated.length == 0 ? "Faktury usunięte pomyślnie" : "Wystąpił błąd przy usuwaniu faktur :" + isAllDelated.toString(), close: "Zamknij", ok: null }
                });
                dialogRefDelateInfo.afterClosed().subscribe(() => {
                  if (isAllDelated.length != 0) {
                    this.UpdateInvoices();
                  } else {
                    this.UpdateInvoices();
                  }
                  this.dialog.closeAll();
                });
              }
            }));/*.subscribe(() => {
            if (++countRespons >= this.selection.selected.length) {
              //po wszystkich elementach
              const dialogRefDelateInfo = this.dialog.open(AskIfDialog, {
                disableClose: true,
                data: { data: isAllDelated.length == 0 ? "Faktury usunięte pomyślnie" : "Wystąpił błąd przy usuwaniu faktur :" + isAllDelated.toString(), close: "Zamknij", ok: null }
              });
              dialogRefDelateInfo.afterClosed().subscribe(() => {
                if (isAllDelated.length != 0) {
                  this.UpdateInvoices();
                } else {
                  this.UpdateInvoices();
                }
                this.dialog.closeAll();
              });
            }
          },
            error => {
              isAllDelated.push(invoice.number)
              if (++countRespons >= this.selection.selected.length) {
                //po wszystkich elementach
                const dialogRefDelateInfo = this.dialog.open(AskIfDialog, {
                  disableClose: true,
                  data: { data: isAllDelated.length == 0 ? "Faktury usunięte pomyślnie" : "Wystąpił błąd przy usuwaniu faktur :" + isAllDelated.toString(), close: "Zamknij", ok: null }
                });
                dialogRefDelateInfo.afterClosed().subscribe(() => {
                  if (isAllDelated.length != 0) {
                    this.UpdateInvoices();
                  } else {
                    //this.selection.selected.splice(0, this.selection.selected.length);
                    this.UpdateInvoices();
                  }
                  this.dialog.closeAll();
                });
              }
            }
          );*/
        });
      }
    });
  }
  //select rows
  isAllSelected() {
    const numSelected = this.selection.selected.length;
    const numRows = this.dataSource.data.length;
    return numSelected === numRows;
  }

  masterToggle() {
    this.isAllSelected() ?
      this.selection.clear() :
      this.dataSource.data.forEach(row => this.selection.select(row));
  }

  //calculator
  calculatorChangeRow(row: Invoice, isselect: boolean) {
    var calculatorRow = this.calculator.data.filter(c => c.paymentCurrency == row.paymentCurrency);
    if (calculatorRow[0] != undefined) {
      calculatorRow[0].bruttoWorth = !isselect ? Number((calculatorRow[0].bruttoWorth - row.bruttoWorth).toFixed(4)) : Number((calculatorRow[0].bruttoWorth + row.bruttoWorth).toFixed(4));
      calculatorRow[0].nettoWorth = !isselect ? Number((calculatorRow[0].nettoWorth - row.nettoWorth).toFixed(4)) : Number((calculatorRow[0].nettoWorth + row.nettoWorth).toFixed(4));
      calculatorRow[0].vat = !isselect ? Number((calculatorRow[0].vat - (row.bruttoWorth - row.nettoWorth)).toFixed(4)) : Number((calculatorRow[0].vat + (row.bruttoWorth - row.nettoWorth)).toFixed(4));
      calculatorRow[0].paymentValue = !isselect ? Number((calculatorRow[0].paymentValue - row.paymentsForInvoices.reduce((sum, current) => sum + current.paymentValueForInvoice, 0)).toFixed(4)) : Number((calculatorRow[0].paymentValue + row.paymentsForInvoices.reduce((sum, current) => sum + current.paymentValueForInvoice, 0)).toFixed(4));
      calculatorRow[0].paymentStatus = Number((calculatorRow[0].bruttoWorth - calculatorRow[0].paymentValue).toFixed(4));
      if (calculatorRow[0].paymentStatus < 0) {
        calculatorRow[0].paymentStatus = 0;
      }

      if (calculatorRow[0].bruttoWorth == 0) {
        this.calculator.data = this.calculator.data.filter(c => c.paymentCurrency != row.paymentCurrency);
      }
    }
    else {
      const ihc = <InvoiceCalculator>{ paymentCurrency: row.paymentCurrency, nettoWorth: row.nettoWorth, bruttoWorth: row.bruttoWorth, vat: Number((row.bruttoWorth - row.nettoWorth).toFixed(4)), paymentValue: 0, paymentStatus: 0 };
      this.calculator.data.push(ihc);
    }
    this.calculator._updateChangeSubscription();
  }
  //http
  loadInvoices(filter: string,
    numericFilter: string,
    sort: string,
    startDateFilter: any,
    endDateFilter: any,
    pageIndex: number,
    pageSize: number) {

    if (moment(endDateFilter).isValid()) {
      endDateFilter = moment(endDateFilter).toDate();
      endDateFilter.setMinutes((endDateFilter.getMinutes() - endDateFilter.getTimezoneOffset()))
      endDateFilter = endDateFilter.toISOString();
    } else endDateFilter = null

    if (moment(startDateFilter).isValid()) {
      startDateFilter = moment(startDateFilter).toDate();
      startDateFilter.setMinutes((startDateFilter.getMinutes() - startDateFilter.getTimezoneOffset()))
      startDateFilter = startDateFilter.toISOString();
    } else startDateFilter = null

    let companyId;
    this._authService.currentSelectedCompany.subscribe(company => companyId = company['id']);
    var params = new HttpParams().set('sort', sort).set('pageNumber', pageIndex.toString()).set('pageSize', pageSize.toString()).set('companyId', companyId);
    if (filter) params = params.append('filter', filter);
    if (numericFilter) params = params.append('numericFilter', numericFilter);
    if (startDateFilter) params = params.append('startDateFilter', startDateFilter);
    if (endDateFilter) params = params.append('endDateFilter', endDateFilter);
    return this._httpClient.get<InvoiceTable>('api/invoice/getinvoices', params);

  }

  //helpers

  public toISOSDateString(date: Date) {
    return date.getFullYear() + '-' +
      ('0' + (date.getMonth() + 1)).slice(-2) + '-' +
      ('0' + date.getDate()).slice(-2);
  }
  UpdateInvoices() {
    this.loadInvoices(
      this.createFilter(this.filter),
      this.createFilter(this.numericFilter),
      this.sort.active + " " + this.sort.direction,
      this.dateFilter.get("CreatedStart").value,
      this.dateFilter.get("CreatedEnd").value,
      this.paginator.pageIndex,
      this.paginator.pageSize
    ).pipe(
      map(data => {
        this.isLoadingResults = false;
        this.invoicesTotalCount = data.totalCount;
        this.selection.clear();
        return data.invoice;
      })
    ).subscribe(data => {
      data.forEach(d => {
        d.created = new Date(d.created);
        d.sellDate = new Date(d.sellDate);
        d.paymentDate = new Date(d.paymentDate);
      });
      this.dataSource.data = data;
    });
  }
  EditMailTempaltes() {
    const dialogRef = this.dialog.open(InvoiceMailTemplateComponent, {
      maxHeight: '96vh',
      height: 'auto',
      width: '90vw',
    });  }
}

