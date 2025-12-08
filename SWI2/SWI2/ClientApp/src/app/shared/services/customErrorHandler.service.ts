import { HttpParams } from '@angular/common/http';
import { ErrorHandler, Injectable } from '@angular/core';
import { type } from 'os';
import { ApiService } from './api.service';

@Injectable()
export class CustomErrorHandlerService extends ErrorHandler {

  constructor(private _api: ApiService) {
    super();
  }

  handleError(error: Error) {
    this._api.post(`log`, new HttpParams().set('exception', error.message));
  }
}
