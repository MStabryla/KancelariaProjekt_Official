import { SelectionModel } from '@angular/cdk/collections';
import { Component } from '@angular/core';
import { Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { Subject } from 'rxjs';
import { FormModel } from '../../models/Form.model';
import { UserAddComponent } from '../user-add/user-add.component';
import { UserViewComponent } from '../user-view/user-view.component';

@Component({
    selector: 'swi-users',
    templateUrl: './users.component.html',
    styleUrls: ['./users.component.css']
})
/** users component*/
export class UsersComponent {
  sortActive = "created";
  requestUrl = "api/adminpanel/users";
  detailsComponent = UserViewComponent;
  headerRow = [
    { description: "ADMIN.USERS.SELECT_COMPANYS.TABLE_ID_DESCRIPTION", name: "id", type: "text", ifRange: false } as FormModel,
    { description: "ADMIN.USERS.SELECT_COMPANYS.TABLE_USERNAME_DESCRIPTION", name: "userName", type: "text", ifRange: false } as FormModel,
    { description: "ADMIN.USERS.SELECT_COMPANYS.TABLE_ROLE_DESCRIPTION", name: "userRole", type: "role", ifRange: false } as FormModel,
    { description: "ADMIN.USERS.SELECT_COMPANYS.TABLE_PHONENUMBER_DESCRIPTION", name: "phoneNumber", type: "text", ifRange: false } as FormModel,
    { description: "ADMIN.USERS.SELECT_COMPANYS.TABLE_EMAIL_DESCRIPTION", name: "email", type: "text", ifRange: false } as FormModel,
    { description: "ADMIN.USERS.SELECT_COMPANYS.TABLE_CREATED_DESCRIPTION", name: "created", type: "date", ifRange: true } as FormModel
  ];
  validators = [Validators.nullValidator, Validators.nullValidator, Validators.nullValidator, Validators.nullValidator, Validators.email, Validators.nullValidator, Validators.nullValidator];
  selection = new SelectionModel<any>(true, []);
  refreshTable: Subject<void> = new Subject<void>();

  constructor(public dialog: MatDialog) {

  }
  dbClickHandler(data: any) {
    this.dialog.open(this.detailsComponent, {
      height: 'auto',
      width: 'auto',
      maxHeight: '75vh',
      data: data
    }).afterClosed().subscribe(x => {
      
    });
  }
  add() {
    this.dialog.open(UserAddComponent, {
      height: 'auto',
      width: 'auto',
      maxHeight: '75vh'
    }).afterClosed().subscribe(x => {
    });
  }
}
