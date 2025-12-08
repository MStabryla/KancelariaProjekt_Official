/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { FtpAdminComponent } from './ftp-admin.component';

let component: FtpAdminComponent;
let fixture: ComponentFixture<FtpAdminComponent>;

describe('ftp-admin component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ FtpAdminComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(FtpAdminComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});