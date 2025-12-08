import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { InvoiceEditComponent } from './invoiceEdit.component';

let component: InvoiceEditComponent;
let fixture: ComponentFixture<InvoiceEditComponent>;

describe('invoice component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
          declarations: [InvoiceEditComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
      fixture = TestBed.createComponent(InvoiceEditComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});
