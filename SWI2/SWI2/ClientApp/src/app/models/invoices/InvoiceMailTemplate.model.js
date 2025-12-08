"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.InvoiceMailTemplate = void 0;
var model_helper_1 = require("../../shared/helpers/model-helper");
var InvoiceMailTemplate = /** @class */ (function () {
    function InvoiceMailTemplate(obj) {
        this.created = new Date();
        this.id = 0;
        this.mailLanguage = 0;
        this.message = '';
        this.title = '';
        if (obj)
            model_helper_1.AssignToModel(this, obj);
    }
    return InvoiceMailTemplate;
}());
exports.InvoiceMailTemplate = InvoiceMailTemplate;
//# sourceMappingURL=InvoiceMailTemplate.model.js.map