/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { DocumentsComponent } from './documents.component';

let component: DocumentsComponent;
let fixture: ComponentFixture<DocumentsComponent>;

describe('documents component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ DocumentsComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(DocumentsComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});