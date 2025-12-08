/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { PaymentMethodsComponent } from './payment-methods.component';

let component: PaymentMethodsComponent;
let fixture: ComponentFixture<PaymentMethodsComponent>;

describe('payment-methods component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ PaymentMethodsComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(PaymentMethodsComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});