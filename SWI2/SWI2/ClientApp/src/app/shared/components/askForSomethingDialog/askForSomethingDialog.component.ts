import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'askForSomethingDialog',
  templateUrl: './askForSomethingDialog.component.html',
  styleUrls: ['./askForSomethingDialog.component.css']
})
export class AskForSomethingDialog implements OnInit {
  returnForm: FormGroup;
  constructor(
    formBuilder: FormBuilder,
    @Inject(MAT_DIALOG_DATA) public data: { data: string, close: string, ok: string, returnDescription: string, regEx: string }) {
    this.returnForm = formBuilder.group({
      returnValue: [
        null,
        [data.regEx.length > 0 ? Validators.pattern(data.regEx) : Validators.nullValidator, Validators.required]
      ]
    });

  }

  ngOnInit(): void {

  }


}
