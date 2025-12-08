import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ApiService } from '../../shared/services/api.service';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';
import { UserDetails } from '../userdetails.model';
import { UserChangeEmail } from '../user-change-email.model';
import { HttpErrorResponse } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { AlertData } from '../../alert/alert.component';

@Component({
    selector: 'app-user-email-change',
    templateUrl: './user-email-change.component.html',
    styleUrls: ['./user-email-change.component.css']
})
/** user-email-change component*/
export class UserEmailChangeComponent {
  /** user-email-change ctor */
  change: FormGroup
  changeEmail: UserChangeEmail = new UserChangeEmail({})

  constructor(private _api: ApiService,
    private dialogRef: MatDialogRef<UserDetails>,
    private errorDialog: ErrorDialogService,
    private dialog: MatDialog,
    private formBuilder: FormBuilder,
    @Inject(MAT_DIALOG_DATA) public data: UserDetails) {
    this.change = formBuilder.group({
      password: [this.changeEmail.password],
      email: [this.changeEmail.email]
    })
  }
  onSubmit() {
    const rawData = this.change.value;
    const data = new UserChangeEmail(rawData);
    data.url = environment.urlAddress + "/user/emailchange";
    this._api.post("/api/user/chemail", data).toPromise().then(() => {
      this.errorDialog.showAlert(new AlertData("Wysłano email", "Poprawnie wysłano email na adres " + data.email));
      this.dialogRef.close({ op: "sendedEmail", data: this.data });
    }).catch((x: HttpErrorResponse) => {
      this.errorDialog.showDialog(x);
    })
  }
}
