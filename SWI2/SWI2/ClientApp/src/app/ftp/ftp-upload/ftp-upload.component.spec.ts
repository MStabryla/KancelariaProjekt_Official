/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { FtpUploadComponent } from './ftp-upload.component';

let component: FtpUploadComponent;
let fixture: ComponentFixture<FtpUploadComponent>;

describe('ftp-upload component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ FtpUploadComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(FtpUploadComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});