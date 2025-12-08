import { Component } from '@angular/core';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { FileModel } from '../file.model';

@Component({
    selector: 'swi-ftp-view',
    templateUrl: './ftp-view.component.html',
    styleUrls: ['./ftp-view.component.css']
})
/** ftp-view component*/
export class FtpViewComponent {
  homeModel: FileModel;
  companies: FileModel[];

  public userRole: string;
  /** ftp-view ctor */
  constructor(private _auth: AuthenticationService) {
    this.homeModel = new FileModel({ name: "Home", path: "home", type: 1 });
    this._auth.currentUserRole.subscribe((role) => {
      this.userRole = role;
    })
    this._auth.currentUserCompanys.subscribe((companies: object[]) => {
      this.companies = companies.map((x: unknown) => {
        return new FileModel({ name: x["name"], path: "/companies/" + x["id"] + (x["name"] ? "-" + x["name"] : ""), type: 1})
      });
    })
  }
}
