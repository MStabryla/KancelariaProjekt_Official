import { Component, OnInit } from '@angular/core';
import { Validators } from '@angular/forms';
import { FormModel } from '../../models/Form.model';
import { Subject } from 'rxjs';
import { MatDialogRef } from '@angular/material/dialog';
import { MessageTemplateModel } from '../message.model';
import { SelectionModel } from '@angular/cdk/collections';
import { ApiService } from '../../shared/services/api.service';
import { HttpErrorResponse } from '@angular/common/http';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';
@Component({
  selector: 'app-message-tempalte',
  templateUrl: './message-tempalte.component.html',
  styleUrls: ['./message-tempalte.component.css']
})
export class MessageTempalteComponent implements OnInit {
  sortActive = "created";
  requestUrl = "api/messagepanel/templates";
  detailsComponent = null;
  headerRow = [
    { description: "MESSAGES.MESSAGE_TEMPLATE.TABLE_TITLE", name: "title", type: "text", ifRange: false } as FormModel,
    { description: "MESSAGES.MESSAGE_TEMPLATE.TABLE_CONTENT", name: "message", type: "text", ifRange: false } as FormModel,
    { description: "MESSAGES.MESSAGE_TEMPLATE.TABLE_CREATED", name: "created", type: "date", ifRange: false } as FormModel
  ]
  validators = [Validators.nullValidator, Validators.nullValidator, Validators.nullValidator];
  refreshTable: Subject<void> = new Subject<void>();
  selection = new SelectionModel<MessageTemplateModel>(true, []);

  constructor(private dialogRef: MatDialogRef<MessageTempalteComponent>,
    private errorD: ErrorDialogService,
    private _api: ApiService) { }


  ngOnInit(): void {
  }


  dbClickHandler(event) {
    this.dialogRef.close(event);
  }
  delateTemplates() {
    this.selection.selected.forEach(t => {
      this._api.delete("/api/messagepanel/templates/" + t.id).toPromise().then((data: any) => {
        this.refreshTable.next();
      }).catch((x: HttpErrorResponse) => {
        this.errorD.showDialog(x);
      }).finally(() => {
      });
    });

  }
}
