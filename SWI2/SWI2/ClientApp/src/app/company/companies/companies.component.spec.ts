import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { CompaniesComponent } from './companies.component';

let component: CompaniesComponent;
let fixture: ComponentFixture<CompaniesComponent>;

describe('companies component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ CompaniesComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(CompaniesComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});
