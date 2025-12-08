import { Validators } from "@angular/forms";
import { ContractorBankAccount } from "./ContractorBankAccount.model";

export class Contractor {
  constructor(obj?: any) {
    Object.assign(this, obj);
/*    this.id = 0;
    this.name = null;
    this.street = null;
    this.houseNumber = null;
    this.apartamentNumber = null;
    this.postalcode = null;
    this.city = null;
    this.nip = null;
    this.postoffice = null;
    this.mailLanguage = null;
    this.wNAccount = null;*/
  }
  id: number;
  name: string;
  street: string;
  houseNumber: string;
  apartamentNumber: string;
  postalcode: string;
  postoffice: string;
  city: string;
  country: string;
  nip: number;
  created: Date;
  wNAccount: string;
  email: string;
  mailLanguage: MailLanguage;
  contractorBankAccounts: ContractorBankAccount[];
}

export enum MailLanguage {
  PL,
  EN,
  IT
}
