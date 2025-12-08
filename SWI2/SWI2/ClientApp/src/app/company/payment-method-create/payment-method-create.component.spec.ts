import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { PaymentMethodCreateComponent } from './payment-method-create.component';

let component: PaymentMethodCreateComponent;
let fixture: ComponentFixture<PaymentMethodCreateComponent>;

describe('payment-method-create component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ PaymentMethodCreateComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(PaymentMethodCreateComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});
