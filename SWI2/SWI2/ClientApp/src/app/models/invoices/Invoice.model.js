"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.InvoiceSendeds = exports.SellDateName = exports.InvoiceEntries = exports.InvoiceIssuer = exports.InvoiceContractor = exports.InvoiceTable = exports.PaymentsForInvoices = exports.Invoice = void 0;
var model_helper_1 = require("../../shared/helpers/model-helper");
var Invoice = /** @class */ (function () {
    function Invoice(obj) {
        this.id = 0;
        this.number = '';
        this.nettoWorth = 0;
        this.bruttoWorth = 0;
        this.correcting = '';
        this.paymentStatus = '';
        this.paymentCurrency = '';
        this.paymentsForInvoices = [new PaymentsForInvoices()];
        this.created = new Date();
        this.creationPlace = '';
        this.paymentAccountNumber = '';
        this.paymentBank = '';
        this.note = '';
        this.sellDateName = new SellDateName();
        this.sellDate = new Date();
        this.paymentDate = new Date();
        this.isTransferType = false;
        this.invoiceIssuer = new InvoiceIssuer();
        this.invoiceContractor = new InvoiceContractor();
        this.invoiceEntries = [new InvoiceEntries()];
        this.invoiceSendeds = [new InvoiceSendeds()];
        this.rate = 1;
        if (obj)
            model_helper_1.AssignToModel(this, obj);
    }
    return Invoice;
}());
exports.Invoice = Invoice;
var PaymentsForInvoices = /** @class */ (function () {
    function PaymentsForInvoices() {
        this.id = 0;
        this.payment = { id: 0 };
        this.paymentValueForInvoice = 0;
    }
    return PaymentsForInvoices;
}());
exports.PaymentsForInvoices = PaymentsForInvoices;
var InvoiceTable = /** @class */ (function () {
    function InvoiceTable() {
    }
    return InvoiceTable;
}());
exports.InvoiceTable = InvoiceTable;
var InvoiceContractor = /** @class */ (function () {
    function InvoiceContractor(obj) {
        this.name = '';
        this.street = '';
        this.houseNumber = '';
        this.apartamentNumber = '';
        this.postalcode = '';
        this.city = '';
        this.country = '';
        this.nip = '';
        this.contractor = { id: 0, name: '' /*, contractorBankAccounts :[]*/ };
        this.postoffice = '';
        this.id = 0;
        this.created = new Date();
        if (obj)
            model_helper_1.AssignToModel(this, obj);
    }
    return InvoiceContractor;
}());
exports.InvoiceContractor = InvoiceContractor;
var InvoiceIssuer = /** @class */ (function () {
    function InvoiceIssuer() {
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
    return InvoiceIssuer;
}());
exports.InvoiceIssuer = InvoiceIssuer;
var InvoiceEntries = /** @class */ (function () {
    function InvoiceEntries() {
        this.id = 0;
        this.gtu = null;
        this.name = '';
        this.price = 1;
        this.quantity = 1;
        this.vat = 23;
    }
    return InvoiceEntries;
}());
exports.InvoiceEntries = InvoiceEntries;
var SellDateName = /** @class */ (function () {
    function SellDateName() {
        this.id = null;
        this.name = null;
    }
    return SellDateName;
}());
exports.SellDateName = SellDateName;
var InvoiceSendeds = /** @class */ (function () {
    function InvoiceSendeds() {
        this.id = null;
        this.email = null;
        this.user = { userName: null };
        this.created = null;
        this.number = null;
    }
    return InvoiceSendeds;
}());
exports.InvoiceSendeds = InvoiceSendeds;
//# sourceMappingURL=Invoice.model.js.map