import { HttpErrorResponse } from '@angular/common/http';
import { Component, Inject } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { AlertData } from '../../alert/alert.component';
import { ApiService } from '../../shared/services/api.service';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';
import { UserModel } from '../user.model';

@Component({
    selector: 'swi-change-role',
    templateUrl: './change-role.component.html',
    styleUrls: ['./change-role.component.css']
})
/** change-role component*/
export class ChangeRoleComponent {
  /** change-role ctor */
  constructor(private _api: ApiService,
    @Inject(MAT_DIALOG_DATA) public data: UserModel,
    private dialogRef: MatDialogRef<UserModel>,
    private errorD: ErrorDialogService,
    public dialog: MatDialog) {

  }

  submit(newRole: string) {
    const urlMap = new Map<string, string>([
      ["Client", "/api/clientpanel/"],
      ["Employee", "/api/employeepanel/"]
    ]);

    const url = urlMap.get(newRole)  + this.data.id;

    this._api.post(url, null).toPromise().then((data) => {
      this.errorD.showAlert(new AlertData("Zmieniono rolę", "Poprawnie zmieniono rolę użytkownika" + this.data.userName + " na " + newRole));
      this.data.userRole = newRole;
      this.dialogRef.close();
    }).catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
      this.dialogRef.close();
    })
  }
}
