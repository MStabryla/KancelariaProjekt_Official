"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.MailLanguage = exports.Contractor = void 0;
var Contractor = /** @class */ (function () {
    function Contractor(obj) {
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
    return Contractor;
}());
exports.Contractor = Contractor;
var MailLanguage;
(function (MailLanguage) {
    MailLanguage[MailLanguage["PL"] = 0] = "PL";
    MailLanguage[MailLanguage["EN"] = 1] = "EN";
    MailLanguage[MailLanguage["IT"] = 2] = "IT";
})(MailLanguage = exports.MailLanguage || (exports.MailLanguage = {}));
//# sourceMappingURL=Contractor.model.js.map