/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { LetterRecipientCreateComponent } from './letter-recipient-create.component';

let component: LetterRecipientCreateComponent;
let fixture: ComponentFixture<LetterRecipientCreateComponent>;

describe('letter-recipient-create component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ LetterRecipientCreateComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(LetterRecipientCreateComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});