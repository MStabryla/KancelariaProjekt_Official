import { HttpErrorResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { UserRegistration } from '../../models/authentication/register-user/userRegistration.model';
import { ApiService } from '../../shared/services/api.service';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';
import { UserModel } from '../user.model';

@Component({
  selector: 'swi-user-add',
  templateUrl: './user-add.component.html',
  styleUrls: ['./user-add.component.css']
})
/** user-add component*/
export class UserAddComponent {
  createFormGroup: FormGroup;
  user: UserRegistration;
  /** user-add ctor */
  constructor(private _api: ApiService,
    private formBuilder: FormBuilder,
    private dialogRef: MatDialogRef<object>,
    private dialog: MatDialog,
    private errorD: ErrorDialogService) {
    this.createFormGroup = formBuilder.group({
      name: ['', Validators.required],
      surname: ['', Validators.required],
      login: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
      confirmPassword: ['', Validators.required],
      language: ['', [Validators.required, Validators.minLength(2)]],
    })
    
  }


  onSubmit() {
    if (this.createFormGroup.valid && this.createFormGroup.value.password === this.createFormGroup.value.confirmPassword) {
      const data = this.createFormGroup.value;
      this._api.post("/api/adminpanel", data).toPromise().then(() => {
        this.dialogRef.close({ op: "create", data: data });
      }).catch((x: HttpErrorResponse) => {
        this.errorD.showDialog(x);
      });
    }
  }
}
