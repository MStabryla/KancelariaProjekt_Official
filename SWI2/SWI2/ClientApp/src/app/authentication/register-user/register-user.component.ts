import { UserRegistration } from '../../models/authentication/register-user/userRegistration.model';
import { AuthenticationService } from './../../shared/services/authentication.service';
import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { PasswordConfirmationValidatorService } from './../../shared/custom-validators/passwordConfirmationValidator.service';
import { environment } from '../../../environments/environment';
import { Console } from 'console';
import { MatDialog } from '@angular/material/dialog';
import { AskIfDialog } from '../../shared/components/askIfDialog/askIfDialog.component';

@Component({
  selector: 'app-register-user',
  templateUrl: './register-user.component.html',
  styleUrls: ['./register-user.component.css']
})
export class RegisterUserComponent implements OnInit {
  public registerForm: FormGroup;
  public errorMessage = '';
  public showError: boolean;
  constructor(private _authService: AuthenticationService, private _passConfValidator: PasswordConfirmationValidatorService,
    private _router: Router,
    public dialog: MatDialog) {
    this.registerForm = new FormGroup({});
    
  }
  ngOnInit(): void {
    this.registerForm = new FormGroup({
      name: new FormControl(''),
      surname: new FormControl(''),
      login: new FormControl(''),
      email: new FormControl('', [Validators.required, Validators.email]),
      password: new FormControl('', [Validators.required]),
      language: new FormControl(''),
      confirm: new FormControl('')
    });
    
  }
  public validateControl = (controlName: string) => {
    return this.registerForm.controls[controlName].invalid && this.registerForm.controls[controlName].touched
  }
  public hasError = (controlName: string, errorName: string) => {
    return this.registerForm.controls[controlName].hasError(errorName)
  }
  public registerUser = (registerFormValue) => {
    const formValues = { ...registerFormValue };
    const user: UserRegistration = {
      name: formValues.name,
      surname: formValues.surname,
      login: formValues.login,
      email: formValues.email,
      password: formValues.password,
      confirmPassword: formValues.confirm,
      language: formValues.language,
      clientURI: environment.urlAddress + '/authentication/emailconfirmation'
    };
    this._authService.registerUser("api/authentication/register", user)
      .subscribe(result => {
        const userRegisterdDialog = this.dialog.open(AskIfDialog, {
          disableClose: true,
          data: { data: "Użytkownik zarejestrowany. Potwierdź maila by się zalogować.", close: "Zamknij", ok: null }
        });
        userRegisterdDialog.afterClosed().subscribe(() => {
          this._router.navigate(["/authentication/login"]);
        });
      },
        error => {
          this.errorMessage = '';
          if (Array.isArray(error.error.errors))
            for (let i = 0; i < error.error.errors.length; i++)
              this.errorMessage += error.error.errors[i] + ' ';
          else
            Object.entries(error.error.errors).forEach(([key, val]) =>
              this.errorMessage += val + ' ');
          this.showError = true;
        });
  }
}
