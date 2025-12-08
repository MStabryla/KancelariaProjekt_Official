import { HttpErrorResponse } from '@angular/common/http';
import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ApiService } from '../../shared/services/api.service';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';
import { LetterModel } from '../letter.model';

@Component({
    selector: 'swi-letters-view',
    templateUrl: './letters-view.component.html',
    styleUrls: ['./letters-view.component.css']
})
/** letters-view component*/
export class LettersViewComponent {
  userRole: string;
  checkIfFileExist = false;

  constructor(@Inject(MAT_DIALOG_DATA) public data: LetterModel,
    private _api: ApiService,
    private dialogRef: MatDialogRef<LetterModel>,
    private errorD: ErrorDialogService,
    private _auth: AuthenticationService) {
    _auth.currentUserRole.subscribe(x => {
      this.userRole = x;
    })
    this.checkIfFileExist = this.data.hasLetterFile;
    this._api.get("api/documentpanel/letter/" + data.id).toPromise().then(data => {
      this.data = new LetterModel(data);
    })
  }
  download() {
    window.open("/api/documentpanel/letter/" + this.data.id + "/download?&access_token=" + localStorage.getItem("jwt"));
  }

  delete() {
    this._api.delete("/api/documentpanel/letter/" + this.data.id).toPromise().then(() => {
      this.dialogRef.close({ op: "remove", data: this.data });
    }).catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    });
  }
}
