"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.BasicModelClass = void 0;
var BasicModelClass = /** @class */ (function () {
    function BasicModelClass(rawData) {
        var _this = this;
        Object.keys(rawData).forEach(function (x) {
            _this[x] = rawData[x];
        });
    }
    return BasicModelClass;
}());
exports.BasicModelClass = BasicModelClass;
//# sourceMappingURL=basic-model.js.map