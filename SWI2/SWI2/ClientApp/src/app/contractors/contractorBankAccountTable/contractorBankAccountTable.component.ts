import { Component, Inject } from "@angular/core";
import { MatDialog, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { ContractorBankAccount, ContractorBankAccountTableModel } from "../../models/contractors/ContractorBankAccount.model";
import { AskIfDialog } from "../../shared/components/askIfDialog/askIfDialog.component";
import { CustomFormDialog, FormPart } from "../../shared/components/customFormDialog/customFormDialog.component";
import { ApiService } from "../../shared/services/api.service";
import { AuthenticationService } from "../../shared/services/authentication.service";

@Component({
  selector: 'swi-contractorBankAccountTable',
  templateUrl: './contractorBankAccountTable.component.html',
  styleUrls: ['./contractorBankAccountTable.component.css']
})

export class ContractorBankAccountTable {

  requestUrl = "api/contractor/bankaccount/";

  actualCompanyId: number;
  selectedRow: ContractorBankAccountTableModel;
  dataSource: any;
  constructor(public dialog: MatDialog,
    private _api: ApiService,
    public _authService: AuthenticationService,
    @Inject(MAT_DIALOG_DATA) public data: ContractorBankAccountTableModel[]) {
    this._authService.currentSelectedCompany.subscribe(c => { this.actualCompanyId = c.id; })
     
  }

  add() {
    const dialogRef = this.dialog.open(CustomFormDialog, {
      disableClose: true,
      width: '90vw',
      data: {
        description: "CONTRACTORS.CONTRACTORBANKACCOUNTTABLE.ADD_BANK_ACCOUNT_DESCRIPTION",
        close: "BACK",
        ok: "ADD",
        form: [
          { description: "CONTRACTOR", name: "name", value: this.selectedRow.contarctorName, regEx: {value:"", disabled:true} } as FormPart,
          { description: "BANK_NAME", name: "bankName", value: '', regEx: "" } as FormPart,
          { description: "ACCOUNT_NUMBER", name: "accountNumber", value: '', regEx: "" } as FormPart]
      }
    });

    dialogRef.afterClosed().subscribe(ba => {
      ba = {id: 0, bankName: ba.bankName, accountNumber: ba.accountNumber, contractor: { id: this.selectedRow.contarctorId }} as ContractorBankAccount;
      this._api.post<ContractorBankAccount>(this.requestUrl + this.selectedRow.contarctorId, ba).subscribe(
        res => {
          this.data.push(new ContractorBankAccountTableModel(res.id, this.selectedRow.contarctorId, this.selectedRow.contarctorName, res.bankName, res.accountNumber, res.created));
        this.dialog.open(AskIfDialog, {
          disableClose: true,
          data: { data: "CONTRACTORS.CONTRACTORBANKACCOUNTTABLE.ADD_SUCCESS", close: "CLOSE", ok: null }
        });
      },
        error => {
          this.dialog.open(AskIfDialog, {
            disableClose: true,
            data: { data: "CONTRACTORS.CONTRACTORBANKACCOUNTTABLE.ADD_ERROR", close: "CLOSE", ok: null }
          });
        });
    });

  }

  edit() {
    const dialogRef = this.dialog.open(CustomFormDialog, {
      disableClose: true,
      width: '90vw',
      data: {
        description: "CONTRACTORS.CONTRACTORBANKACCOUNTTABLE.EDIT_BANK_ACCOUNT_DESCRIPTION",
        close: "BACK",
        ok: "EDIT",
        form: [
          { description: "BANK_NAME", name: "bankName", value: this.selectedRow.bankName, regEx: "" } as FormPart,
          { description: "ACCOUNT_NUMBER", name: "accountNumber", value: this.selectedRow.accountNumber, regEx: "" } as FormPart]
      }
    });
    dialogRef.afterClosed().subscribe(nc => {

      if (nc != null) {
        nc = this.getDiffrences(this.getSendObject(this.selectedRow), this.getSendObject(nc));
        nc.id = this.selectedRow.id;
        this._api.put<ContractorBankAccount>(this.requestUrl + this.actualCompanyId, nc).subscribe(
          res => {
            this.selectedRow.created = res.created;
            this.selectedRow.bankName = res.bankName;
            this.selectedRow.accountNumber = res.accountNumber; 
          this.dialog.open(AskIfDialog, {
            disableClose: true,
            data: { data: "CONTRACTORS.CONTRACTORBANKACCOUNTTABLE.EDIT_SUCCESS", close: "CLOSE", ok: null }
          });
        },
          error => {
            this.dialog.open(AskIfDialog, {
              disableClose: true,
              data: { data: "CONTRACTORS.CONTRACTORBANKACCOUNTTABLE.EDIT_ERROR", close: "CLOSE", ok: null }
            });
          });
      }
    });
  }


  delate() {
    const dialogRef = this.dialog.open(AskIfDialog, {
      disableClose: true,
      data: { data: "CONTRACTORS.CONTRACTORBANKACCOUNTTABLE.ASK_DELETE", close: "NO", ok: "YES" }
    });

    dialogRef.afterClosed().subscribe(ifDel => {
      if (ifDel) {
        this._api.delete(this.requestUrl + this.selectedRow.id).subscribe(
          res => {
            delete this.selectedRow; 
            this.dialog.open(AskIfDialog, {
              disableClose: true,
              data: { data: "CONTRACTORS.CONTRACTORBANKACCOUNTTABLE.DELETE_SUCCESS", close: "CLOSE", ok: null }
            });
          },
          error => {
            this.dialog.open(AskIfDialog, {
              disableClose: true,
              data: { data: "CONTRACTORS.CONTRACTORBANKACCOUNTTABLE.DELETE_ERROR", close: "CLOSE", ok: null }
            });
          });
      }
    });
  }

  selectBankAccout(selectedRow) {
    this.selectedRow = selectedRow;

  }
  //helpers
  getSendObject(contractor: object) {
    let sedContractor = {};
    Object.entries(contractor).forEach(([k, v]) => {
      if (typeof v === 'object' && v !== null) {
        Object.entries(contractor).forEach(([ko, vo]) => {
          sedContractor[ko] = vo;
        });
      } else {
        sedContractor[k] = v;
      }
    });
    return sedContractor;
  }
  getDiffrences(startingObject, changes) {
    let diff = {};
    Object.entries(changes).forEach(([k, v]) => {
      if (v != startingObject[k] && typeof v != 'object' && v != null) {
        diff[k] = v;

      }
    });
    return diff;
  }
}
