import { SelectionModel } from "@angular/cdk/collections";
import { Component } from "@angular/core";
import { Validators } from "@angular/forms";
import { Subject } from "rxjs";
import { FormModel } from "../../models/Form.model";

@Component({
  selector: 'swi-invoiceSendAll',
  templateUrl: './invoiceSendAll.component.html',
  styleUrls: ['./invoiceSendAll.component.css']
})

export class invoiceSendAll {

  sortActive = "created";
  requestUrl = "api/invoice/sendedinvoices/{id}";
  detailsComponent = null;
  headerRow = [{ description: "NUMBER", name: "number", type: "text", ifRange: false } as FormModel,
  { description: "EDIT", name: "email", type: "email", ifRange: false } as FormModel,
  { description: "USER", name: "userName", type: "text", ifRange: false } as FormModel,
    { description: "CREATION_DATE", name: "created", type: "date", ifRange: true } as FormModel];
  validators = [Validators.email, Validators.nullValidator, Validators.nullValidator, Validators.pattern("^[0-9]*$")];
 // selection = new SelectionModel<any>(true, []);
  refreshTable: Subject<void> = new Subject<void>();

  constructor() {
  }
}


