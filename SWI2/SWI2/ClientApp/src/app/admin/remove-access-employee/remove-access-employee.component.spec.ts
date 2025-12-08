/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { RemoveAccessEmployeeComponent } from './remove-access-employee.component';

let component: RemoveAccessEmployeeComponent;
let fixture: ComponentFixture<RemoveAccessEmployeeComponent>;

describe('remove-access-employee component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ RemoveAccessEmployeeComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(RemoveAccessEmployeeComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});