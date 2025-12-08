import { SelectionModel } from "@angular/cdk/collections";
import { Component } from "@angular/core";
import { Validators } from "@angular/forms";
import { MatDialog } from "@angular/material/dialog";
import { Subject } from "rxjs";
import { Contractor, MailLanguage } from "../../models/contractors/Contractor.model";
import { ContractorBankAccountTableModel } from "../../models/contractors/ContractorBankAccount.model";
import { FormModel } from "../../models/Form.model";
import { Adress } from "../../shared/components/adressMatInput/adressMatInput.component";
import { AskIfDialog } from "../../shared/components/askIfDialog/askIfDialog.component";
import { CodeCoutry } from "../../shared/components/coutryCodeMatInput/codeCoutryMatInput.component";
import { CustomFormDialog, FormPart } from "../../shared/components/customFormDialog/customFormDialog.component";
import { ApiService } from "../../shared/services/api.service";
import { AuthenticationService } from "../../shared/services/authentication.service";
import { ContractorBankAccountTable } from "../contractorBankAccountTable/contractorBankAccountTable.component";

@Component({
  selector: 'swi-contractorsTable',
  templateUrl: './contractorsTable.component.html',
  styleUrls: ['./contractorsTable.component.css']
})

export class ContractorsTable {

  sortActive = "created";
  requestUrl = "api/contractor/{id}";
  headerRow = [{ description: "CONTRACTORS.CONTRACTORTABLE.TABLE_CONTRACTOR_NAME_DESCRIPTION", name: "name", type: "text", ifRange: false } as FormModel,
    { description: "CONTRACTORS.CONTRACTORTABLE.TABLE_NIP_DESCRIPTION", name: "nip", type: "text", ifRange: false } as FormModel,
    { description: "CONTRACTORS.CONTRACTORTABLE.TABLE_ADRESS_DESCRIPTION", name: "adress-input", type: "adress-input", ifRange: false } as FormModel,
    { description: "CONTRACTORS.CONTRACTORTABLE.TABLE_CODECOUNTRY_DESCRIPTION", name: "codeCoutry-input", type: "codeCoutry-input", ifRange: false } as FormModel,
    { description: "CONTRACTORS.CONTRACTORTABLE.TABLE_WNA_ACCOUNT_DESCRIPTION", name: "wnAccount", type: "text", ifRange: false } as FormModel,
    { description: "CONTRACTORS.CONTRACTORTABLE.TABLE_EMAIL_DESCRIPTION", name: "email", type: "email", ifRange: false } as FormModel,
    { description: "CONTRACTORS.CONTRACTORTABLE.TABLE_EMAIL_LANGUAGE_DESCRIPTION", name: "mailLanguage", type: MailLanguage, ifRange: false } as FormModel,
    { description: "CONTRACTORS.CONTRACTORTABLE.TABLE_CREATION_DATE_DESCRIPTION", name: "created", type: "date", ifRange: true } as FormModel

  ];
  validators = [Validators.nullValidator, Validators.pattern("^[0-9]*$"), Validators.nullValidator, Validators.nullValidator, Validators.nullValidator, Validators.email, Validators.nullValidator, Validators.nullValidator];
  selection = new SelectionModel<Contractor>(true, []);
  actualCompanyId: number;
  refreshTable: Subject<void> = new Subject<void>();

  constructor(public dialog: MatDialog,
    private _api: ApiService,
    public _authService: AuthenticationService) {
    this._authService.currentSelectedCompany.subscribe(c => { this.actualCompanyId = c.id; })
  }

  add() {
    this.modifiyContractor(new Contractor()).then(nc => {
      if (nc != null) {
        nc.id = 0;
        this._api.post<Contractor>("api/contractor/" + this.actualCompanyId, this.getSendObject(nc)).subscribe(res => {
          this.dialog.open(AskIfDialog, {
            disableClose: true,
            data: { data: "CONTRACTORS.CONTRACTORTABLE.ADD_SUCCESS", close: "CLOSE", ok: null }
          });
          this.refreshTable.next();
        },
          error => {
            this.dialog.open(AskIfDialog, {
              disableClose: true,
              data: { data: "CONTRACTORS.CONTRACTORTABLE.ADD_SUCCESS", close: "CLOSE", ok: null }
            });
          });
      }
    });
  }

  edit() {
    this.modifiyContractor(this.selection.selected[0]).then(nc => {

      if (nc != null) {
        nc = this.getDiffrences(this.getSendObject(this.selection.selected[0]), this.getSendObject(nc));
        nc.id = this.selection.selected[0].id;
        this._api.put<Contractor>("api/contractor/" + this.actualCompanyId, nc).subscribe(res => {
          this.dialog.open(AskIfDialog, {
            disableClose: true,
            data: { data: "CONTRACTORS.CONTRACTORTABLE.EDIT_SUCCESS", close: "CLOSE", ok: null }
          });
          this.refreshTable.next();
        },
          error => {
            this.dialog.open(AskIfDialog, {
              disableClose: true,
              data: { data: "CONTRACTORS.CONTRACTORTABLE.EDIT_ERROR", close: "CLOSE", ok: null }
            });
          });
      }
    });

  }
  dblClickContractor(contractor: Contractor) {
    this.modifiyContractor(contractor).then(nc => {
      if (nc != null) {

        nc = this.getDiffrences(this.getSendObject(contractor), this.getSendObject(nc));
        nc.id = contractor.id;

        this._api.put<Contractor>("api/contractor/" + this.actualCompanyId, nc).subscribe(res => {
          this.dialog.open(AskIfDialog, {
            disableClose: true,
            data: { data: "CONTRACTORS.CONTRACTORTABLE.EDIT_SUCCESS", close: "CLOSE", ok: null }
          });
          this.refreshTable.next();
        },
          error => {
            this.dialog.open(AskIfDialog, {
              disableClose: true,
              data: { data: "CONTRACTORS.CONTRACTORTABLE.EDIT_ERROR", close: "CLOSE", ok: null }
            });
          });
      }

    });

  }

  modifiyContractor(contractor: Contractor) {

    const dialogRef = this.dialog.open(CustomFormDialog, {
      disableClose: true,
      maxHeight: '96vh',
      height: 'auto',
      width: '90vw',
      data: {
        description: contractor.id ? "CONTRACTORS.CONTRACTORTABLE.EDIT_CONTRACTOR_DESCRIPTION" : "CONTRACTORS.CONTRACTORTABLE.ADD_CONTRACTOR_DESCRIPTION",
        close: "Cofnij",
        ok: contractor.id ? "EDIT" : "ADD",
        form: [
          { description: "NAME", name: "name", value: contractor.name, regEx: "" } as FormPart,
          { description: "NIP", name: "nip", value: contractor.nip, regEx: "" } as FormPart,
          { description: "ADRESS", name: "adress", value: new Adress(contractor.street != null ? contractor.street : "", contractor.houseNumber != null ? contractor.houseNumber : "", contractor.apartamentNumber != null ? contractor.apartamentNumber : "", contractor.city != null ? contractor.city : ""), regEx: "" } as FormPart,
          { description: "CODECOUNTRY", name: "codeCoutry", value: new CodeCoutry(contractor.postalcode != null ? contractor.postalcode : "", contractor.postoffice != null ? contractor.postoffice : "", contractor.country != null ? contractor.country : ""), regEx: "" } as FormPart,
          { description: "EMAIL", name: "email", value: contractor.email, regEx: "^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$" } as FormPart,
          {
            description: "MAIL_LANGUAGE",
            name: "mailLanguage",
            value: [contractor.mailLanguage, MailLanguage]/*[
             { value: "PL", viewValue: "polska" } as SelectOption,
              { viewValue: "angielska", value: "EN" } as SelectOption,
              { viewValue: "włoska", value: "IT" } as SelectOption]*/,
            regEx: ""
          } as FormPart,
          { description: "WN_ACCOUNT", name: "wNAccount", value: contractor.wNAccount, regEx: "" } as FormPart
        ]
      }
    });

    return dialogRef.afterClosed().toPromise();
  }

  delate() {
    const dialogRef = this.dialog.open(AskIfDialog, {
      disableClose: true,
      data: { data: "CONTRACTORS.CONTRACTORTABLE.ASK_DELETE", close: "NO", ok: "YES," }
    });

    dialogRef.afterClosed().subscribe(ifDel => {
      if (ifDel) {
        let counter = 0;
        let errors = [];
        let errorscount = 0;
        this.selection.selected.forEach(c => {
          this._api.delete("api/contractor/" + c.id).subscribe(
            res => {
              if (counter == this.selection.selected.length) {
                if (errors.length == 0) {
                  this.dialog.open(AskIfDialog, {
                    disableClose: true,
                    data: { data: this.selection.selected.length == 1 ? "CONTRACTORS.CONTRACTORTABLE.DELETE_SUCCESS" : "CONTRACTORS.CONTRACTORTABLE.DELETE_SUCCESS_MENY", close: "CLOSE", ok: null }
                  });
                }
                else {
                  this.dialog.open(AskIfDialog, {
                    disableClose: true,
                    data: { data: this.selection.selected.length == 1 ? "CONTRACTORS.CONTRACTORTABLE.DELETE_ERROR" : "CONTRACTORS.CONTRACTORTABLE.DELETE_ERROR_MENY", close: "CLOSE", ok: null }
                  });
                }
              }
              counter++;
            },
            error => {
              errors.push(this.selection.selected[counter].name);
              if (counter == this.selection.selected.length) {
                if (errors.length == 0) {
                  this.dialog.open(AskIfDialog, {
                    disableClose: true,
                    data: { data: this.selection.selected.length == 1 ? "CONTRACTORS.CONTRACTORTABLE.DELETE_SUCCESS" : "CONTRACTORS.CONTRACTORTABLE.DELETE_SUCCESS_MENY", close: "CLOSE", ok: null }
                  });
                }
                else {
                  this.dialog.open(AskIfDialog, {
                    disableClose: true,
                    data: { data: this.selection.selected.length == 1 ? "CONTRACTORS.CONTRACTORTABLE.DELETE_ERROR" : "CONTRACTORS.CONTRACTORTABLE.DELETE_ERROR_MENY", close: "CLOSE", ok: null }
                  });
                }
              }
              counter++;
            });
        });
      }
    });
  }

  manageContractorsBankAccounts() {
    let selectedBankAccounts =this.selection.selected.map(c => {
      return c.contractorBankAccounts.map(cba => {
        return new ContractorBankAccountTableModel(cba.id, c.id, c.name, cba.bankName, cba.accountNumber, cba.created)
      });
    });
    selectedBankAccounts = [].concat(...selectedBankAccounts);
    const dialogRef = this.dialog.open(ContractorBankAccountTable, {
      width: '90vw',
      data: selectedBankAccounts
    });
    dialogRef.afterClosed().subscribe(result => {
      this.refreshTable.next();
    });
    return dialogRef.afterClosed().toPromise();

  }
  //helpers
  getSendObject(contractor: object) {
    let sedContractor = {};
    Object.entries(contractor).forEach(([k, v]) => {
      if (typeof v === 'object' && v !== null) {
        Object.entries(v).forEach(([ko, vo]) => {
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
