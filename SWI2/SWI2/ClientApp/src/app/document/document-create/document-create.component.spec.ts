/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { DocumentCreateComponent } from './document-create.component';

let component: DocumentCreateComponent;
let fixture: ComponentFixture<DocumentCreateComponent>;

describe('document-create component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ DocumentCreateComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(DocumentCreateComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});