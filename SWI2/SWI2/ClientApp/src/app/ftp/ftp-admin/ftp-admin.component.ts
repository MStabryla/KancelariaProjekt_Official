import { HttpErrorResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { ApiService } from '../../shared/services/api.service';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';
import { FileModel } from '../file.model';

@Component({
  selector: 'swi-ftp-admin',
  templateUrl: './ftp-admin.component.html',
  styleUrls: ['./ftp-admin.component.css']
})
/** ftp-admin component*/
export class FtpAdminComponent {
  users: FileModel;
  companies: FileModel;

  public userSearch: string;
  public companiesSearch: string;

  public userSearchFiles: FileModel[];
  public companySearchFiles: FileModel[];

  public userRole: string;

  /** ftp-view ctor */
  constructor(private _auth: AuthenticationService,
    private _api: ApiService,
    private errorD: ErrorDialogService) {
    this.users = new FileModel({ name: "Użytkownicy", path: "/users/", type: 1 });
    this.companies = new FileModel({ name: "Firmy", path: "/companies/", type: 1 });

    this._auth.currentUserRole.subscribe((role) => {
      this.userRole = role;
      //if (this.userRole !== "Administrator")
      //  console.log("redirect")
    })

  }

  onKeyPressEvent(query: string, event: KeyboardEvent, path: string) {
    if (event.keyCode === 13) {
      this.confirmSearch(query, path)
    }
  }

  confirmSearch(query: string, path: string) {
    this._api.get<FileModel[]>("/api/filepanel/search?path=" + btoa(path) + "&query=" + btoa(query)).toPromise().then((data: any) => {
      if (path.includes("users")) {
        this.userSearchFiles = this.userSearchFiles = data.childFiles.map((x: unknown) => {
          return new FileModel(x);
        }).sort((a) => a.type === 'dir' ? -1 : 1);
      }
      else if (path.includes("companies")) {
        this.companySearchFiles = this.userSearchFiles = data.childFiles.map((x: unknown) => {
          return new FileModel(x);
        }).sort((a) => a.type === 'dir' ? -1 : 1);
      }
    }).catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
      return [];
    })
  }

  cancelSearch(path: string) {
    if(path.includes("users")) {
      this.userSearchFiles = null;
      this.userSearch = "";
    }
    else if (path.includes("companies")) {
      this.companySearchFiles = null;
      this.companiesSearch = "";
    }
  }
}
