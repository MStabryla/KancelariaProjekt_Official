/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { MessagesCreateComponent } from './messages-create.component';

let component: MessagesCreateComponent;
let fixture: ComponentFixture<MessagesCreateComponent>;

describe('messages-create component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ MessagesCreateComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(MessagesCreateComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});