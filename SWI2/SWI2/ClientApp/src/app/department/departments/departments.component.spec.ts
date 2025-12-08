/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { DepartmentsComponent } from './departments.component';

let component: DepartmentsComponent;
let fixture: ComponentFixture<DepartmentsComponent>;

describe('departments component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ DepartmentsComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(DepartmentsComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});