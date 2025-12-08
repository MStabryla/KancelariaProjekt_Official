import { HttpErrorResponse } from '@angular/common/http';
import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ApiService } from '../../shared/services/api.service';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';
import { DocumentModel } from '../document.model';

@Component({
  selector: 'swi-document-view',
  templateUrl: './document-view.component.html',
  styleUrls: ['./document-view.component.css']
})
/** document-view component*/
export class DocumentViewComponent {
  userRole: string;
  checkIfFileExist = false;

  constructor(@Inject(MAT_DIALOG_DATA) public data: DocumentModel,
    private _api: ApiService,
    private dialogRef: MatDialogRef<DocumentModel>,
    private errorD: ErrorDialogService,
    private _auth: AuthenticationService) {
    _auth.currentUserRole.subscribe(x => {
      this.userRole = x;
    })
    this.checkIfFileExist = this.data.hasDocumentFile;
    this._api.get("api/documentpanel/document/" + data.id).toPromise().then(data => {
      this.data = new DocumentModel(data);
    })
  }

  download() {
    window.open("/api/documentpanel/document/" + this.data.id + "/download?&access_token=" + localStorage.getItem("jwt"));
  }

  delete() {
    this._api.delete("/api/documentpanel/document/" + this.data.id).toPromise().then(() => {
      this.dialogRef.close({ op: "remove", data: this.data });
    }).catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    });
  }
}
