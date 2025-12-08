/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { UserChangePasswordComponent } from './user-change-password.component';

let component: UserChangePasswordComponent;
let fixture: ComponentFixture<UserChangePasswordComponent>;

describe('user-change-password component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ UserChangePasswordComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(UserChangePasswordComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});