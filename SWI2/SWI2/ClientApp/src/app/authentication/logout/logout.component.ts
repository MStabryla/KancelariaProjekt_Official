import { LoginUser } from './../../models/authentication/login/loginUser.model';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthenticationService } from './../../shared/services/authentication.service';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'swi-logout',
  templateUrl: './logout.component.html',
  styleUrls: ['./logout.component.css']
})
export class LogoutComponent implements OnInit {
  private isLogout: boolean =false
  constructor(private _authService: AuthenticationService, private _router: Router) { }

  ngOnInit(): void {


    (async () => {
      this._authService.logout();
      this.isLogout = true;

      this._router.navigateByUrl('', { skipLocationChange: true }).then(() => {
        this._router.navigate(['']);
      }); 

    })();

  }

}
function delay(ms: number) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

