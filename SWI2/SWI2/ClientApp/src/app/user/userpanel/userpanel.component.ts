import { HttpErrorResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute } from '@angular/router';
import { AlertData } from '../../alert/alert.component';
import { ApiService } from '../../shared/services/api.service';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';
import { UserChangePasswordComponent } from '../user-change-password/user-change-password.component';
import { UserEditComponent } from '../user-edit/user-edit.component';
import { UserEmailChangeComponent } from '../user-email-change/user-email-change.component';
import { UserDetails } from '../userdetails.model';

@Component({
  selector: 'app-userpanel',
  templateUrl: './userpanel.component.html',
  styleUrls: ['./userpanel.component.css']
})
/** userpanel component*/
export class UserpanelComponent {

  userUrl = "/api/user";
  UserDetails = new UserDetails({});
  //private errorD = new ErrorDialogService(this.dialog);

  constructor(
    private api: ApiService,
    public errorD: ErrorDialogService,
    private authService: AuthenticationService,
    public dialog: MatDialog,
    private route: ActivatedRoute) {
    this.refresh()
    route.url.subscribe(url => {
      if (url.length > 1) {
        this.confirmChangeEmail();
      }
    })
  }

  ngOnInit() {
    //this.refresh();
  }

  refresh() {
    this.api.get(this.userUrl).toPromise().then(x => {
      this.UserDetails = new UserDetails(x);
      this.authService.currentUserName.toPromise().then(userName => this.UserDetails.login = userName);
    }).catch(x => this.errorD.showDialog(x))
    //this.api.get(this.userUrl).toPromise().then((data: unknown) => {
    //  this.UserDetails = new UserDetails(data);
    //  this.authService.currentUserName.subscribe(x => {
    //    this.UserDetails.login = x;
    //  })
    //}).catch(x => this.errorD.showDialog(x))
  }

  forceError() {
    this.api.get("/api/companypanel/-1").toPromise().then(() => {
    }).catch(x => this.errorD.showDialog(x))
  }
  editData() {
    this.dialog.open(UserEditComponent, {
      height: 'auto',
      width: 'auto',
      maxHeight: '75vh',
      data: this.UserDetails
    }).afterClosed().subscribe(res => {
      this.refresh();
    })
  }
  changePassword() {
    this.dialog.open(UserChangePasswordComponent, {
      height: 'auto',
      width: 'auto',
      maxHeight: '75vh',
      data: this.UserDetails
    }).afterClosed().subscribe(res => {

    })
  }
  changeEmail() {
    this.dialog.open(UserEmailChangeComponent, {
      height: 'auto',
      width: 'auto',
      maxHeight: '75vh',
      data: this.UserDetails
    }).afterClosed().subscribe(res => {

    })
  }
  confirmChangeEmail() {
    this.route.queryParams.subscribe(params => {
      if (params.token && params.email) {
        this.api.post("/api/user/conemail", { token: params.token, email: params.email }).toPromise().then(() => {
          this.errorD.showAlert(new AlertData("USER_PANEL.USER_PANEL.EMAIL_CHANGED_TITLE_SUCCES", "USER_PANEL.USER_PANEL.EMAIL_CHANGED_CONTENT_SUCCES", { email: params.email }))
        }).catch(x => {
          this.errorD.showDialog(x)
        })
      }
    })
  }

}
