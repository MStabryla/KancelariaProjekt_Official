"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.AuthenticationService = void 0;
var http_1 = require("@angular/common/http");
var environment_1 = require("./../../../environments/environment");
var rxjs_1 = require("rxjs");
var operators_1 = require("rxjs/operators");
var custom_encoder_1 = require("../custom-encoder");
var AuthenticationService = /** @class */ (function () {
    function AuthenticationService(_http, _jwtHelper) {
        var _this = this;
        this._http = _http;
        this._jwtHelper = _jwtHelper;
        // User related properties
        this.loginStatus = new rxjs_1.BehaviorSubject(this.checkLoginStatus());
        this.UserName = new rxjs_1.BehaviorSubject(localStorage.getItem('username'));
        this.UserRole = new rxjs_1.BehaviorSubject(localStorage.getItem('userRole'));
        this._envUrl = environment_1.environment.urlAddress;
        this.urlAntiForgeryToken = "/api/authentication/antiforgery";
        //Login Method
        this.loginUser = function (route, body) {
            return _this._http.post(_this.createCompleteRoute(route, _this._envUrl), body).pipe(operators_1.map(function (result) {
                if (result && result.token) {
                    var decoded = _this._jwtHelper.decodeToken(result.token);
                    _this.loginStatus.next(true);
                    localStorage.setItem('loginStatus', '1');
                    localStorage.setItem('jwt', result.token);
                    localStorage.setItem('username', result.username);
                    localStorage.setItem('expiration', result.expiration);
                    localStorage.setItem('userRole', result.userRole);
                    _this.UserName.next(localStorage.getItem('username'));
                    _this.UserRole.next(localStorage.getItem('userRole'));
                }
                return result;
            }));
        };
        this.logout = function () {
            // Set Loginstatus to false and delete saved jwt cookie
            _this.loginStatus.next(false);
            localStorage.removeItem('jwt');
            localStorage.removeItem('userRole');
            localStorage.removeItem('username');
            localStorage.removeItem('expiration');
            localStorage.setItem('loginStatus', '0');
            //this.router.navigate(['/login']);
        };
        this.registerUser = function (route, body) {
            return _this._http.post(_this.createCompleteRoute(route, _this._envUrl), body);
        };
        this.forgotPassword = function (route, body) {
            return _this._http.post(_this.createCompleteRoute(route, _this._envUrl), body);
        };
        this.resetPassword = function (route, body) {
            return _this._http.post(_this.createCompleteRoute(route, _this._envUrl), body);
        };
        this.confirmEmail = function (route, token, login) {
            var params = new http_1.HttpParams({ encoder: new custom_encoder_1.CustomEncoder() });
            params = params.append('token', token);
            params = params.append('login', login);
            return _this._http.get(_this.createCompleteRoute(route, _this._envUrl), { params: params });
        };
        this.createCompleteRoute = function (route, envAddress) {
            return envAddress + "/" + route;
        };
    }
    AuthenticationService.prototype.antiForgery = function () {
        this._http.get(this.urlAntiForgeryToken).subscribe();
    };
    AuthenticationService.prototype.checkLoginStatus = function () {
        var loginCookie = localStorage.getItem("loginStatus");
        if (loginCookie === "1") {
            if (localStorage.getItem('jwt') === null || localStorage.getItem('jwt') === undefined) {
                return false;
            }
            // Get and Decode the Token
            var token = localStorage.getItem('jwt');
            var decoded = this._jwtHelper.decodeToken(token);
            // Check if the cookie is valid
            if (decoded.exp === undefined) {
                return false;
            }
            // Get Current Date Time
            var date = new Date(0);
            // Convert EXp Time to UTC
            var tokenExpDate = date.setUTCSeconds(decoded.exp);
            // If Value of Token time greter than 
            if (tokenExpDate.valueOf() > new Date().valueOf()) {
                return true;
            }
            return false;
        }
        return false;
    };
    Object.defineProperty(AuthenticationService.prototype, "isLoggesIn", {
        get: function () {
            return this.loginStatus.asObservable();
        },
        enumerable: false,
        configurable: true
    });
    Object.defineProperty(AuthenticationService.prototype, "currentUserName", {
        get: function () {
            return this.UserName.asObservable();
        },
        enumerable: false,
        configurable: true
    });
    Object.defineProperty(AuthenticationService.prototype, "currentUserRole", {
        get: function () {
            return this.UserRole.asObservable();
        },
        enumerable: false,
        configurable: true
    });
    return AuthenticationService;
}());
exports.AuthenticationService = AuthenticationService;
//# sourceMappingURL=authentication.service.js.map
