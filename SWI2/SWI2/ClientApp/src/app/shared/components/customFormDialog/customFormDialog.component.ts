import { AfterViewInit, Component, Inject } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Adress } from '../adressMatInput/adressMatInput.component';
import { CodeCoutry } from '../coutryCodeMatInput/codeCoutryMatInput.component';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'customFormDialog',
  templateUrl: './customFormDialog.component.html',
  styleUrls: ['./customFormDialog.component.css']
})
export class CustomFormDialog {
  returnForm: FormGroup;

  constructor(
    private formBuilder: FormBuilder,
    @Inject(MAT_DIALOG_DATA) public data: { description: string, close: string, ok: string, form: FormPart[] })
  {
    this.returnForm = this.generateForm();
  }

  generateForm(): FormGroup {
    let form = {};
    this.data.form.forEach(fc => {
      if (typeof fc.regEx == 'object') {
        form[fc.name] = [fc.value, fc.regEx.value.length > 0 ? Validators.pattern(fc.regEx.value) : Validators.nullValidator]
      }
      else {
        if (Array.isArray(fc.value)) {
          form[fc.name] = [null, fc.regEx.length > 0 ? Validators.pattern(fc.regEx) : Validators.nullValidator]
        }
        else form[fc.name] = [fc.value, fc.regEx.length > 0 ? Validators.pattern(fc.regEx) : Validators.nullValidator]
      }
    });
    let formGroup = this.formBuilder.group(form);
    return formGroup;
  }

}

export class FormPart {
  description: string;
  name: string;
  value: any;
  regEx: any;
}
