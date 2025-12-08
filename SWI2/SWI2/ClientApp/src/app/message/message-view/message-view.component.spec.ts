/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { MessageViewComponent } from './message-view.component';

let component: MessageViewComponent;
let fixture: ComponentFixture<MessageViewComponent>;

describe('message-view component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ MessageViewComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(MessageViewComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});