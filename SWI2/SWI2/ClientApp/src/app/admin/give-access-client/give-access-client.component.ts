import { SelectionModel } from '@angular/cdk/collections';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, Inject } from '@angular/core';
import { Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Subject } from 'rxjs';
import { Company } from '../../company/company.model';
import { FormModel } from '../../models/Form.model';
import { ApiService } from '../../shared/services/api.service';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';
import { UserModel } from '../user.model';

@Component({
    selector: 'swi-give-access-client',
    templateUrl: './give-access-client.component.html',
    styleUrls: ['./give-access-client.component.css']
})
export class GiveAccessClientComponent {

  sortActive = "created";
  requestUrl = "api/companypanel/";
  detailsComponent = null;
  headerRow = [
    { description: "ADMIN.GIVE_ACCESS_CLIENT.COMPANYS_TABLE_NAME_DESCRIPTION", name: "name", type: "text", ifRange: false } as FormModel,
  ];
  validators = [Validators.nullValidator];
  selection = new SelectionModel<any>(true, []);
  refreshTable: Subject<void> = new Subject<void>();

  isLoading = false;
  howManyQueries: number;

    /** give-access-client ctor */
  constructor(private _api: ApiService,
    private errorD: ErrorDialogService,
    private dialogRef: MatDialogRef<UserModel>,
    @Inject(MAT_DIALOG_DATA) public data: UserModel) {

  }

  dbClickHandler(data: any) {
  }

  confirm() {
    if (this.selection.selected.length > 0)
      this.howManyQueries = this.selection.selected.length;
    const errorArray = [];
    this.isLoading = true;
    this.selection.selected.forEach((x) => {
      this._api.post("api/clientpanel/" + this.data.id + "/" + x.id, null).toPromise().then((x) => {
        this.howManyQueries--;
        if (this.howManyQueries <= 0) {
          this.isLoading = false;
          if (errorArray.length === 0) {
            this.dialogRef.close({ op: "ok", data: this.selection.selected })
          }
          else {
            this.dialogRef.close({ op: "error", data: errorArray})
          }
        }
      }).catch((x: HttpErrorResponse) => {
        errorArray.push(x);
        this.howManyQueries--;
        if (this.howManyQueries <= 0) {
          this.isLoading = false;
          if (errorArray.length === 0) {
            this.dialogRef.close({ op: "ok", data: this.selection.selected })
          }
          else {
            this.dialogRef.close({ op: "error", data: errorArray })
          }
        }
      })
    })
  }
}
