import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ApiService } from '../../shared/services/api.service';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';
import { UserDetails } from '../userdetails.model';
import { UserChangePassword } from "../user-change-password.model"
import { HttpErrorResponse } from '@angular/common/http';
import { AlertData } from '../../alert/alert.component';

@Component({
  selector: 'app-user-change-password',
  templateUrl: './user-change-password.component.html',
  styleUrls: ['./user-change-password.component.css']
})
/** user-email-change component*/
export class UserChangePasswordComponent {
  /** user-email-change ctor */
  change: FormGroup;
  changePassword: UserChangePassword = new UserChangePassword({});
  constructor(private _api: ApiService,
    private dialogRef: MatDialogRef<UserDetails>,
    private dialog: ErrorDialogService,
    private formBuilder: FormBuilder,
    @Inject(MAT_DIALOG_DATA) public data: UserDetails) {
    this.change = formBuilder.group({
      previousPassword: [this.changePassword.previousPassword, Validators.required],
      password: [this.changePassword.password, [Validators.required/*, this.passwordValidator()*/]],
      confirmPassword: [this.changePassword.confirmPassword, [Validators.required/*, this.passwordValidator()*/]]
    });
  }
  passwordValidator(): ValidatorFn {
    return (group: FormGroup): ValidationErrors => {
      if (group.parent === null || group.parent.controls === undefined) {
        return { notEquivalent: true };
      }
      const passwd = group.parent.controls["password"];
      const checkPasswd = group.parent.controls["confirmPassword"];
      if (passwd === null || checkPasswd === null) {
        return { notEquivalent: true };
      }
      else if (passwd.value !== checkPasswd.value) {
        checkPasswd.setErrors({ notEquivalent: true });
        return { notEquivalent: true };
      }
      return { };
    }
  }
  onSubmit() {
    if (!this.change.valid)
      return;
    const rawData = this.change.value;
    const data = new UserChangePassword(rawData);
    this._api.post("/api/user/chpassword", data).toPromise().then(() => {
      this.dialog.showAlert(new AlertData("Success", "Password changed"));
      this.dialogRef.close({ op: "changedPassword", data: this.data });
    }).catch((x: HttpErrorResponse) => {
      this.dialog.showDialog(x);
    })
  }
}
