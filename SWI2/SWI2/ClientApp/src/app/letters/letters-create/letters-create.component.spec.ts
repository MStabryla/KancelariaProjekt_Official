/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { LettersCreateComponent } from './letters-create.component';

let component: LettersCreateComponent;
let fixture: ComponentFixture<LettersCreateComponent>;

describe('letters-create component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ LettersCreateComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(LettersCreateComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});