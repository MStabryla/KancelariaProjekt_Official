import { SelectionModel } from "@angular/cdk/collections";
import { Component } from "@angular/core";
import { Validators } from "@angular/forms";
import { MatDialog } from "@angular/material/dialog";
import { Subject } from "rxjs";
import { FormModel } from "../../models/Form.model";
import { PaymentModel, PaymentTableModel } from "../../models/payments/Payment.model";
import { AskIfDialog } from "../../shared/components/askIfDialog/askIfDialog.component";
import { ApiService } from "../../shared/services/api.service";
import { AuthenticationService } from "../../shared/services/authentication.service";
import { ErrorDialogService } from "../../shared/services/error-dialog-service";
import { paymentsCsvSettelment } from "../paymentsCsvSettelment/paymentsCsvSettelment.component";
import { paymentsEdit } from "../paymentsEdit/paymentsEdit.component";

@Component({
  selector: 'swi-paymentsTable',
  templateUrl: './paymentsTable.component.html',
  styleUrls: ['./paymentsTable.component.css']
})

export class PaymentsTable {

  sortActive = "created";
  requestUrl = "api/payment/{id}";
  headerRow = [{ description: "PEYMENTS.PAYMENT_TABLE.TABLE_COTRACTOR_NAME", name: "contractorName", type: "text", ifRange: false } as FormModel,
    { description: "PEYMENTS.PAYMENT_TABLE.TABLE_INVOICE_NUMBER", name: "paymentsForInvoices", type: ["invoice","number"], ifRange: false } as FormModel,
    { description: "PEYMENTS.PAYMENT_TABLE.TABLE_CONTRACTOR_NIP", name: "contractorNip", type: "text", ifRange: false } as FormModel,
    { description: "PEYMENTS.PAYMENT_TABLE.TABLE_COTRACTOR_BANK_NAME", name: "contractorBankAccountName", type: "text", ifRange: false } as FormModel,
    { description: "PEYMENTS.PAYMENT_TABLE.TABLE_COTRACTOR_BANK_ACCOUNT", name: "contractorBankAccountNumber", type: "text", ifRange: false } as FormModel,
    { description: "PEYMENTS.PAYMENT_TABLE.TABLE_TITLE", name: "topic", type: "text", ifRange: false } as FormModel,
    { description: "PEYMENTS.PAYMENT_TABLE.TABLE_PAYMENT_VALUE", name: "paymentValue", type: "number", ifRange: true } as FormModel,
    { description: "PEYMENTS.PAYMENT_TABLE.TABLE_PAYMENT_CURRENCY", name: "currency", type: "text", ifRange: false } as FormModel,
    { description: "PEYMENTS.PAYMENT_TABLE.TABLE_PAYMENT_DATE", name: "paymentDate", type: "date", ifRange: true } as FormModel,
    { description: "PEYMENTS.PAYMENT_TABLE.TABLE_NAME_ADDED_DATE", name: "created", type: "date", ifRange: true } as FormModel
  ];
  validators = [Validators.nullValidator, Validators.pattern("^[0-9]*$"), Validators.nullValidator, Validators.nullValidator, Validators.nullValidator, Validators.pattern("^[0-9]*$"), Validators.nullValidator, Validators.nullValidator, Validators.nullValidator];

  selection = new SelectionModel<PaymentTableModel>(true, []);
  refreshTable: Subject<void> = new Subject<void>();
  actualCompanyId: number;

  constructor(
    private _api: ApiService,
    public _authService: AuthenticationService,
    private errorD: ErrorDialogService,
    public dialog: MatDialog) {
    this._authService.currentSelectedCompany.subscribe(c => { this.actualCompanyId = c.id; });

  }

  openAdd() {
    const dialogRef = this.dialog.open(paymentsEdit, {
      disableClose: true,
      maxHeight: '96vh',
      height: 'auto',
      width: '90vw',
      data: { data: [new PaymentTableModel()], action: 'add' } 
    });

    dialogRef.afterClosed().subscribe(result => {
      this.refreshTable.next();
    });

  }
  openEdit() {
    const dialogRef = this.dialog.open(paymentsEdit, {
      disableClose: true,
      maxHeight: '96vh',
      height: 'auto',
      width: '90vw',
      data: { data: this.selection.selected, action: 'edit' }
    });

    dialogRef.afterClosed().subscribe(result => {
      this.refreshTable.next();
      this.selection.clear();
    });
  }

  delate() {

    const dialogRef = this.dialog.open(AskIfDialog, {
      disableClose: true,
      maxHeight: '96vh',
      height: 'auto',
      width: '90vw',
      data: { data: this.selection.select.length > 1 ? "PAYMENTS.PAYMENT_TABLE.ASK_IF_DELETE" : "PAYMENTS.PAYMENT_TABLE.ASK_IF_DELETE_MENY" , close: "NO", ok: "YES" }
    });

    dialogRef.afterClosed().subscribe(result => {

      if (result) {
        Object.entries(this.selection.selected).forEach(([_, v],index) => {
          this._api.delete<PaymentModel>(`api/payment/` + v.id).subscribe(
            res => {
              if ((this.selection.selected.length - 1) == index) {
                this.refreshTable.next();
                this.selection.clear();
              }
            },
            error => {
              this.errorD.showDialog(error);
            });
        });
      }
    });
  }

  dblClickPayment(payment) {
    const dialogRef = this.dialog.open(paymentsEdit, {
      disableClose: true,
      maxHeight: '96vh',
      height: 'auto',
      width: '90vw',
      data: { data: [payment], action: 'edit' }
    });

    dialogRef.afterClosed().subscribe(result => {
      this.refreshTable.next();
    });
  }

  csvSettelment() {
    const dialogRef = this.dialog.open(paymentsCsvSettelment, {
      disableClose: true,
      maxHeight: '96vh',
      height: 'auto',
      width: '90vw',
    });

    dialogRef.afterClosed().subscribe(result => {
      this.refreshTable.next();
    });
  }
}
