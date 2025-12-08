/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { DocumentViewComponent } from './document-view.component';

let component: DocumentViewComponent;
let fixture: ComponentFixture<DocumentViewComponent>;

describe('document-view component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ DocumentViewComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(DocumentViewComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});