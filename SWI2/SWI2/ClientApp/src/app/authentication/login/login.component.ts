import { LoginUser } from './../../models/authentication/login/loginUser.model';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthenticationService } from './../../shared/services/authentication.service';
import { Component, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  public loginForm: FormGroup;
  public errorMessage: string = '';
  public showError: boolean;
  public isLoadingResults: boolean;
  private _returnUrl: string;

  constructor(private _authService: AuthenticationService, private _router: Router, private _route: ActivatedRoute) { }

  ngOnInit(): void {
    this.loginForm = new FormGroup({
      username: new FormControl("", [Validators.required]),
      password: new FormControl("", [Validators.required])
    })

    this._returnUrl = this._route.snapshot.queryParams['returnUrl'] || '/';
  }

  public validateControl = (controlName: string) => {
    return this.loginForm.controls[controlName].invalid && this.loginForm.controls[controlName].touched
  }

  public hasError = (controlName: string, errorName: string) => {
    return this.loginForm.controls[controlName].hasError(errorName)
  }

  public loginUser = (loginFormValue) => {
    this.showError = false;
    const login = { ...loginFormValue };
    const userForAuth: LoginUser = {
      login: login.username,
      password: btoa(login.password)
    }
    this.isLoadingResults = true;
    this._authService.loginUser('api/authentication/login', userForAuth)
      .subscribe(() => {
        this._router.navigateByUrl(this._returnUrl);
        this.isLoadingResults = true;
      },
      (error) => {
        if (error.status === 404) {
            
          this._router.navigateByUrl(this._returnUrl +'/accessdenied');
        }
        else {
          this.errorMessage = error.error;
          this.showError = true;
        }
        this.isLoadingResults = false;
      })
  }
}
