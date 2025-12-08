import { Component, Input } from '@angular/core';
import { MessageModel } from '../message.model';

@Component({
  selector: 'swi-message-view',
  templateUrl: './message-view.component.html',
  styleUrls: ['./message-view.component.css']
})
/** message-view component*/
export class MessageViewComponent {
  @Input() data: MessageModel;
  @Input() messageType: string;
  /** message-view ctor */
  constructor() {

  }
}
