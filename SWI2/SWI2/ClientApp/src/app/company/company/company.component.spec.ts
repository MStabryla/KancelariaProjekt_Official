import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { CompanyComponent } from './company.component';

let component: CompanyComponent;
let fixture: ComponentFixture<CompanyComponent>;

describe('company component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ CompanyComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(CompanyComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});
