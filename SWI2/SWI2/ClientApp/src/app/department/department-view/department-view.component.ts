import { Component, Inject } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ApiService } from '../../shared/services/api.service';
import { DepartmentEditComponent } from '../department-edit/department-edit.component';
import { Department } from '../department.model';
import { SWIError } from '../../errors/error';
import { HttpErrorResponse } from '@angular/common/http';
import { ErrorDialogComponent } from '../../errors/error-dialog/error-dialog.component';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';

@Component({
    selector: 'app-department-view',
    templateUrl: './department-view.component.html',
    styleUrls: ['./department-view.component.css']
})
/** department-view component*/
export class DepartmentViewComponent {
  /** department-view ctor */
  constructor(
    public _api: ApiService,
    @Inject(MAT_DIALOG_DATA) public data: Department,
    private dialogRef: MatDialogRef<Department>,
    public dialog: MatDialog,
    public errorD: ErrorDialogService) {

  }

  edit() {
    this.dialog.open(DepartmentEditComponent, {
      height: 'auto',
      width: 'auto',
      maxHeight: '50vh',
      data: this.data
    }).afterClosed().subscribe(x => {
      if (x && x.op === "edit")
        this.data = x.data;
    })
  }

  delete() {
    this._api.delete("api/departmentpanel/" + this.data.id).toPromise().then(() => {
      this.dialogRef.close({ op: "remove", data: this.data });
    }).catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    });
  }
}
