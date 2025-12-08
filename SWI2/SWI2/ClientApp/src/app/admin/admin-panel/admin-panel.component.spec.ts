/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { AdminPanelComponent } from './admin-panel.component';

let component: AdminPanelComponent;
let fixture: ComponentFixture<AdminPanelComponent>;

describe('adminPanel component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ AdminPanelComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(AdminPanelComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});