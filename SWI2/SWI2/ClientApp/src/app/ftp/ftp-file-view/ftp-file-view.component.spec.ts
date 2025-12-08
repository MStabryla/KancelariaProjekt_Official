/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { FtpFileViewComponent } from './ftp-file-view.component';

let component: FtpFileViewComponent;
let fixture: ComponentFixture<FtpFileViewComponent>;

describe('ftp-file-view component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ FtpFileViewComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(FtpFileViewComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});