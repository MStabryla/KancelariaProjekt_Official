/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { GiveAccessEmployeeComponent } from './give-access-employee.component';

let component: GiveAccessEmployeeComponent;
let fixture: ComponentFixture<GiveAccessEmployeeComponent>;

describe('give-access-employee component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ GiveAccessEmployeeComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(GiveAccessEmployeeComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});