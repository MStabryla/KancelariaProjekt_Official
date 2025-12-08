import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { CompanyEditComponent } from './company-edit.component';

let component: CompanyEditComponent;
let fixture: ComponentFixture<CompanyEditComponent>;

describe('company-edit component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ CompanyEditComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(CompanyEditComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});
