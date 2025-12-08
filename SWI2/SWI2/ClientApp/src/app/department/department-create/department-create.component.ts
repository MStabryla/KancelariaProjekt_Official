import { Component,Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA} from '@angular/material/dialog';
import { SWIError } from '../../errors/error';
import { ApiService } from '../../shared/services/api.service';
import { Department } from '../department.model';
import { HttpErrorResponse } from '@angular/common/http';
import { ErrorDialogComponent } from '../../errors/error-dialog/error-dialog.component';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';

@Component({
    selector: 'app-department-create',
    templateUrl: './department-create.component.html',
    styleUrls: ['./department-create.component.css']
})
/** department-create component*/
export class DepartmentCreateComponent {
  createFormGroup: FormGroup;

  constructor(private _api: ApiService,
    private dialogRef: MatDialogRef<Department>,
    formBuilder: FormBuilder,
    private dialog: MatDialog,
    private errorD: ErrorDialogService,
    @Inject(MAT_DIALOG_DATA) public data: string) {
    this.createFormGroup = formBuilder.group({
      name: ['', Validators.required],
      type: ['1', Validators.required],
      folderName: ['', Validators.required]
    })
  }

  onSubmit() {
    if (!this.createFormGroup.valid)
      return;
    const rawData = this.createFormGroup.value;
    const data = new Department(rawData);
    this._api.post("/api/departmentpanel/" + this.data, data).toPromise().then(() => {
      this.dialogRef.close({ op: "create", data: data });
    }).catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    });
  }
}
