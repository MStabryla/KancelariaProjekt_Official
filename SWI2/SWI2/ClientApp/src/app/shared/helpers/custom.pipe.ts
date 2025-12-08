import { Pipe, PipeTransform } from '@angular/core';
import * as moment from 'moment';
import { getCurrencySymbol } from '@angular/common';

@Pipe({
  name: 'currencySymbol'
})
export class CurrencySymbolPipe implements PipeTransform {

  transform(
    code: string,
    format: 'wide' | 'narrow' = 'narrow',
    locale?: string
  ): any {
    return getCurrencySymbol(code, format, locale);
  }
}

@Pipe({
  name: 'mapone'
})
export class MapOne implements PipeTransform {

  transform(value: object[], keys: string[]): any {
    return value.map(i => { let one = i; keys.forEach(k => { one = one[k] }); return one; });
  }

}

@Pipe({
  name: 'typeof'
})
export class TypeofPipe implements PipeTransform {

  transform(value: any): any {
    if (Array.isArray(value)) {
      return 'array';
    }
    else {
      return typeof value;
    }
  }

}

@Pipe({
  name: 'split'
})
export class SplitPipe implements PipeTransform {

  transform(text: string, by: string, index: number = 1) {
    if (text != null) {
      let arr = text.split(by);
      return arr[index];
    } else {
      return null;
    }
  }
}
@Pipe({
  name: 'instanceofDate'
})
export class InstanceofDatePipe implements PipeTransform {

  transform(value: any): any {
    if (typeof value != 'number' && moment(value, moment.ISO_8601, true).isValid()) {
      return true;
    }
    else return false;
  }

}
