"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ContractorPaymentMachingModel = exports.ContractorBankAccountPaymentMachingModel = exports.PaymentMachingModel = exports.PaymentsForInvoices = exports.InvoicePaymentFormModel = exports.PaymentsForInvoicesFormModel = exports.ContractorBankAccountPaymentFormModel = exports.ContractorPaymentFormModel = exports.PaymentFormModel = exports.PaymentModel = exports.PaymentTableModel = void 0;
var model_helper_1 = require("../../shared/helpers/model-helper");
var Base_model_1 = require("../Base.model");
var PaymentTableModel = /** @class */ (function () {
    function PaymentTableModel(obj) {
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
        }
        else {
            Object.assign(this, obj);
        }
    }
    return PaymentTableModel;
}());
exports.PaymentTableModel = PaymentTableModel;
var PaymentModel = /** @class */ (function () {
    function PaymentModel(obj) {
        this.id = 0;
        this.contractorBankAccount = new Base_model_1.BaseModel();
        this.paymentsForInvoices = [new PaymentsForInvoices()];
        this.topic = '';
        this.paymentValue = 0;
        this.currency = '';
        this.paymentDate = new Date();
        model_helper_1.AssignToModel(this, obj);
    }
    return PaymentModel;
}());
exports.PaymentModel = PaymentModel;
var PaymentFormModel = /** @class */ (function () {
    function PaymentFormModel() {
    }
    return PaymentFormModel;
}());
exports.PaymentFormModel = PaymentFormModel;
var ContractorPaymentFormModel = /** @class */ (function () {
    function ContractorPaymentFormModel(obj) {
        this.id = 0;
        this.name = '';
        this.nip = '';
        Object.assign(this, obj);
    }
    return ContractorPaymentFormModel;
}());
exports.ContractorPaymentFormModel = ContractorPaymentFormModel;
var ContractorBankAccountPaymentFormModel = /** @class */ (function () {
    function ContractorBankAccountPaymentFormModel(obj) {
        this.id = 0;
        this.bankName = '';
        this.accountNumber = '';
        Object.assign(this, obj);
    }
    return ContractorBankAccountPaymentFormModel;
}());
exports.ContractorBankAccountPaymentFormModel = ContractorBankAccountPaymentFormModel;
var PaymentsForInvoicesFormModel = /** @class */ (function () {
    function PaymentsForInvoicesFormModel(obj) {
        this.paymentValueForInvoice = 0;
        Object.assign(this, obj);
    }
    return PaymentsForInvoicesFormModel;
}());
exports.PaymentsForInvoicesFormModel = PaymentsForInvoicesFormModel;
var InvoicePaymentFormModel = /** @class */ (function () {
    function InvoicePaymentFormModel(obj) {
        Object.assign(this, obj);
    }
    return InvoicePaymentFormModel;
}());
exports.InvoicePaymentFormModel = InvoicePaymentFormModel;
var PaymentsForInvoices = /** @class */ (function () {
    function PaymentsForInvoices(obj) {
        this.id = 0;
        this.invoice = new Base_model_1.BaseModel();
        this.paymentValueForInvoice = 0;
        Object.assign(this, obj);
    }
    return PaymentsForInvoices;
}());
exports.PaymentsForInvoices = PaymentsForInvoices;
var PaymentMachingModel = /** @class */ (function () {
    function PaymentMachingModel() {
    }
    return PaymentMachingModel;
}());
exports.PaymentMachingModel = PaymentMachingModel;
var ContractorBankAccountPaymentMachingModel = /** @class */ (function () {
    function ContractorBankAccountPaymentMachingModel() {
    }
    return ContractorBankAccountPaymentMachingModel;
}());
exports.ContractorBankAccountPaymentMachingModel = ContractorBankAccountPaymentMachingModel;
var ContractorPaymentMachingModel = /** @class */ (function () {
    function ContractorPaymentMachingModel() {
    }
    return ContractorPaymentMachingModel;
}());
exports.ContractorPaymentMachingModel = ContractorPaymentMachingModel;
//# sourceMappingURL=Payment.model.js.map