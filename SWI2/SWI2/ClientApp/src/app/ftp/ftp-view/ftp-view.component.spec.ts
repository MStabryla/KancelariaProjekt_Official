/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { FtpViewComponent } from './ftp-view.component';

let component: FtpViewComponent;
let fixture: ComponentFixture<FtpViewComponent>;

describe('ftp-view component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ FtpViewComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(FtpViewComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});