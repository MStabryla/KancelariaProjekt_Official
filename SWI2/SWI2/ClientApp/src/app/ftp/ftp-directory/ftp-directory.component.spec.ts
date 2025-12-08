/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { FtpDirectoryComponent } from './ftp-directory.component';

let component: FtpDirectoryComponent;
let fixture: ComponentFixture<FtpDirectoryComponent>;

describe('ftp-directory component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ FtpDirectoryComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(FtpDirectoryComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});