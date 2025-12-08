import { HttpErrorResponse, HttpEventType, HttpResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { finalize } from 'rxjs/operators';
import { TableParamsModel } from '../../models/tableParams.model';
import { ApiService } from '../../shared/services/api.service';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';
import { LetterModel } from '../letter.model';

@Component({
    selector: 'swi-letters-create',
    templateUrl: './letters-create.component.html',
    styleUrls: ['./letters-create.component.css']
})
/** letters-create component*/
export class LettersCreateComponent {
  createFormGroup: FormGroup;
  letterRecipients: any[];
  file: File;
  userRole: string;

  sending = false;
  loadingLetterRecipients = false;
  creatingLetterRecipient = false;


  constructor(private formBuilder: FormBuilder,
    private _auth: AuthenticationService,
    private _api: ApiService,
    private dialogRef: MatDialogRef<LetterModel>,
    private errorD: ErrorDialogService) {
    this.createFormGroup = formBuilder.group({
      notes: ['', [Validators.nullValidator]],
      registeredNumbr: ['', [Validators.nullValidator]],
      outLetter: ["false", [Validators.nullValidator]],
      isPaid: ["false", [Validators.nullValidator]],
      isRegistered: ["false", [Validators.nullValidator]],
      isEmail: ["false", [Validators.nullValidator]],
      isNormal: ["false", [Validators.nullValidator]],
      isCourier: ["false", [Validators.nullValidator]],
      withConfirm: ["false", [Validators.nullValidator]],
      added: [null, [Validators.required]],
      letterRecipient: [null, [Validators.required]],
      letterRecipientSearch: ['', [Validators.nullValidator]],
      letterRecipientCreate: ['', [Validators.nullValidator]],
      file: ['', [Validators.nullValidator]],
    })
    _auth.currentUserRole.subscribe(x => {
      this.userRole = x;
    })
  }

  getLetterRecipients(event: any) {
    this.loadingLetterRecipients = true;
    const searchValue = (this.createFormGroup.get("letterRecipient").value ?? "") + event.key;
    const tableParams = { pageNumber: 0, pageSize: 20, sort: 'name', filters: [{ name: "name", value: searchValue }] } as TableParamsModel;
    this._api.get("/api/documentpanel/letter/recipients", { query: btoa(JSON.stringify(tableParams)) }).toPromise().then((data: any) => {
      this.letterRecipients = data.elements;
    }).catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    }).finally(() => {
      this.loadingLetterRecipients = false;
    });
  }
  displayFn(letterRecipient: any): string {
    return letterRecipient && letterRecipient.name ? letterRecipient.name : '';
  }

  fileSelected(event) {
    this.file = event.target.files[0];
  }
  startCreatingLetterRecipient() {
    this.creatingLetterRecipient = true;
    //new popup
  }
  createLetterRecipient() {
    const newLetterRecipient = this.createFormGroup.value["letterRecipientCreate"];
    if (newLetterRecipient === "")
      return;

    this._api.post("/api/documentpanel/letter/recipients/", { Name: newLetterRecipient }).toPromise().catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    }).finally(() => { this.getLetterRecipients({ key: "" }); this.createFormGroup.value["createDocumentType"] = ""; this.creatingLetterRecipient = false; });
  }
  deleteLetterRecipient(id: number) {
    this._api.delete("/api/documentpanel/letter/recipients/" + id).toPromise().catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    }).finally(() => { this.getLetterRecipients({key:""}); });
  }

  searchFiles(fileInput: any) {
    fileInput.click();
  }
  getErrors() {
    const properties = Object.getOwnPropertyNames(this.createFormGroup.value)
    return properties.map(x => { return { property: x, error: this.createFormGroup.get(x).errors } });
  }

  onSubmit() {
    if (!this.createFormGroup.valid) {
      return;
    }
    const rawData = this.createFormGroup.value;
    rawData["letterRecipientId"] = this.createFormGroup.value.letterRecipient.id;
    const data = new LetterModel(rawData);
    const formData = new FormData();
    formData.append("file", this.file);
    formData.append("modelJson", JSON.stringify(data));

    const url = "/api/documentpanel/letter";
    this.sending = true;
    this._api.post(url, formData, { responseType: "blob", reportProgress: true, observe: "events" }, { reportProgress: true }).pipe(
      finalize(() => {
        this.sending = false;
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
