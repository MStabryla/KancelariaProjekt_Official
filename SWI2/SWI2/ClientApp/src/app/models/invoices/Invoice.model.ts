import { AssignToModel } from "../../shared/helpers/model-helper";
import { BaseModel } from "../Base.model";
import { ContractorBankAccount } from "../contractors/ContractorBankAccount.model";

export class Invoice {
  constructor(obj?: any) {
    if (obj) AssignToModel(this, obj);
  }
  id: number = 0;
  number: string = '';
  nettoWorth: number = 0;
  bruttoWorth: number = 0;
  correcting: string = '';
  paymentStatus: string = '';
  paymentCurrency: string = '';
  paymentsForInvoices: PaymentsForInvoices[] = [new PaymentsForInvoices()];
  created: Date = new Date();
  creationPlace: string = '';
  paymentAccountNumber: string = '';
  paymentBank: string = '';
  note: string = '';
  sellDateName: SellDateName = new SellDateName();
  sellDate: Date = new Date();
  paymentDate: Date = new Date();
  isTransferType: boolean = false;
  invoiceIssuer: InvoiceIssuer = new InvoiceIssuer();
  invoiceContractor: InvoiceContractor = new InvoiceContractor();
  invoiceEntries: InvoiceEntries[] = [new InvoiceEntries()];
  invoiceSendeds: InvoiceSendeds[] = [new InvoiceSendeds()];
  rate = 1;
}
export class PaymentsForInvoices {
  id = 0;
  payment = { id: 0 };
  paymentValueForInvoice = 0;
}

export class InvoiceTable {
  totalCount: number;
  invoice: Invoice[];
}
export class InvoiceContractor {
  constructor(obj?) {
    if (obj) AssignToModel(this, obj);
  }
  name: string = '';
  street: string = '';
  houseNumber: string = '';
  apartamentNumber: string = '';
  postalcode: string = '';
  city: string = '';
  country: string = '';
  nip: string = '';
  contractor: { id: number, name: string } = { id: 0, name: ''/*, contractorBankAccounts :[]*/ };
  postoffice: string = '';
  id: number = 0;
  created: Date = new Date();
}
export class InvoiceIssuer {
  constructor() {
    this.name = null;
    this.street = null;
    this.houseNumber = null;
    this.apartamentNumber = null;
    this.postalcode = null;
    this.city = null;
    this.country = null;
    this.nip = null;
    this.id = null;
    this.postoffice = null;
    this.created = null;
  }
  name: string;
  street: string;
  houseNumber: string;
  apartamentNumber: string;
  postoffice: string;
  postalcode: string;
  city: string;
  country: string;
  nip: number;
  id: number;
  created: Date;
}
export class InvoiceEntries {
  constructor() {
    this.id = 0;
    this.gtu = null;
    this.name = '';
    this.price = 1;
    this.quantity = 1;
    this.vat = 23;
  }
  id: number;
  gtu: number;
  name: string;
  price: number;
  quantity: number;
  vat: number
}
export class SellDateName {
  constructor() {
    this.id = null;
    this.name = null;
  }
  id: number;
  name: string;
}
export class InvoiceSendeds {
  constructor() {
    this.id = null;
    this.email = null;
    this.user = { userName: null };
    this.created = null;
    this.number = null;
  }
  id: number;
  number: string;
  email: string;
  user: { userName: string };
  created: Date;
}
