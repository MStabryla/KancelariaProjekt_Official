/// <reference path="../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { UserpanelComponent } from './userpanel.component';

let component: UserpanelComponent;
let fixture: ComponentFixture<UserpanelComponent>;

describe('userpanel component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ UserpanelComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(UserpanelComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});