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
exports.MessageTemplateModel = exports.MessageModel = void 0;
var model_helper_1 = require("../shared/helpers/model-helper");
var basic_model_1 = require("../shared/models/basic-model");
var MessageModel = /** @class */ (function (_super) {
    __extends(MessageModel, _super);
    function MessageModel(rawData) {
        var _this = _super.call(this, rawData) || this;
        _this.posted = rawData["posted"] ? new Date(rawData["posted"]) : null;
        _this.readed = rawData["readed"] ? new Date(rawData["readed"]) : null;
        return _this;
    }
    return MessageModel;
}(basic_model_1.BasicModelClass));
exports.MessageModel = MessageModel;
var MessageTemplateModel = /** @class */ (function () {
    function MessageTemplateModel(obj) {
        this.id = 0;
        this.title = '';
        this.message = '';
        this.created = new Date();
        this.updated = new Date();
        if (obj)
            model_helper_1.AssignToModel(this, obj);
    }
    return MessageTemplateModel;
}());
exports.MessageTemplateModel = MessageTemplateModel;
//# sourceMappingURL=message.model.js.map