import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, Inject, ViewChild } from '@angular/core';
import { MatDialog, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Invoice, InvoiceEntries, InvoiceSendeds } from '../../models/invoices/Invoice.model';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { ApiService } from '../../shared/services/api.service';

@Component({
  selector: 'swi-invoiceSend',
  templateUrl: './invoiceSend.component.html',
  styleUrls: ['./invoiceSend.component.css']
})

export class InvoiceSend {
  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild('sendedTable') sendedTable;
  displayedInvoiceColumns: string[] = ['Number', 'User', 'Email', 'Created'];
  dataSource = new MatTableDataSource<InvoiceSendeds>();


  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any[],
    public dialog: MatDialog) {
    this.UpdateTable()
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;

    let tbody = this.sendedTable._elementRef.nativeElement.children[1];
    for (let td = 0;td < tbody.children.length; td++) {
      if (tbody.children[td].firstChild.innerHTML == '') tbody.children[td].firstChild.remove();
    }
  }

  UpdateTable() {
    this.data.forEach((i: any) => {
      let isNumber = true;
      i.invoiceSendeds.forEach(is => {
        let isViue = new InvoiceSendeds ();
        if (isNumber) {
          isViue.number = i.number + '|' + i.invoiceSendeds.length;
          isNumber = false;
        }
        isViue.created = is.created;
        isViue.email = is.email;
        isViue.user.userName = is.user.userName;

        this.dataSource.data.push(isViue);
      });
    });
  }
}
