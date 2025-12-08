import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { BrowserModule, By } from "@angular/platform-browser";
import { DepartmentCreateComponent } from './department-create.component';

let component: DepartmentCreateComponent;
let fixture: ComponentFixture<DepartmentCreateComponent>;

describe('department-create component', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ DepartmentCreateComponent ],
            imports: [ BrowserModule ],
            providers: [
                { provide: ComponentFixtureAutoDetect, useValue: true }
            ]
        });
        fixture = TestBed.createComponent(DepartmentCreateComponent);
        component = fixture.componentInstance;
    }));

    it('should do something', async(() => {
        expect(true).toEqual(true);
    }));
});
