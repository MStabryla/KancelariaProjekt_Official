import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InvoiceMailTemplateComponent } from './invoice-mail-template.component';

describe('InvoiceMailTemplateComponent', () => {
  let component: InvoiceMailTemplateComponent;
  let fixture: ComponentFixture<InvoiceMailTemplateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ InvoiceMailTemplateComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(InvoiceMailTemplateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
