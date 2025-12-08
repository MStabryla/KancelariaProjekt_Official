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
exports.FileModel = void 0;
var basic_model_1 = require("../shared/models/basic-model");
var FileModel = /** @class */ (function (_super) {
    __extends(FileModel, _super);
    function FileModel(rawData) {
        var _a;
        var _this = _super.call(this, rawData) || this;
        _this.created = new Date();
        _this.modified = new Date();
        _this.path = (_a = rawData["fullName"]) !== null && _a !== void 0 ? _a : rawData["path"];
        _this.type = rawData["type"] === 1 ? "dir" : rawData["type"] === 0 ? "file" : "link";
        return _this;
    }
    return FileModel;
}(basic_model_1.BasicModelClass));
exports.FileModel = FileModel;
//# sourceMappingURL=file.model.js.map