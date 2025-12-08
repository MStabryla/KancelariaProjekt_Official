import { AssignToModel } from "../shared/helpers/model-helper";
import { BasicModelClass } from "../shared/models/basic-model";

export class MessageModel extends BasicModelClass {

  constructor(rawData: unknown) {
    super(rawData);
    this.posted = rawData["posted"] ? new Date(rawData["posted"]) : null
    this.readed = rawData["readed"] ? new Date(rawData["readed"]) : null
  }

  id: number;
  title: string;
  content: string;
  posted: Date;
  readed: Date;
  messageReceiverId: string;
  messageReceiverName: string;
  messageSenderName: string;
}

export class MessageTemplateModel {
  constructor(obj?: any) {
    if (obj) AssignToModel(this, obj);
  }
  id = 0;
  title = '';
  message = '';
  created = new Date();
  updated = new Date();
}

