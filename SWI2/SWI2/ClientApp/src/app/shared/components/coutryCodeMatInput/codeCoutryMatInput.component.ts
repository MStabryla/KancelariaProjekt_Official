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
export class CodeCoutry {
  constructor(
    public postalcode: string='',
    public postoffice: string='',
    public country: string=''
  ) { }
} 

/** Custom `MatFormFieldControl` for telephone number input. */
@Component({
  selector: 'codeCoutry-input',
  templateUrl: 'codeCoutryMatInput.component.html',
  styleUrls: ['codeCoutryMatInput.component.css'],
  providers: [{ provide: MatFormFieldControl, useExisting: coutryCodeMatInput }]
})
export class coutryCodeMatInput
  implements ControlValueAccessor, MatFormFieldControl<CodeCoutry>, OnDestroy {
  @ViewChild('postalcode') postalcodeInput: ElementRef;
  @ViewChild('postoffice') postofficeInput: ElementRef;
  @ViewChild('country') countryInput: ElementRef;
  static nextId = 0;

  
  parts: FormGroup;
  stateChanges = new Subject<void>();
  focused = false;
  onChange = (_: any) => { };
  onTouched = () => { };
  id = `adress-input-${coutryCodeMatInput.nextId++}`;

  get empty() {
    const {
      value: { postalcode, postoffice, country}
    } = this.parts;

    return !postalcode && !postoffice && !country;
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
  get value(): CodeCoutry | null {
      const {
        value: { postalcode, postoffice, country}
      } = this.parts;
    return new CodeCoutry(postalcode, postoffice, country);
  }
  set value(tel: CodeCoutry | null) {
    const { postalcode, postoffice, country} = tel || new CodeCoutry('', '', '');
    this.parts.setValue({ postalcode, postoffice, country});
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
      postalcode: [
        null,
        [Validators.required]
      ],
      postoffice: [
        null,
        [Validators.required]
      ],
      country: [
        null,
        [Validators.required]
      ]
    });

    _focusMonitor.monitor(_elementRef, true).subscribe(origin => {
      if (origin) {
        this.postalcodeInput.nativeElement.style.backgroundColor = "lightgray";
        this.postofficeInput.nativeElement.style.backgroundColor = "lightgray";
        this.countryInput.nativeElement.style.backgroundColor = "lightgray";
      } else {
        this.postalcodeInput.nativeElement.style.backgroundColor = "";
        this.postofficeInput.nativeElement.style.backgroundColor = "";
        this.countryInput.nativeElement.style.backgroundColor = "";
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
      .querySelector('.codeCountryInputContainer')!;
    controlElement.setAttribute('aria-describedby', ids.join(' '));
  }

  writeValue(tel: CodeCoutry | null): void {
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
