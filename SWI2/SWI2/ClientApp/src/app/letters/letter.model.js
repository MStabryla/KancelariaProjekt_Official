"use strict";
var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (Object.prototype.hasOwnProperty.call(b, p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
exports.LetterModel = void 0;
var basic_model_1 = require("../shared/models/basic-model");
var LetterModel = /** @class */ (function (_super) {
    __extends(LetterModel, _super);
    function LetterModel() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    return LetterModel;
}(basic_model_1.BasicModelClass));
exports.LetterModel = LetterModel;
//# sourceMappingURL=letter.model.js.map