import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ApplianceJobLogViewComponent } from './appliance-job-log-view.component';

describe('ApplianceJobLogViewComponent', () => {
  let component: ApplianceJobLogViewComponent;
  let fixture: ComponentFixture<ApplianceJobLogViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ApplianceJobLogViewComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ApplianceJobLogViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
