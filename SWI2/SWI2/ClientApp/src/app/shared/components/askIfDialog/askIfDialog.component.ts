import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'askIfDialog',
  templateUrl: './askIfDialog.component.html',
  styleUrls: ['./askIfDialog.component.css']
})
export class AskIfDialog implements OnInit {

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: {data:string,close:string,ok:string}) { }

  ngOnInit(): void {
  }


}
