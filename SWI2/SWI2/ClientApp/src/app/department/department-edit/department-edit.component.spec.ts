import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { DepartmentEditComponent } from './department-edit.component';

let component: DepartmentEditComponent;
let fixture: ComponentFixture<DepartmentEditComponent>;

describe('department-edit component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ DepartmentEditComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(DepartmentEditComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});
