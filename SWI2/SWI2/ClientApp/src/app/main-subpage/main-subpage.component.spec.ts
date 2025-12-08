import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MainSubpageComponent } from './main-subpage.component';

describe('MainSubpageComponent', () => {
  let component: MainSubpageComponent;
  let fixture: ComponentFixture<MainSubpageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MainSubpageComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(MainSubpageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
