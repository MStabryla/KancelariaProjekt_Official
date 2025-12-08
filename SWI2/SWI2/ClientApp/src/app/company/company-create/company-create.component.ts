import { HttpErrorResponse } from '@angular/common/http';
import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ErrorDialogComponent } from '../../errors/error-dialog/error-dialog.component';
import { ApiService } from '../../shared/services/api.service';
import { Company } from '../company.model';
import { SWIError } from '../../errors/error';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';

@Component({
    selector: 'swi-company-create',
    templateUrl: './company-create.component.html',
    styleUrls: ['./company-create.component.css']
})
/** company-create component*/
export class CompanyCreateComponent {
    userRole: string = "";
    public createFG: FormGroup;

    /** company-create ctor */
  constructor(private _api: ApiService,
    private dialogRef: MatDialogRef<Company>,
    private formBuilder: FormBuilder,
    private dialog: MatDialog,
    private _auth: AuthenticationService,
    private errorD: ErrorDialogService) {
    this._auth.currentUserRole.subscribe(x => {
      this.userRole = x;
    })
    this.createFG = formBuilder.group({
      name: ['', Validators.required],
      country: [''],
      creationPlace: [''],
      address: formBuilder.group({
        city: [''],
        postoffice: [''],
        postalcode: [''],
        street: [''],
        housenumber: [''],
        apartamentNumber: ['']
      }),
      nip: ['', Validators.required],
      defaultWNAAccount: [''],
      defaultMAAccount: [''],
      defaultMAVatAccount: [''],
    })
  }
  onSubmit() {
    const rawData = this.createFG.value;
    const data = new Company(rawData);
    data.creationPlace = rawData.creationPlace;
    data.city = rawData.address.city;
    data.postoffice = rawData.address.postoffice;
    data.postalcode = rawData.address.postalcode;
    data.street = rawData.address.street;
    data.housenumber = rawData.address.housenumber;
    data.apartamentNumber = rawData.address.apartamentNumber;
    this._api.post("/api/companypanel/", data).toPromise().then(() => {
      this.dialogRef.close({ op: "create", data: data });
    }).catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    });
  }
}
