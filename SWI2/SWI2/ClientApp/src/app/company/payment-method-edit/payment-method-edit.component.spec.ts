import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { PaymentMethodEditComponent } from './payment-method-edit.component';

let component: PaymentMethodEditComponent;
let fixture: ComponentFixture<PaymentMethodEditComponent>;

describe('payment-method-edit component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ PaymentMethodEditComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(PaymentMethodEditComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});
