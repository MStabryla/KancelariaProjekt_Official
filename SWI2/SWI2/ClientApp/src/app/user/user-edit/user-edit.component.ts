import { HttpErrorResponse } from '@angular/common/http';
import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ApiService } from '../../shared/services/api.service';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';
import { UserDetails } from '../userdetails.model';

@Component({
  selector: 'app-user-edit',
  templateUrl: './user-edit.component.html',
  styleUrls: ['./user-edit.component.css']
})
/** user-edit component*/
export class UserEditComponent {
  /** user-edit ctor */
  public editFG: FormGroup;
  constructor(private _api: ApiService,
    private dialogRef: MatDialogRef<UserDetails>,
    private dialog: ErrorDialogService,
    private formBuilder: FormBuilder,
    @Inject(MAT_DIALOG_DATA) public data: UserDetails)
  {
    this.editFG = formBuilder.group({
      name: [data.name],
      surname: [data.surname],
      folderName: [data.folderName]
    });
  }
  onSubmit() {
    const rawData = this.editFG.value;
    const data = new UserDetails(rawData);
    this.data = data;
    this._api.put("/api/user", data).toPromise().then(() => {
      this.dialogRef.close({ op: "edit", data: this.data });
    }).catch((x: HttpErrorResponse) => {
      this.dialog.showDialog(x);
    })
  }
}
