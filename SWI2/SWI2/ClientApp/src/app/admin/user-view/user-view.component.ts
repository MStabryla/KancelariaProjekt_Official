import { HttpErrorResponse } from '@angular/common/http';
import { Component, Inject } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { AlertData } from '../../alert/alert.component';
import { ApiService } from '../../shared/services/api.service';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';
import { ChangeRoleComponent } from '../change-role/change-role.component';
import { GiveAccessClientComponent } from '../give-access-client/give-access-client.component';
import { GiveAccessEmployeeComponent } from '../give-access-employee/give-access-employee.component';
import { RemoveAccessClientComponent } from '../remove-access-client/remove-access-client.component';
import { RemoveAccessEmployeeComponent } from '../remove-access-employee/remove-access-employee.component';
import { UserModel } from '../user.model';

@Component({
    selector: 'swi-user-view',
    templateUrl: './user-view.component.html',
    styleUrls: ['./user-view.component.css']
})
/** user-view component*/
export class UserViewComponent {
    /** user-view ctor */
  constructor(public _api: ApiService,
    @Inject(MAT_DIALOG_DATA) public data: UserModel,
    private dialogRef: MatDialogRef<UserModel>,
    private errorD: ErrorDialogService,
    public dialog: MatDialog) {

  }

  giveAccess() {
    if (this.data.userRole === 'Employee') {
      this.dialog.open(GiveAccessEmployeeComponent, {
        height: 'auto',
        width: 'auto',
        maxHeight: '75vh',
        data: this.data
      }).afterClosed().subscribe(res => {
        if (res && res.op === "ok") {
          this.errorD.showAlert(new AlertData("Zmieniono uprawnienia", "Poprawnie zmieniono uprawnienia dostepu użytkownikowi " + this.data.userName));
        }
        else if (res && res.op === "error") {
          res.data.forEach((x: HttpErrorResponse) => {
            this.errorD.showDialog(x);
          })
        }
      })
    }
    else if (this.data.userRole === 'Client') {
      this.dialog.open(GiveAccessClientComponent, {
        height: 'auto',
        width: 'auto',
        maxHeight: '75vh',
        data: this.data
      }).afterClosed().subscribe(res => {
        if (res && res.op === "ok") {
          this.errorD.showAlert(new AlertData("Zmieniono uprawnienia", "Poprawnie zmieniono uprawnienia dostepu użytkownikowi " + this.data.userName));
        }
        else if (res && res.op === "error") {
          res.data.forEach((x: HttpErrorResponse) => {
            this.errorD.showDialog(x);
          })
        }
      })
    }
  }
  removeAccess() {
    if (this.data.userRole === 'Employee') {
      this.dialog.open(RemoveAccessEmployeeComponent, {
        height: 'auto',
        width: 'auto',
        maxHeight: '75vh',
        data: this.data
      }).afterClosed().subscribe(res => {
        if (res && res.op === "ok") {
          this.errorD.showAlert(new AlertData("Zmieniono uprawnienia", "Poprawnie zmieniono uprawnienia dostepu użytkownikowi " + this.data.userName));
        }
        else if (res && res.op === "error") {
          res.data.forEach((x: HttpErrorResponse) => {
            this.errorD.showDialog(x);
          })
        }
      })
    }
    else if (this.data.userRole === 'Client') {
      this.dialog.open(RemoveAccessClientComponent, {
        height: 'auto',
        width: 'auto',
        maxHeight: '75vh',
        data: this.data
      }).afterClosed().subscribe(res => {
        if (res && res.op === "ok") {
          this.errorD.showAlert(new AlertData("Zmieniono uprawnienia", "Poprawnie zmieniono uprawnienia dostepu użytkownikowi " + this.data.userName));
        }
        else if (res && res.op === "error") {
          res.data.forEach((x: HttpErrorResponse) => {
            this.errorD.showDialog(x);
          })
        }
      })
    }
  }
  blockUser() {
    this._api.post("api/adminpanel/" + this.data.id + "/lock", null).toPromise().then(() => {
      this.errorD.showAlert(new AlertData("Zablokowano użytkownika", "Użytkownik " + this.data.userName + " został pomyslnie zablokowany"));
      this.data.isActive = false;
    }).catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    })
  }
  unlockUser() {
    this._api.post("api/adminpanel/" + this.data.id + "/activate", null).toPromise().then(() => {
      this.errorD.showAlert(new AlertData("Odblokowano użytkownika", "Użytkownik " + this.data.userName + " został pomyslnie odblokowany"));
      this.data.isActive = true;
    }).catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    })
  }

  checkChangeRole(userRole: string): boolean {
    const forbiddenRoles = ["Administrator", "Employee", "Client"];
    return !forbiddenRoles.includes(userRole);
  }

  changeRole() {
    this.dialog.open(ChangeRoleComponent, {
      height: 'auto',
      width: 'auto',
      maxHeight: '75vh',
      data: this.data
    }).afterClosed().subscribe(res => {
    })
  }
}
