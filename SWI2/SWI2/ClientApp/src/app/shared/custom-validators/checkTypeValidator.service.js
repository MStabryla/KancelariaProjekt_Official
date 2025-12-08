"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.checkType = void 0;
function checkType(type) {
    return function (control) {
        if (typeof control.value != type) {
            return { wrongType: typeof control.value };
        }
        return null;
    };
}
exports.checkType = checkType;
//# sourceMappingURL=checkTypeValidator.service.js.map