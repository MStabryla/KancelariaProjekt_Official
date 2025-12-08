import { ResetPassword } from './../../models/authentication/forgot-password/resetPassword.model';
import { ForgotPassword } from './../../models/authentication/forgot-password/forgotPassword.model';
import { LoginResponse } from './../../models/authentication/login/loginResponse.model';
import { RegistrationResponse } from './../../models/authentication/register-user/registrationResponse.model';
import { LoginUser } from './../../models/authentication/login/loginUser.model';
import { UserRegistration } from './../../models/authentication/register-user/userRegistration.model';
import { HttpParams } from '@angular/common/http';
import { ApiService } from './api.service'
import { Injectable } from '@angular/core';
import { environment } from './../../../environments/environment';
import { BehaviorSubject, Subject } from 'rxjs';
import { map } from 'rxjs/operators';
import { JwtHelperService } from '@auth0/angular-jwt';
import { CustomEncoder } from '../helpers/custom-encoder';

@Injectable({
  providedIn: 'root'
})

export class AuthenticationService {

  // User related properties
  private loginStatus = new BehaviorSubject<boolean>(this.checkLoginStatus());
  private UserName = new BehaviorSubject<string>(localStorage.getItem('userName'));
  private UserRole = new BehaviorSubject<string>(localStorage.getItem('userRole'));
  private UserCompanys = new BehaviorSubject<Array<object>>(this.generateUserCompanys());
  public SelectedCompany = new BehaviorSubject<any>(this.generateSelectedCompany());
  private _envUrl: string = environment.urlAddress;

  


  constructor(private _api: ApiService,
    private _jwtHelper: JwtHelperService) { }

  
  //Login Method
  public loginUser = (route: string, body: LoginUser) => {
    return this._api.post<any>(this.createCompleteRoute(route, this._envUrl), body).pipe(
      map(result => {

        if (result && result.token) {
          const decoded = this._jwtHelper.decodeToken(result.token);

          localStorage.setItem('loginStatus', '1');
          localStorage.setItem('jwt', result.token);
          localStorage.setItem('userName', decoded.sub);
          localStorage.setItem('expiration', decoded.exp);
          localStorage.setItem('userRole', decoded.role);
          localStorage.setItem('companys', decoded.companys);

          document.cookie = "jwt=" + result.token;

          this.UserCompanys.next(this.generateUserCompanys());
          this.SelectedCompany.next(this.generateSelectedCompany());
          this.loginStatus.next(true);
          this.UserName.next(localStorage.getItem('userName'));
          this.UserRole.next(localStorage.getItem('userRole'));
        }
        return result;

      })

    );
  }
  public logout() {
    if (this.loginStatus)
      this.loginStatus.next(false);

    if (this.UserCompanys)
      this.UserCompanys.next(null);

    if (this.UserName)
      this.UserName.next(null);

    if (this.UserRole)
      this.UserRole.next(null);

    if (this.SelectedCompany)
      this.SelectedCompany.next({});

    localStorage.clear();
    localStorage.setItem('loginStatus', '0');

  }

  checkLoginStatus(): boolean {
    const login = localStorage.getItem("loginStatus");

    if (login === "1") {
      if (localStorage.getItem('jwt') === null || localStorage.getItem('jwt') === undefined) {
        return false;
      }

      const exp = localStorage.getItem("expiration");
      if (exp === undefined || exp === null) {
        return false;
      }

      const date = new Date(0);
      let tokenExpDate = date.setUTCSeconds(Number(exp));
      if (tokenExpDate.valueOf() > new Date().valueOf()) {
        return true;
      } else {
        this.logout();
        return false;
      }
    }
    return false;
  }

  public registerUser = (route: string, body: UserRegistration) => {
    return this._api.post<RegistrationResponse>(this.createCompleteRoute(route, this._envUrl), body);
  }

  public forgotPassword = (route: string, body: ForgotPassword) => {
    return this._api.post(this.createCompleteRoute(route, this._envUrl), body);
  }

  public resetPassword = (route: string, body: ResetPassword) => {
    return this._api.post(this.createCompleteRoute(route, this._envUrl), body);
  }

  public confirmEmail = (route: string, token: string, login: string) => {
    let params = new HttpParams({ encoder: new CustomEncoder() })
    params = params.append('token', token);
    params = params.append('login', login);

    return this._api.get(this.createCompleteRoute(route, this._envUrl),params);
  }

  generateUserCompanys(): Array<object> {
    let companys = localStorage.getItem('companys');
    if (companys !== '' && companys !== null) {
      let userArrey = [];
      companys.split("_").forEach((value) => {
        let counter = 0;
        let company = {};
        companys.split("|").forEach((value) => {
          if (counter === 0) {
            company['id'] = Number(value);
          } else if (counter === 1) {
            company['name'] = value;
          } else {
            company['isInBoard'] = Boolean(value);
          }
          counter++;
        });

        userArrey.push(company);
      });
      localStorage.setItem('company', userArrey[0].id + '|' + userArrey[0].name + '|' + userArrey[0].isInBoard);
      return userArrey;
    } else {
      const company = localStorage.getItem('company');
      if (localStorage.getItem('userRole') === "Administrator" && company != '' && company != null) {
        const companyObj = company.split('|');
        return [{ id: companyObj[0], name: companyObj[1], isInBoard: companyObj[2] }];
      } else {
        return [];
      }
      
    }

  }

  generateSelectedCompany(): object {
    var localCompany = localStorage.getItem('company');
    if (localCompany != null) {
      var counter = 0;
      var company = {};
      localCompany.split("|").forEach((value) => {
        if (counter == 0) {
          company['id'] = value;
        } else if (counter == 1) {
          company['name'] = value;
        } else {
          company['isInBoard'] = Boolean(value);
        }
        counter++;
      });
      return company;
    } else {
      return {};
    }
  }

  get isLoggesIn() {
    return this.loginStatus.asObservable();
  }

  setSelectedCompany(company) {
    this.currentUserRole.subscribe(role => {
      if (role === "Administrator") {
        this.UserCompanys.next([company]);
      }
    });
    this.SelectedCompany.next(company);
    localStorage.setItem('company', company.id + '|' + company.name + '|' + company.isInBoard);

  }

  get currentUserName() {
    return this.UserName.asObservable();
  }

  get currentUserRole() {
    return this.UserRole.asObservable();
  }
  get currentUserCompanys() {
    return this.UserCompanys.asObservable();
  }
  get currentSelectedCompany() {
    return this.SelectedCompany.asObservable();
  }
  private createCompleteRoute = (route: string, envAddress: string) => {
    return `${envAddress}/${route}`;
  }
}

