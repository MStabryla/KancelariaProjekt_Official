import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { PaymentMethodViewComponent } from './payment-method-view.component';

let component: PaymentMethodViewComponent;
let fixture: ComponentFixture<PaymentMethodViewComponent>;

describe('payment-method-view component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ PaymentMethodViewComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(PaymentMethodViewComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});
