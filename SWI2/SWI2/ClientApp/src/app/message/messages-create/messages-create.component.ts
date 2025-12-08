import { HttpErrorResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { startWith } from 'rxjs/operators';
import { TableParamsModel } from '../../models/tableParams.model';
import { ApiService } from '../../shared/services/api.service';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';
import { MessageTempalteComponent } from '../message-tempalte/message-tempalte.component';
import { MessageTemplateModel } from '../message.model';
import { MessageModel } from '../message.model';

@Component({
  selector: 'swi-messages-create',
  templateUrl: './messages-create.component.html',
  styleUrls: ['./messages-create.component.css']
})
/** messages-create component*/
export class MessagesCreateComponent {
  createFormGroup: FormGroup;
  userName: string;

  messageReceivers = [];
  loadingMessageReceivers = false;
  messageTamplate = null;
  constructor(public dialog: MatDialog,
    private formBuilder: FormBuilder,
    private _auth: AuthenticationService,
    private _api: ApiService,
    private errorD: ErrorDialogService,
    private dialogRef: MatDialogRef<MessageModel>,) {
    this.createFormGroup = formBuilder.group({
      title: ['', [Validators.required]],
      content: ['', [Validators.required]],
      messageReceiver: [null, [Validators.required]],
    })
    this._auth.currentUserName.subscribe(data => {
      this.userName = data;
    })
    this.createFormGroup.controls.messageReceiver.valueChanges.pipe(startWith('')).subscribe(mr => {
      this.getUsers(mr);    });
  }

  displayFn(messageReceiver: any): string {
    return messageReceiver && messageReceiver.name ? messageReceiver.name : '';
  }
  getUsers(messageReceiver: any) {
    this.loadingMessageReceivers = true;
    const searchValue = messageReceiver.name ? messageReceiver.name : messageReceiver;
    const tableParams = { pageNumber: 0, pageSize: 20, sort: 'UserName', filters: [{ name: "UserName", value: searchValue }] } as TableParamsModel;
    this._api.get("/api/messagepanel/receivers", { query: btoa(JSON.stringify(tableParams)) }).toPromise().then((data: any) => {
      this.messageReceivers = data;
    }).catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    }).finally(() => {
      this.loadingMessageReceivers = false;
    });
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
    rawData["MessageReceiverId"] = this.createFormGroup.value.messageReceiver.id;
    rawData["messageReceiver"] = null;
    const data = new MessageModel(rawData);

    const url = "/api/messagepanel";
    this._api.post(url, data).toPromise().then(x => {
      this.dialogRef.close({ op: "create", data: x })
    }).catch ((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    })
    
  }
  openTempalates() {
    this.dialog.open(MessageTempalteComponent, {
      height: 'auto',
      width: 'auto',
      maxHeight: '75vh'
    }).afterClosed().subscribe(template => {
      this.messageTamplate = template;
      this.createFormGroup.patchValue({
        title: template.title,
        content: template.message
      });
    });
  }
  editTemplate() {
    this._api.put("/api/messagepanel/templates/" + this.messageTamplate.id, new MessageTemplateModel({ id: 0, title: this.createFormGroup.value.title, message: this.createFormGroup.value.content })).toPromise().then((data: any) => {
    }).catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    }).finally(() => {
    });
  }
  addTemplate() {
    this._api.post("/api/messagepanel/templates/", new MessageTemplateModel({ title: this.createFormGroup.value.title, message: this.createFormGroup.value.content })).toPromise().then((data: any) => {
    }).catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    }).finally(() => {
      this.loadingMessageReceivers = false;
    });
  }
}
