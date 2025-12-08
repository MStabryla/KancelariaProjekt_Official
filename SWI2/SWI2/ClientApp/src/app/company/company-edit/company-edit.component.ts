import { HttpErrorResponse } from '@angular/common/http';
import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ErrorDialogComponent } from '../../errors/error-dialog/error-dialog.component';
import { ApiService } from '../../shared/services/api.service';
import { Company } from '../company.model';
import { SWIError } from '../../errors/error';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';

@Component({
    selector: 'app-company-edit',
    templateUrl: './company-edit.component.html',
    styleUrls: ['./company-edit.component.css']
})
export class CompanyEditComponent {
  userRole: string = "";
  public editFG: FormGroup;

  constructor(private _api: ApiService,
    private dialogRef: MatDialogRef<Company>,
    private formBuilder: FormBuilder,
    private dialog: MatDialog,
    private _auth: AuthenticationService,
    private errorD: ErrorDialogService,
    @Inject(MAT_DIALOG_DATA) public data: Company)
  {
    this._auth.currentUserRole.subscribe(x => {
      this.userRole = x;
    })
    this.editFG = formBuilder.group({
      name: [data.name],
      country: [data.country],
      address: formBuilder.group({
        city: [data.city],
        postoffice: [data.postoffice],
        postalcode: [data.postalcode],
        street: [data.street],
        housenumber: [data.housenumber],
        apartamentNumber: [data.apartamentNumber]
      }),
      nip: [data.nip],
      defaultWNAAccount: [data.defaultWNAAccount],
      defaultMAAccount: [data.defaultMAAccount],
      defaultMAVatAccount: [data.defaultMAVatAccount],
    })
  }
  onSubmit() {
    const rawData = this.editFG.value;
    const data = new Company(rawData);
    data.id = this.data.id;
    data.creationPlace = this.data.creationPlace;
    data.city = rawData.address.city;
    data.postoffice = rawData.address.postoffice;
    data.postalcode = rawData.address.postalcode;
    data.street = rawData.address.street;
    data.housenumber = rawData.address.housenumber;
    data.apartamentNumber = rawData.address.apartamentNumber;
    this.data = data;
    this._api.put("/api/companypanel/" + (this.userRole !== "client" ? this.data.id : ""), data).toPromise().then(() => {
      this.dialogRef.close({ op:"edit",data: this.data });
    }).catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    });
  }
}
