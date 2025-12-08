export class ContractorBankAccount {
  constructor(obj?: any) {
    Object.assign(this, obj);
/*    this.id = 0;
    this.contractor = {id:0};
    this.bankName = null;
    this.accountNumber = null;
    this.created = null;*/
  }
  id: number;
  contractor: { id: number };
  bankName: string;
  accountNumber: string;
  created: Date;
 // Payments: Payment[];
}

export class ContractorBankAccountTableModel {
  constructor(id = 0,
    contarctorId= 0,
    contarctorName: string = null,
    bankName : string = null,
    accountNumber : string = null,
    created: Date = null) {
    this.id = id;
    this.contarctorId = contarctorId;
    this.contarctorName = contarctorName;
    this.bankName = bankName;
    this.accountNumber = accountNumber;
    this.created = created;
  }
  id: number;
  contarctorId: number;
  contarctorName: string;
  bankName: string;
  accountNumber: string;
  created: Date;
  getDescriptions(): {} {
    return {
      contarctorName: "CONTRACTORS.CONTRACTORBANKACCOUNTTABLE.TABLE_CONTRACTOR_NAME_DESCRIPTION", bankName: "CONTRACTORS.CONTRACTORBANKACCOUNTTABLE.TABLE_BANK_NAME_DESCRIPTION",
      accountNumber: "CONTRACTORS.CONTRACTORBANKACCOUNTTABLE.TABLE_ACCOUNT_NUMBER_DESCRIPTION", created: "CONTRACTORS.CONTRACTORBANKACCOUNTTABLE.TABLE_CREATED_DESCRIPTION"
    };
  }
}
