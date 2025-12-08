/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { UserEmailChangeComponent } from './user-email-change.component';

let component: UserEmailChangeComponent;
let fixture: ComponentFixture<UserEmailChangeComponent>;

describe('user-email-change component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ UserEmailChangeComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(UserEmailChangeComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});