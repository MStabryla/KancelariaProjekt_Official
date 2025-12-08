"use strict";
var __assign = (this && this.__assign) || function () {
    __assign = Object.assign || function(t) {
        for (var s, i = 1, n = arguments.length; i < n; i++) {
            s = arguments[i];
            for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
                t[p] = s[p];
        }
        return t;
    };
    return __assign.apply(this, arguments);
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.AssignToModel = void 0;
var moment_1 = require("moment");
function AssignToModel(to, from) {
    Object.entries(to).forEach(function (_a) {
        var k = _a[0], v = _a[1];
        if (from && from.hasOwnProperty(k)) {
            if (Array.isArray(from[k])) {
                var newElement_1 = __assign({}, v[0]);
                delete v[0];
                Object.entries(from[k]).forEach(function (_a) {
                    var ak = _a[0], av = _a[1];
                    v[ak] = __assign({}, newElement_1);
                    AssignToModel(v[ak], av);
                });
            }
            else {
                if (from[k] instanceof Object) {
                    if (moment_1.isMoment(from[k])) {
                        to[k] = from[k].format('YYYY-MM-DDTHH:mm:ss');
                    }
                    else {
                        to[k] = __assign({}, to[k]);
                        AssignToModel(to[k], from[k]);
                    }
                }
                else {
                    to[k] = from[k];
                }
            }
        }
        else {
            delete to[k];
        }
    });
}
exports.AssignToModel = AssignToModel;
;
//# sourceMappingURL=model-helper.js.map