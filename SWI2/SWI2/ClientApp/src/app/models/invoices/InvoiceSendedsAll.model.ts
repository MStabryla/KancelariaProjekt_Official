import { Validators } from "@angular/forms";
import { FormModel } from "./Form.model";

export class InvoiceSendedsAll {
  constructor() {
    this.id = null;
    this.email = null;
    this.userName = null;
    this.created = null;
    this.number = null;
  }
  id: number;
  number: string;
  email: string;
  userName: string;
  created: Date;

/*  getFormDescriptions(): FormModel[] {
    return [{ description: "numer", name: "number", type: "number", ifRange:true } as FormModel,
      { description: "email", name: "email", type: "email", ifRange: false } as FormModel,
      { description: "użytkownik", name: "userName", type: "text", ifRange: false } as FormModel,
      { description: "data utworzenia", name: "created", type: "date", ifRange: true } as FormModel];
  }

  getRegExs(): Validators[] {
    return [Validators.email, Validators.nullValidator, Validators.nullValidator, Validators.pattern("^[0-9]*$")];
  }*/
}
