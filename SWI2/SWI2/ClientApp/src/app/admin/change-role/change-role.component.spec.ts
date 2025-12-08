/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { ChangeRoleComponent } from './change-role.component';

let component: ChangeRoleComponent;
let fixture: ComponentFixture<ChangeRoleComponent>;

describe('change-role component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ ChangeRoleComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(ChangeRoleComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});