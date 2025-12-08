import { Injectable } from '@angular/core';
import { HttpClient, HttpEvent, HttpRequest } from '@angular/common/http';
import { Router } from '@angular/router'
import { from, Observable } from 'rxjs';


//@Injectable służy do automatycznego wstawiania instacji danego obiektu do kontruktorów komponentów
@Injectable()
export class ApiService {
  private urlAntiForgeryToken = "/api/antiforgery";
  private wait = false;

  constructor(private http: HttpClient, private router: Router) {

  }
  // zapytanie get do okreslonej ścieszki z parametrami
  private async antiForgery<T>(next: Observable<T>): Promise<T> /*Observable<T>*/ {
    await this.http.get<T>(this.urlAntiForgeryToken).toPromise();
    return next.toPromise();
    //return next();
  }
  private async antiForgeryEvent<T>(next: Observable<HttpEvent<T>>): Promise<HttpEvent<T>> /*Observable<T>*/ {
    await this.http.get<T>(this.urlAntiForgeryToken).toPromise();
    return next.toPromise();
    //return next();
  }

  get<T>(path: any, params: any = null): Observable<T> {
    const respons = from(this.antiForgery<T>(this.http.get<T>(path, {
        params: params
    })
    ))
    return respons;
  }
  // zapytanie post do okreslonej ścieszki z parametrami i danymi wysyłanymi przez post

  post<T>(path: any, data: any, params: any = null, headers: any = null): Observable<T> {
    return from(this.antiForgery<T>(this.http.post<T>(path, data, {
      params: params,
      headers:headers
      })
    ))
    
  }
  put<T>(path: any, data: any, params: any = null): Observable<T> {
    return from(this.antiForgery<T>(this.http.put<T>(path, data, {
        params: params
      })
    ))
    
  }
  delete<T>(path: any, params: any = null): Observable<T> {
    return from(this.antiForgery<T>(this.http.delete<T>(path, {
        params: params
      })
    ))
    
  }

  downloadFile<T>(path: any, params: any = null): Observable<HttpEvent<T>> {
    return from(this.antiForgeryEvent<T>(this.http.request<T>(new HttpRequest('GET', path, {
        reportProgress: true,
        responseType: 'blob'
      }))
    ));
    
  }

  uploadFile<T>(file: Blob): Observable<HttpEvent<void>> {
    const formData = new FormData();
    formData.append('file', file);

    return null;
  }
}
