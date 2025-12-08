import { HttpErrorResponse, HttpEventType, HttpResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { finalize } from 'rxjs/operators';
import { TableParamsModel } from '../../models/tableParams.model';
import { ApiService } from '../../shared/services/api.service';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';
import { DocumentModel } from '../document.model';

@Component({
  selector: 'swi-document-create',
  templateUrl: './document-create.component.html',
  styleUrls: ['./document-create.component.css']
})
/** document-create component*/
export class DocumentCreateComponent {
  createFormGroup: FormGroup;
  documentTypes: any[];
  companies: any[] = [];
  file: File;
  userRole: string;

  sending = false;
  creatingDocumentType = false;
  loadingCompanies = false;
  showErrors = false;

  
  /** document-create ctor */
  constructor(private formBuilder: FormBuilder,
    private _auth: AuthenticationService,
    private _api: ApiService,
    private dialogRef: MatDialogRef<DocumentModel>,
    private errorD: ErrorDialogService,) {
    this.createFormGroup = formBuilder.group({
      notes: ['', [Validators.nullValidator]],
      comment: ['', [Validators.nullValidator]],
      isProtocol: ["false", [Validators.nullValidator]],
      outDocument: ["false", [Validators.nullValidator]],
      company: [null, [Validators.required]],
      documentTypeId: [null, [Validators.required]],
      createDocumentType: ['', [Validators.nullValidator]],
      companySearch: ['', [Validators.nullValidator]],
      file: ['', [Validators.nullValidator]],
    })
    this.getDocumentTypes();
    _auth.currentUserRole.subscribe(x => {
      this.userRole = x;
    })
  }


  getDocumentTypes() {
    this._api.get("/api/documentpanel/document/types").toPromise().then((data: any[]) => {
      this.documentTypes = data;
    }).catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    });
  }

  getCompanies(event: any) {
    this.loadingCompanies = true;
    const searchValue = (this.createFormGroup.get("company").value ?? "") + event.key;
    const tableParams = { pageNumber: 0, pageSize: 0, sort: '', filters: [{ name: "name", value: searchValue }] } as TableParamsModel;
    this._api.get("/api/documentpanel/document/companies", { query: btoa(JSON.stringify(tableParams)) }).toPromise().then((data: any[]) => {
      this.companies = data;
    }).catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    }).finally(() => {
      this.loadingCompanies = false;
    });
  }
  displayFn(company: any): string {
    return company && company.name ? company.name : '';
  }
  fileSelected(event) {
    this.file = event.target.files[0];
  }
  startCreatingDocumentType() {
    this.creatingDocumentType = true;
  }
  createDocumentType() {
    const newDocumentTypeName = this.createFormGroup.value["createDocumentType"];
    if (newDocumentTypeName === "")
      return;

    this._api.post("/api/documentpanel/document/types", { Name: newDocumentTypeName }).toPromise().catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    }).finally(() => { this.getDocumentTypes(); this.createFormGroup.value["createDocumentType"] = ""; this.creatingDocumentType = false; });
  }
  deleteDocumentType(id: number) {
    this._api.delete("/api/documentpanel/document/types/" + id).toPromise().catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    }).finally(() => { this.getDocumentTypes(); });
  }

  getErrors() {
    const properties = Object.getOwnPropertyNames(this.createFormGroup.value)
    return properties.map(x => { return { property: x, error: this.createFormGroup.get(x).errors } });
  }

  searchFiles(fileInput: any) {
    fileInput.click();
  }

  onSubmit() {
    if (!this.createFormGroup.valid) {
      this.showErrors = true;
      return;
    }
    const rawData = this.createFormGroup.value;
    rawData["companyId"] = this.createFormGroup.value.company.id;
    const data = new DocumentModel(rawData); 
    const formData = new FormData();
    formData.append("file", this.file);
    formData.append("modelJson", JSON.stringify(data));

    const url = "/api/documentpanel/document";
    this.sending = true;
    this._api.post(url, formData, { responseType: "blob", reportProgress: true, observe: "events" }, { reportProgress: true }).pipe(
      finalize(() => {
        this.sending = false;
        this.showErrors = false;
        this.dialogRef.close({ op: "create", data: event })
      })
    ).subscribe(
      (event: any) => {
        if (event.type === HttpEventType.UploadProgress) {
        } else if (event instanceof HttpResponse) {
          this.dialogRef.close({ op: "create", data: event })
        }
      },
      (error) => {
        this.errorD.showDialog(error);
      }
    );
  }
}
