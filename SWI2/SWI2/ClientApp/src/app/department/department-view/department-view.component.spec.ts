import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { DepartmentViewComponent } from './department-view.component';

let component: DepartmentViewComponent;
let fixture: ComponentFixture<DepartmentViewComponent>;

describe('department-view component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ DepartmentViewComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(DepartmentViewComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});
