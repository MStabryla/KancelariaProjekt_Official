import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { SWIError } from '../../errors/error';
import { ApiService } from '../../shared/services/api.service';
import { Department } from '../department.model';
import { HttpErrorResponse } from '@angular/common/http';
import { ErrorDialogComponent } from '../../errors/error-dialog/error-dialog.component';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';

@Component({
    selector: 'app-department-edit',
    templateUrl: './department-edit.component.html',
    styleUrls: ['./department-edit.component.css']
})
/** department-edit component*/
export class DepartmentEditComponent {

  editForgGroup: FormGroup;

  constructor(private _api: ApiService,
    private dialogRef: MatDialogRef<Department>,
    private formBuilder: FormBuilder,
    private dialog: MatDialog,
    private errorD: ErrorDialogService,
    @Inject(MAT_DIALOG_DATA) public data: Department) {
    this.editForgGroup = formBuilder.group({
      name: [data.name, Validators.required],
      type: [data.type, Validators.required],
      folderName: [data.folderName, Validators.required]
    })
  }

  onSubmit() {
    const rawData = this.editForgGroup.value;
    const data = new Department(rawData);
    const id = this.data.id;
    this.data = data;
    this._api.put("/api/departmentpanel/" + id, data).toPromise().then(() => {
      this.dialogRef.close({ op: "edit", data: this.data });
    }).catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    });
  }
}
