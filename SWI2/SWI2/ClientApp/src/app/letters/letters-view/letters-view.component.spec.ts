/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { LettersViewComponent } from './letters-view.component';

let component: LettersViewComponent;
let fixture: ComponentFixture<LettersViewComponent>;

describe('letters-view component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ LettersViewComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(LettersViewComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});