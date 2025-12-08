/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { GiveAccessClientComponent } from './give-access-client.component';

let component: GiveAccessClientComponent;
let fixture: ComponentFixture<GiveAccessClientComponent>;

describe('give-access-client component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ GiveAccessClientComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(GiveAccessClientComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});