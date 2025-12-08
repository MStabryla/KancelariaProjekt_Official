import { SelectionModel } from '@angular/cdk/collections';
import { Component } from '@angular/core';
import { Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { Subject } from 'rxjs';
import { FormModel } from '../../models/Form.model';
import { DepartmentViewComponent } from '../department-view/department-view.component';

@Component({
    selector: 'swi-departments',
    templateUrl: './departments.component.html',
    styleUrls: ['./departments.component.css']
})
/** departments component*/
export class DepartmentsComponent {
  sortActive = "created";
  requestUrl = "api/departmentpanel/company/{id}";
  detailsComponent = DepartmentViewComponent;
  headerRow = [
    { description: "NAME", name: "name", type: "text", ifRange: false } as FormModel,
    { description: "DEPARTMENTS.DEPARTMENTS.TABLE_TYPE_DESCRIPTION", name: "type", type: "number", ifRange: false } as FormModel,
    { description: "DEPARTMENTS.DEPARTMENTS.TABLE_FOLDER_NAME_DESCRIPTION", name: "folderName", type: "text", ifRange: false } as FormModel,
    { description: "CREATION_DATE", name: "created", type: "date", ifRange: true } as FormModel
  ];
  validators = [Validators.nullValidator, Validators.nullValidator, Validators.nullValidator, Validators.nullValidator];
  selection = new SelectionModel<any>(true, []);
  refreshTable: Subject<void> = new Subject<void>();
    /** departments ctor */
  constructor(public dialog: MatDialog) {

  }
  dbClickHandler(data: any) {
    //this._router.navigateByUrl(this.detailsComponent + "/" + data["id"]);

    this.dialog.open(this.detailsComponent, {
      maxHeight: '96vh',
      height: 'auto',
      width: '90vw',
      data: data
    }).afterClosed().subscribe(x => {
    });
  }
}
