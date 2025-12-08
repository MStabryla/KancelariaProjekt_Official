import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthenticationService } from './authentication.service';

@Injectable()
export class AntiForgeryService implements HttpInterceptor {
  private aftoken = "X-XSRF-TOKEN";
  constructor (private _authService: AuthenticationService)
  {
  }
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    
    const requestToken = this.getCookieValue(this.aftoken);
    return next.handle(req.clone({
      headers: req.headers.set(this.aftoken, requestToken)
    }));
  }

  private getCookieValue(cookieName: string) {
    const allCookies = decodeURIComponent(document.cookie).split("; ");
    for (let i = 0; i < allCookies.length; i++) {
      const cookie = allCookies[i];
      if (cookie.startsWith(cookieName + "=")) {
        return cookie.substring(cookieName.length + 1);
      }
    }
    return "";
  }
} 
