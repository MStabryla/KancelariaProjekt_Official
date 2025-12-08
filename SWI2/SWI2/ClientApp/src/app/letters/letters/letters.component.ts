import { SelectionModel } from '@angular/cdk/collections';
import { Component } from '@angular/core';
import { Validators } from '@angular/forms';
import { FormModel } from '../../models/Form.model';
import { MatDialog } from '@angular/material/dialog';
import { Subject } from 'rxjs';
import { AlertData } from '../../alert/alert.component';
import { AskIfDialog } from '../../shared/components/askIfDialog/askIfDialog.component';
import { ApiService } from '../../shared/services/api.service';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';
import { LetterModel } from '../letter.model';
import { LettersViewComponent } from '../letters-view/letters-view.component';
import { LettersCreateComponent } from '../letters-create/letters-create.component';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
    selector: 'swi-letters',
    templateUrl: './letters.component.html',
    styleUrls: ['./letters.component.css']
})
/** letters component*/
export class LettersComponent {
  sortActive = "created";
  requestUrl = "api/documentpanel/letter/";
  detailsComponent = LettersViewComponent;
  headerRow = [
    { description: "LETTERS.LETTERS.TABLE_FROM_TO", name: "outLetter", type: "boolean", ifRange: false, boolFilter: (v) => v ? "od" : "do" } as FormModel,
    { description: "LETTERS.LETTERS.TABLE_NOTES", name: "notes", type: "text", ifRange: false } as FormModel,
    { description: "LETTERS.LETTERS.TABLE_REAGISTERD_NUMBER", name: "registeredNumbr", type: "text", ifRange: false } as FormModel,
    { description: "LETTERS.LETTERS.TABLE_IF_PAID", name: "isPaid", type: "boolean", ifRange: false, boolFilter: (v) => v ? "tak" : "nie" } as FormModel,
    { description: "LETTERS.LETTERS.TABLE_IS_REAGISTERD", name: "isRegistered", type: "boolean", ifRange: false, boolFilter: (v) => v ? "tak" : "nie" } as FormModel,
    { description: "LETTERS.LETTERS.TABLE_IS_EMAIL", name: "isEmail", type: "boolean", ifRange: false, boolFilter: (v) => v ? "tak" : "nie" } as FormModel,
    { description: "LETTERS.LETTERS.TABLE_IS_NORMAL", name: "isNormal", type: "boolean", ifRange: false, boolFilter: (v) => v ? "tak" : "nie" } as FormModel,
    { description: "LETTERS.LETTERS.TABLE_ISCOURIER", name: "isCourier", type: "boolean", ifRange: false, boolFilter: (v) => v ? "tak" : "nie" } as FormModel,
    { description: "LETTERS.LETTERS.TABLE_WITH_CONFIRM", name: "withConfirm", type: "boolean", ifRange: false, boolFilter: (v) => v ? "tak" : "nie" } as FormModel,
    { description: "LETTERS.LETTERS.TABLE_ADDED_DATE", name: "added", type: "date", ifRange: true } as FormModel,
    { description: "CREATION_DATE", name: "created", type: "date", ifRange: true } as FormModel
  ]
  validators = [Validators.nullValidator, Validators.nullValidator, Validators.nullValidator, Validators.nullValidator, Validators.nullValidator, Validators.nullValidator, Validators.nullValidator, Validators.nullValidator, Validators.nullValidator, Validators.nullValidator, Validators.nullValidator];
  selection = new SelectionModel<LetterModel>(true, []);
  refreshTable: Subject<void> = new Subject<void>();

  userRole: string

  constructor(public dialog: MatDialog,
    private _auth: AuthenticationService,
    private _api: ApiService,
    private errorD: ErrorDialogService) {
    _auth.currentUserRole.subscribe(x => {
      this.userRole = x;
    })
  }

  dbClickHandler(data: LetterModel) {
    this.dialog.open(this.detailsComponent, {
      maxHeight: '96vh',
      height: 'auto',
      width: '90vw',
      data: data
    }).afterClosed().subscribe(x => {
      if (x && x.op === "remove")
        this.refreshTable.next();
    });
  }

  add() {
    this.dialog.open(LettersCreateComponent, {
      maxHeight: '96vh',
      height: 'auto',
      width: '90vw',
    }).afterClosed().subscribe(x => {
      if (x && x.op === "create")
        this.refreshTable.next();
    });
  }
  delete() {
    if (this.selection.selected.length === 0)
      return;

    this.dialog.open(AskIfDialog, {
      disableClose: true,
      data: { data: "Czy na pewno chesz usunąć wybrane dokumenty ?", close: "Nie", ok: "Tak" }
    }).afterClosed().subscribe(result => {
      if (!result)
        return;
      let counter = this.selection.selected.length;
      const errorArray = [];
      for (const item of this.selection.selected) {
        this._api.delete("/api/documentpanel/letter/" + item.id).toPromise().then(() => {
          counter--
          if (counter <= 0) {
            if (errorArray.length === 0)
              this.errorD.showAlert(new AlertData("Poprawnie usunięto", "Poprawnie usunięto " + this.selection.selected.length + " dokumentów"));
            else if (errorArray.length !== 0)
              errorArray.forEach(e => this.errorD.showDialog(e))
            this.refreshTable.next();
          }

        }).catch((x: HttpErrorResponse) => {
          counter--
          errorArray.push(x);
          if (counter <= 0) {
            if (errorArray.length === 0)
              this.errorD.showAlert(new AlertData("Poprawnie usunięto", "Poprawnie usunięto " + this.selection.selected.length + " dokumentów"));
            else if (errorArray.length !== 0)
              errorArray.forEach(e => this.errorD.showDialog(e))
            this.refreshTable.next();
          }
        })
      }
    })

  }
}
