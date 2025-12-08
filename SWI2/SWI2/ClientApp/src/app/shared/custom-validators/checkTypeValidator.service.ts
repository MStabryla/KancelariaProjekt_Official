import { Injectable } from '@angular/core';
import { AbstractControl, ValidatorFn } from '@angular/forms';

export function checkType(type): ValidatorFn {
  return (control: AbstractControl): { [key: string]: any } | null => {
    if (typeof control.value != type) {
      return { wrongType: typeof control.value };
    }
    return null;
  };
}
