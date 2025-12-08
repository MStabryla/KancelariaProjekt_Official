"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ContractorBankAccountTableModel = exports.ContractorBankAccount = void 0;
var ContractorBankAccount = /** @class */ (function () {
    function ContractorBankAccount(obj) {
        Object.assign(this, obj);
        /*    this.id = 0;
            this.contractor = {id:0};
            this.bankName = null;
            this.accountNumber = null;
            this.created = null;*/
    }
    return ContractorBankAccount;
}());
exports.ContractorBankAccount = ContractorBankAccount;
var ContractorBankAccountTableModel = /** @class */ (function () {
    function ContractorBankAccountTableModel(id, contarctorId, contarctorName, bankName, accountNumber, created) {
        if (id === void 0) { id = 0; }
        if (contarctorId === void 0) { contarctorId = 0; }
        if (contarctorName === void 0) { contarctorName = null; }
        if (bankName === void 0) { bankName = null; }
        if (accountNumber === void 0) { accountNumber = null; }
        if (created === void 0) { created = null; }
        this.id = id;
        this.contarctorId = contarctorId;
        this.contarctorName = contarctorName;
        this.bankName = bankName;
        this.accountNumber = accountNumber;
        this.created = created;
    }
    ContractorBankAccountTableModel.prototype.getDescriptions = function () {
        return {
            contarctorName: "CONTRACTORS.CONTRACTORBANKACCOUNTTABLE.TABLE_CONTRACTOR_NAME_DESCRIPTION", bankName: "CONTRACTORS.CONTRACTORBANKACCOUNTTABLE.TABLE_BANK_NAME_DESCRIPTION",
            accountNumber: "CONTRACTORS.CONTRACTORBANKACCOUNTTABLE.TABLE_ACCOUNT_NUMBER_DESCRIPTION", created: "CONTRACTORS.CONTRACTORBANKACCOUNTTABLE.TABLE_CREATED_DESCRIPTION"
        };
    };
    return ContractorBankAccountTableModel;
}());
exports.ContractorBankAccountTableModel = ContractorBankAccountTableModel;
//# sourceMappingURL=ContractorBankAccount.model.js.map