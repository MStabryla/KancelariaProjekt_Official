"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.DateAdapter = void 0;
var DateAdapter = /** @class */ (function () {
    function DateAdapter() {
    }
    DateAdapter.prototype.toUTCDate = function (date) {
        return date.getUTCFullYear() + '-' +
            ('0' + (date.getUTCMonth() + 1)).slice(-2) + '-' +
            ('0' + date.getUTCDate()).slice(-2);
    };
    return DateAdapter;
}());
exports.DateAdapter = DateAdapter;
//# sourceMappingURL=date-adapter.js.map