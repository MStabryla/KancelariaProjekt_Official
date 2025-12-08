/// <reference path="../../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { RemoveAccessClientComponent } from './remove-access-client.component';

let component: RemoveAccessClientComponent;
let fixture: ComponentFixture<RemoveAccessClientComponent>;

describe('remove-access-client component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ RemoveAccessClientComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(RemoveAccessClientComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});