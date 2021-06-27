/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { MetricRetentionSurfaceViewComponent } from './MetricRetentionSurfaceView.component';

describe('MetricRetentionSurfaceViewComponent', () => {
  let component: MetricRetentionSurfaceViewComponent;
  let fixture: ComponentFixture<MetricRetentionSurfaceViewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MetricRetentionSurfaceViewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MetricRetentionSurfaceViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
