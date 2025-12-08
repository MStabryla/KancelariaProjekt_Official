import { Component } from '@angular/core';
import { Validators } from '@angular/forms';
import { SelectionModel } from "@angular/cdk/collections";
import { FormModel } from '../../models/Form.model';
import { MatDialog } from '@angular/material/dialog';
import { CompanyComponent } from '../company/company.component';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { CompanyCreateComponent } from '../company-create/company-create.component';

@Component({
    selector: 'swi-companies',
    templateUrl: './companies.component.html',
    styleUrls: ['./companies.component.css']
})
/** companies component*/
export class CompaniesComponent {
    sortActive = "created";
    requestUrl = "api/companypanel/";
    detailsComponent = 'company';
    headerRow = [
      { description: "COMPANY.COMPANIES.TABLE_NAME_DESCRIPTION", name: "name", type: "text", ifRange: false } as FormModel,
      { description: "COMPANY.COMPANIES.TABLE_NIP_DESCRIPTION", name: "nip", type: "text", ifRange: false } as FormModel,
      { description: "COMPANY.COMPANIES.TABLE_ADRESS_INPUT_DESCRIPTION", name: "adress-input", type: "adress-input", ifRange: false } as FormModel,
      { description: "COMPANY.COMPANIES.TABLE_CODECOUNTRY_INPUT_DESCRIPTION", name: "codeCoutry-input", type: "codeCoutry-input", ifRange: false } as FormModel,
      { description: "COMPANY.COMPANIES.TABLE_DEFAULTWNAACCOUNT_DESCRIPTION", name: "defaultWNAAccount", type: "text", ifRange: false } as FormModel,
      { description: "COMPANY.COMPANIES.TABLE_DEFAULTMAACCOUNT_DESCRIPTION", name: "defaultMAAccount", type: "text", ifRange: false } as FormModel,
      { description: "COMPANY.COMPANIES.TABLE_DEFAULTMAVATACCOUNT_DESCRIPTION", name: "defaultMAVatAccount", type: "text", ifRange: false } as FormModel,
      { description: "COMPANY.COMPANIES.TABLE_CREATED_DESCRIPTION", name: "created", type: "date", ifRange: true } as FormModel
  ];
  validators = [Validators.nullValidator, Validators.pattern("^[0-9]*$"), Validators.nullValidator, Validators.nullValidator, Validators.nullValidator, Validators.nullValidator, Validators.nullValidator, Validators.nullValidator];
  selection = new SelectionModel<any>(true, []);
  refreshTable: Subject<void> = new Subject<void>();
  userRole: string = "";

    /** companies ctor */
  constructor(public dialog: MatDialog,
    private _auth: AuthenticationService,
    private _router: Router) {
    this.selection.changed.subscribe(s => { });
    this._auth.currentUserRole.subscribe(x => {
      this.userRole = x;
    })
  }

  dbClickHandler(data: any) {
    this._router.navigateByUrl(this.detailsComponent + "/" + data["id"]);
    this._auth.currentUserRole.subscribe(role => {
      if (role === "Administrator") {
        this._auth.setSelectedCompany({ id: data.id, name: data.name, isInBoard:true});
      }
    });
  }
  add() {
    this.dialog.open(CompanyCreateComponent, {
      height: 'auto',
      width: 'auto',
      maxHeight: '75vh'
    }).afterClosed().subscribe(x => {
    });
  }
}
