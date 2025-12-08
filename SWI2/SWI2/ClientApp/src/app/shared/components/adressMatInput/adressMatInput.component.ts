import { FocusMonitor } from '@angular/cdk/a11y';
import { coerceBooleanProperty } from '@angular/cdk/coercion';
import {
  Component,
  ElementRef,
  Inject,
  Input,
  OnDestroy,
  Optional,
  Self,
  ViewChild
} from '@angular/core';
import {
  AbstractControl,
  ControlValueAccessor,
  FormBuilder,
  FormControl,
  FormGroup,
  NgControl,
  Validators
} from '@angular/forms';
import { MAT_FORM_FIELD, MatFormField, MatFormFieldControl } from '@angular/material/form-field';
import { Subject } from 'rxjs';


/** Data structure for holding telephone number. */
export class Adress {
  constructor(
    public street: string ='',
    public houseNumber: string ='',
    public apartamentNumber: string='',
    public city: string='',
  ) { }
}

/** Custom `MatFormFieldControl` for telephone number input. */
@Component({
  selector: 'adress-input',
  templateUrl: 'adressMatInput.component.html',
  styleUrls: ['adressMatInput.component.css'],
  providers: [{ provide: MatFormFieldControl, useExisting: AdressMatInput }]
})
export class AdressMatInput
  implements ControlValueAccessor, MatFormFieldControl<Adress>, OnDestroy {
  @ViewChild('street') streetInput: ElementRef;
  @ViewChild('houseNumber') houseNumberInput: ElementRef;
  @ViewChild('apartamentNumber') apartamentNumberInput: ElementRef;
  @ViewChild('city') cityInput: ElementRef;
  static nextId = 0;
  parts: FormGroup;
  stateChanges = new Subject<void>();
  focused = false;
  onChange = (_: any) => { };
  onTouched = () => { };
  id = `adress-input-${AdressMatInput.nextId++}`;

  get empty() {
    const {
      value: { street, houseNumber, apartamentNumber, city}
    } = this.parts;

    return !street && !houseNumber && !apartamentNumber && !city;
  }

  get shouldLabelFloat() {
    return this.focused || !this.empty;
  }

  @Input()
  get placeholder(): string {
    return this._placeholder;
  }
  set placeholder(value: string) {
    this._placeholder = value;
    this.stateChanges.next();
  }
  private _placeholder: string;

  @Input()
  get required(): boolean {
    return this._required;
  }
  set required(value: boolean) {
    this._required = coerceBooleanProperty(value);
    this.stateChanges.next();
  }
  private _required = false;

  @Input()
  get disabled(): boolean {
    return this._disabled;
  }
  set disabled(value: boolean) {
    this._disabled = coerceBooleanProperty(value);
    this._disabled ? this.parts.disable() : this.parts.enable();
    this.stateChanges.next();
  }
  private _disabled = false;

  @Input()
  get value(): Adress | null {
    const {
      value: { street, houseNumber, apartamentNumber, city}
    } = this.parts;
    return new Adress(street, houseNumber, apartamentNumber, city);
  }
  set value(tel: Adress | null) {
    const { street, houseNumber, apartamentNumber, city } = tel || new Adress('', '','','');
    this.parts.setValue({ street, houseNumber, apartamentNumber, city });
    this.stateChanges.next();
  }

  get errorState(): boolean {
    return this.parts.invalid && this.parts.dirty;
  }

  constructor(
    formBuilder: FormBuilder,
    private _focusMonitor: FocusMonitor,
    private _elementRef: ElementRef<HTMLElement>,
    @Optional() @Inject(MAT_FORM_FIELD) public _formField: MatFormField,
    @Optional() @Self() public ngControl: NgControl) {

    this.parts = formBuilder.group({
      street: [
        null,
        [Validators.required]
      ],
      houseNumber: [
        null,
        [Validators.required]
      ],
      apartamentNumber: [
        null,
        [Validators.required]
      ],
      city: [
        null,
        [Validators.required]
      ]
    });

    _focusMonitor.monitor(_elementRef, true).subscribe(origin => {
      if (origin) {
        this.streetInput.nativeElement.style.backgroundColor = "lightgray";
        this.houseNumberInput.nativeElement.style.backgroundColor = "lightgray";
        this.apartamentNumberInput.nativeElement.style.backgroundColor = "lightgray";
        this.cityInput.nativeElement.style.backgroundColor = "lightgray";
      } else {
        this.streetInput.nativeElement.style.backgroundColor = "";
        this.houseNumberInput.nativeElement.style.backgroundColor = "";
        this.apartamentNumberInput.nativeElement.style.backgroundColor = "";
        this.cityInput.nativeElement.style.backgroundColor = "";
      }
      if (this.focused && !origin) {
        this.onTouched();
      }

      this.focused = !!origin;
      this.stateChanges.next();
    });

    if (this.ngControl != null) {
      this.ngControl.valueAccessor = this;
    }
  }

  ngOnDestroy() {
    this.stateChanges.complete();
    this._focusMonitor.stopMonitoring(this._elementRef);
  }

  onContainerClick() {

  }

  setDescribedByIds(ids: string[]) {
    const controlElement = this._elementRef.nativeElement
      .querySelector('.adressInputContainer')!;
    controlElement.setAttribute('aria-describedby', ids.join(' '));
  }

  writeValue(tel: Adress | null): void {
    this.value = tel;
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  FocusInput(nextElement?: HTMLInputElement): void {
    this._focusMonitor.focusVia(nextElement, 'program');
  }

  handleInput(control: AbstractControl): void {
    this.onChange(this.value);
  }
}
