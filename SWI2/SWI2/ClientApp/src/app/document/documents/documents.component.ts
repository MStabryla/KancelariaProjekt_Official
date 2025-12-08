import { SelectionModel } from '@angular/cdk/collections';
import { HttpErrorResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { Subject } from 'rxjs';
import { AlertData } from '../../alert/alert.component';
import { FormModel } from '../../models/Form.model';
import { AskIfDialog } from '../../shared/components/askIfDialog/askIfDialog.component';
import { ApiService } from '../../shared/services/api.service';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';
import { DocumentCreateComponent } from '../document-create/document-create.component';
import { DocumentViewComponent } from '../document-view/document-view.component';
import { DocumentModel } from '../document.model';

@Component({
    selector: 'swi-documents',
    templateUrl: './documents.component.html',
    styleUrls: ['./documents.component.css']
})
/** documents component*/
export class DocumentsComponent {
  sortActive = "created";
  requestUrl = "api/documentpanel/document/";
  detailsComponent = DocumentViewComponent;
  headerRow = [
    { description: "DOCUMENTS.DOCUMENTS.TABLE_OUTDOCUMENT_DESCRIPTION", name: "outDocument", type: "boolean", ifRange: false, boolFilter: (v) => v ? "od" : "do" } as FormModel,
    { description: "DOCUMENTS.DOCUMENTS.TABLE_NOTES_DESCRIPTION", name: "notes", type: "text", ifRange: false } as FormModel,
    { description: "DOCUMENTS.DOCUMENTS.TABLE_COMMENT_DESCRIPTION", name: "comment", type: "text", ifRange: false } as FormModel,
    { description: "DOCUMENTS.DOCUMENTS.TABLE_DOCUMENT_TYPE_DESCRIPTION", name: "documentType", type: "text", ifRange: false } as FormModel,
    { description: "DOCUMENTS.DOCUMENTS.TABLE_COMPANY_DESCRIPTION", name: "companyName", type: "text", ifRange: false } as FormModel,
    { description: "DOCUMENTS.DOCUMENTS.TABLE_HAS_DOCUMENT_FILE_DESCRIPTION", name: "hasDocumentFile", type: "boolean", ifRange: false, boolFilter: (v) => v ? "tak" : "nie" } as FormModel,
    { description: "DOCUMENTS.DOCUMENTS.TABLE_IS_PROTOCOL_DESCRIPTION", name: "isProtocol", type: "boolean", ifRange: false, boolFilter: (v) => v ? "tak" : "nie" } as FormModel,
    { description: "CREATION_DATE", name: "created", type: "date", ifRange: true } as FormModel
  ]
  validators = [Validators.nullValidator, Validators.nullValidator, Validators.nullValidator, Validators.nullValidator, Validators.nullValidator];
  selection = new SelectionModel<DocumentModel>(true, []);
  refreshTable: Subject<void> = new Subject<void>();

  userRole: string;
  /** documents ctor */
  constructor(public dialog: MatDialog,
    private _auth: AuthenticationService,
    private _api: ApiService,
    private errorD: ErrorDialogService) {
    _auth.currentUserRole.subscribe(x => {
      this.userRole = x;
    })
  }

  dbClickHandler(data: DocumentModel) {
    this.dialog.open(this.detailsComponent, {
      maxHeight: '96vh',
      height: 'auto',
      width: '90vw',
      data: data
    }).afterClosed().subscribe(x => {
      if (x && x.op === "remove")
        this.refreshTable.next();
      this.selection.clear();
    });
  }

  add() {
    this.dialog.open(DocumentCreateComponent, {
      maxHeight: '96vh',
      height: 'auto',
      width: '90vw',
    }).afterClosed().subscribe(x => {
      if (x && x.op === "create")
        this.refreshTable.next();
      this.selection.clear();
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
        this._api.delete("/api/documentpanel/document/" + item.id).toPromise().then(() => {
          counter--
          if (counter <= 0) {
            if(errorArray.length === 0)
              this.errorD.showAlert(new AlertData("Poprawnie usunięto", "Poprawnie usunięto " + this.selection.selected.length + " dokumentów"));
            else if (errorArray.length !== 0)
              errorArray.forEach(e => this.errorD.showDialog(e))
            this.refreshTable.next();
            this.selection.clear();
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
            this.selection.clear();
          }
        })
      }
    })
    
  }
}
