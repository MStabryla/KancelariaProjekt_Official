/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { FtpFileComponent } from './ftp-file.component';

let component: FtpFileComponent;
let fixture: ComponentFixture<FtpFileComponent>;

describe('ftp-file component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ FtpFileComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(FtpFileComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});