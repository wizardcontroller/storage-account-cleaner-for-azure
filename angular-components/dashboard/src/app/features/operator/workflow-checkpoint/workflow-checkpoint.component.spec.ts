import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WorkflowCheckpointComponent } from './workflow-checkpoint.component';

describe('WorkflowCheckpointComponent', () => {
  let component: WorkflowCheckpointComponent;
  let fixture: ComponentFixture<WorkflowCheckpointComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ WorkflowCheckpointComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(WorkflowCheckpointComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
