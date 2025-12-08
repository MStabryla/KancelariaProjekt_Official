import { FormArray, FormGroup } from "@angular/forms";
import { AssignToModel } from "../../shared/helpers/model-helper";
import { BaseModel } from "../Base.model";
export class PaymentTableModel {
  constructor(obj?: object) {
    if (!obj) {
      this.id = 0;
      this.paymentsForInvoices = [null];
      this.contractorName = '';
      this.contractorNip = '';
      this.contractorId = 0;
      this.contractorBankAccountId = 0;
      this.contractorBankAccountName = '';
      this.contractorBankAccountNumber = '';
      this.topic = '';
      this.paymentValue = null;
      this.currency = '';
      this.paymentDate = new Date();
    } else {
      Object.assign(this, obj);
    }
  }
  id: number;
  paymentsForInvoices: PaymentsForInvoicesFormModel[];
  contractorName: string;
  contractorNip: string;
  contractorId: number;
  contractorBankAccountId: number;
  contractorBankAccountName: string;
  contractorBankAccountNumber: string;
  topic: string;
  paymentValue: number;
  currency: string;
  paymentDate: Date;
}
export class PaymentModel {
  constructor(obj?) {
    AssignToModel(this, obj);
  }
  id: number = 0;
  contractorBankAccount: BaseModel = new BaseModel();
  paymentsForInvoices: PaymentsForInvoices[] = [new PaymentsForInvoices()];
  topic: string ='';
  paymentValue: number =0;
  currency: string = '';
  paymentDate: Date = new Date();
}
export class PaymentFormModel {
  constructor() { }
  id: number;
  contractor: ContractorPaymentFormModel;
  contractorBankAccount: ContractorBankAccountPaymentFormModel;
  paymentsForInvoices: PaymentsForInvoicesFormModel[];
  topic: string;
  paymentValue: number;
  currency: string;
  paymentDate: Date;
}

export class ContractorPaymentFormModel {
  constructor(obj?: any) {
    Object.assign(this, obj);
  }
  id: number = 0;
  name: string = '';
  nip: string= '';
}
export class ContractorBankAccountPaymentFormModel {
  constructor(obj?: any) {
    Object.assign(this, obj);
  }
  id: number=0;
  bankName: string='';
  accountNumber: string='';
}

export class PaymentsForInvoicesFormModel  {
  constructor(obj?: any) {
    Object.assign(this, obj);
  }
  id: number;
  invoice: InvoicePaymentFormModel;
  paymentValueForInvoice: number = 0;
}
export class InvoicePaymentFormModel {
  constructor(obj?: any) {
    Object.assign(this, obj);
  }
  id: number;
  number: string;
  bruttoWorth: number;
}

export class PaymentsForInvoices {
  constructor(obj?: any) {
    Object.assign(this, obj);
  }
  id = 0;
  invoice: BaseModel = new BaseModel();
  paymentValueForInvoice: number =0;
}
export class PaymentMachingModel {
  constructor() {
  }
  id: number;
  contractorBankAccount: ContractorBankAccountPaymentMachingModel | FormGroup;
  paymentsForInvoices: PaymentsForInvoicesFormModel[] | FormArray;
  topic: string;
  addressee: string;
  paymentValue: number;
  currency: string;
  paymentDate: Date;
}
export class ContractorBankAccountPaymentMachingModel {
  constructor() {
  }
  id: number;
  bankName: string;
  accountNumber: string;
  contractor: ContractorPaymentMachingModel | FormGroup;
}
export class ContractorPaymentMachingModel {
  constructor() {
  }
  id: number;
  name: string;
}
