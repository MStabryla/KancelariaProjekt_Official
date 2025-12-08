/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { CompanyCreateComponent } from './company-create.component';

let component: CompanyCreateComponent;
let fixture: ComponentFixture<CompanyCreateComponent>;

describe('company-create component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ CompanyCreateComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(CompanyCreateComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});