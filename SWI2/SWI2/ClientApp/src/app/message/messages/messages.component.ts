import { SelectionModel } from '@angular/cdk/collections';
import { HttpErrorResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { Subject } from 'rxjs';
import { AlertData } from '../../alert/alert.component';
import { FormModel } from '../../models/Form.model';
import { AskIfDialog } from '../../shared/components/askIfDialog/askIfDialog.component';
import { ApiService } from '../../shared/services/api.service';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';
import { MessageViewComponent } from '../message-view/message-view.component';
import { MessageModel } from '../message.model';
import { MessagesCreateComponent } from '../messages-create/messages-create.component';

@Component({
  selector: 'swi-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent {
  actMessageType = "received";
  sortActive = "posted";
  requestUrl = {
    "received": "api/messagepanel/received",
    "sended": "api/messagepanel/sended",
    "trashbox": "api/messagepanel/trashbox",
  };
  detailsComponent = MessageViewComponent;
  headerRow = [
    { description: "MESSAGES.MESSAGES.TABLE_TITLE", name: "title", type: "text", ifRange: false } as FormModel,
    { description: "MESSAGES.MESSAGES.TABLE_RECIVER", name: "messageReceiverName", type: "text", ifRange: false } as FormModel,
    { description: "MESSAGES.MESSAGES.TABLE_SENDER", name: "messageSenderName", type: "text", ifRange: false } as FormModel,
    { description: "MESSAGES.MESSAGES.TABLE_SEND_DATE", name: "posted", type: "date", ifRange: true } as FormModel
  ]
  validators = [Validators.nullValidator, Validators.nullValidator, Validators.nullValidator, Validators.nullValidator, Validators.nullValidator ];
  selection = new SelectionModel<MessageModel>(true, []);
  refreshTable: Subject<void> = new Subject<void>();

  actMessage: MessageModel = new MessageModel({});

  constructor(public dialog: MatDialog,
    private _api: ApiService,
    private errorD: ErrorDialogService) {

  }

  changeMessageType() {
    this.actMessage = new MessageModel({});
    this.refreshTable.next();
  }

  dbClickHandler(mess: MessageModel) {
    this._api.get("api/messagepanel/" + mess.id).toPromise().then(data => {
      mess = new MessageModel(data);
      this.actMessage = mess;
    }).catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    });
    
  }

  add() {
    this.dialog.open(MessagesCreateComponent, {
      maxHeight: '96vh',
      height: 'auto',
      width: '90vw',
    }).afterClosed().subscribe(x => {
      if (x && x.op === "create")
        this.refreshTable.next();
    });
  }
  delete() {
    if (this.selection.selected.length === 0)
      return;

    this.dialog.open(AskIfDialog, {
      disableClose: true,
      data: { data: "MESSEGES.MESSEGES.ASK_BIN", close: "NO", ok: "YES" }
    }).afterClosed().subscribe(result => {
      if (!result)
        return;
      let counter = this.selection.selected.length;
      const errorArray = [];
      for (const item of this.selection.selected) {
        this._api.delete("/api/messagepanel/" + item.id).toPromise().then(() => {
          counter--
          if (counter <= 0) {
            if (errorArray.length === 0)
              this.errorD.showAlert(new AlertData("MESSAGES.MESSAGES.DELETED_MESSAGES_TITLE_ALERT","MESSAGES.MESSAGES.DELETED_MESSAGES_ALERT", {length: this.selection.selected.length}));
            else if (errorArray.length !== 0)
              errorArray.forEach(e => this.errorD.showDialog(e))
            this.refreshTable.next();
            this.selection.clear();
          }

        }).catch((x: HttpErrorResponse) => {
          counter--
          errorArray.push(x);
          if (counter <= 0) {
            if (errorArray.length === 0)
              this.errorD.showAlert(new AlertData("MESSAGES.MESSAGES.DELETED_MESSAGES_TITLE_ALERT", "MESSAGES.MESSAGES.DELETED_MESSAGES_ALERT", { length: this.selection.selected.length }));
            else if (errorArray.length !== 0)
              errorArray.forEach(e => this.errorD.showDialog(e))
            this.refreshTable.next();
            this.selection.clear();
          }
        })
      }
    })
  }
  restore() {
    if (this.selection.selected.length === 0)
      return;

    this.dialog.open(AskIfDialog, {
      disableClose: true,
      data: { data: "MESSAGES.MESSAGES.ASK_RECOVER_BIN", close: "NO", ok: "YES" }
    }).afterClosed().subscribe(result => {
      if (!result)
        return;
      let counter = this.selection.selected.length;
      const errorArray = [];
      for (const item of this.selection.selected) {
        this._api.put("/api/messagepanel/" + item.id,null).toPromise().then(() => {
          counter--
          if (counter <= 0) {
            if (errorArray.length === 0)
              this.errorD.showAlert(new AlertData("MESSAGES.MESSAGES.RECOVERD_MESSAGES_TITLE_ALERT", "MESSAGES.MESSAGES.RECOVERD_MESSAGES_ALERT", { length: this.selection.selected.length }));
            else if (errorArray.length !== 0)
              errorArray.forEach(e => this.errorD.showDialog(e))
            this.refreshTable.next();
            this.selection.clear();
          }

        }).catch((x: HttpErrorResponse) => {
          counter--
          errorArray.push(x);
          if (counter <= 0) {
            if (errorArray.length === 0)
              this.errorD.showAlert(new AlertData("MESSAGES.MESSAGES.RECOVERD_MESSAGES_TITLE_ALERT", "MESSAGES.MESSAGES.RECOVERD_MESSAGES_ALERT", { length: this.selection.selected.length }));
            else if (errorArray.length !== 0)
              errorArray.forEach(e => this.errorD.showDialog(e))
            this.refreshTable.next();
            this.selection.clear();
          }
        })
      }
    })
  }
}
