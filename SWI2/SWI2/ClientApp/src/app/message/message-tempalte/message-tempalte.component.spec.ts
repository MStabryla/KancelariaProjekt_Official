import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MessageTempalteComponent } from './message-tempalte.component';

describe('MessageTempalteComponent', () => {
  let component: MessageTempalteComponent;
  let fixture: ComponentFixture<MessageTempalteComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MessageTempalteComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(MessageTempalteComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
