import { AssignToModel } from "../../shared/helpers/model-helper";

export class InvoiceMailTemplate {
  constructor(obj?) {
    if (obj) AssignToModel(this, obj);
  }
  created = new Date();
  id = 0;
  mailLanguage = 0;
  message = '';
  title = '';
}
