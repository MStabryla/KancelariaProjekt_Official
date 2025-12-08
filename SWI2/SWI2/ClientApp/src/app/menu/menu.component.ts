import { Component, HostListener, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AuthenticationService } from './../shared/services/authentication.service';


@Component({
  selector: 'swi-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.css']
})
export class MenuComponent implements OnInit {
  LoginStatus$: Observable<boolean>;
  UserName$: Observable<string>;
  UserCompanys$: Observable<Array<any>>;
  UserRole$: Observable<string>;
  selectedCompany: object;
  screenWidth: number;
  CompanySelected$: Observable<object>;
  constructor(private _authService: AuthenticationService,
  public translate : TranslateService) {
  }

  ngOnInit() {
    this.translate.addLangs(["PL", "EN", "IT"]);
    this.translate.setDefaultLang("PL");


    this.LoginStatus$ = this._authService.isLoggesIn;
    this.CompanySelected$ = this._authService.currentSelectedCompany;
    this.UserName$ = this._authService.currentUserName;
    this.UserCompanys$ = this._authService.currentUserCompanys;
    this.UserRole$ = this._authService.currentUserRole;
    this.UserCompanys$.subscribe(c => { if (c && c[0]) this.selectedCompany = c[0] });
    this.screenWidth = window.innerWidth;
  }

  @HostListener('window:resize', ['$event'])
  onResize(event) {
    this.screenWidth = window.innerWidth;
  }

  ChangeSelectCompany(company) {
    this._authService.setSelectedCompany(company.value);
  }

  onLogout() {
    this._authService.logout();
  }
}


