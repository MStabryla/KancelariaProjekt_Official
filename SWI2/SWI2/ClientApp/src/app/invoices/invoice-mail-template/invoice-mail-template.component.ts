import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { InvoiceMailTemplate } from '../../models/invoices/InvoiceMailTemplate.model';
import { ApiService } from '../../shared/services/api.service';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';

@Component({
  selector: 'app-invoice-mail-template',
  templateUrl: './invoice-mail-template.component.html',
  styleUrls: ['./invoice-mail-template.component.css']
})
export class InvoiceMailTemplateComponent {
  companyId: number;
  mailTemplateForm: FormGroup;
  invoiceMailTemplates: InvoiceMailTemplate[];
  languages = [
    { id: 0, name: "PL" },
    { id: 1, name: "EN" },
    { id: 2, name: "IT" }
  ];
  constructor(private _api: ApiService,
    private formBuilder: FormBuilder,
    private _auth: AuthenticationService,
    private errorD: ErrorDialogService) {
    this.mailTemplateForm = this.formBuilder.group({
      id: [0, Validators.required],
      message: ['', Validators.required],
      title: ['', Validators.required],
      mailLanguage: [0, Validators.required]
    });

    this._auth.currentSelectedCompany.subscribe(data => {
      this.companyId = data.id;
      this._api.get<InvoiceMailTemplate[]>("api/companypanel/mailtemplates/" + this.companyId).toPromise().then(invoiceMailTemplates => {
        this.invoiceMailTemplates = invoiceMailTemplates;
        this.updateForm(0);
      }).catch((x: HttpErrorResponse) => {
        this.errorD.showDialog(x);
      });
    });
  }
  updateForm(mailLanguage) {
    const invoiceMailTemplate = this.invoiceMailTemplates.filter(imt => imt.mailLanguage == mailLanguage)[0];
    if (invoiceMailTemplate != null) {
      this.mailTemplateForm.patchValue({
        id: invoiceMailTemplate.id,
        message: invoiceMailTemplate.message,
        title: invoiceMailTemplate.title,
        mailLanguage: invoiceMailTemplate.mailLanguage
      });
    } else {
      this.mailTemplateForm.patchValue({
        id: 0,
        message: '',
        title: '',
        mailLanguage: mailLanguage
      });
    }

  }

  onSubmit() {
    this._api.post("api/companypanel/mailtemplates/" + this.companyId, new InvoiceMailTemplate(this.mailTemplateForm.value)).toPromise().then(x => {
    }).catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    })

  }
}
