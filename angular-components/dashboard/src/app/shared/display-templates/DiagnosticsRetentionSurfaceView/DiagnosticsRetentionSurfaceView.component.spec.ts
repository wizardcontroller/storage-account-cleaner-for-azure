/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { DiagnosticsRetentionSurfaceViewComponent } from './DiagnosticsRetentionSurfaceView.component';

describe('DiagnosticsRetentionSurfaceViewComponent', () => {
  let component: DiagnosticsRetentionSurfaceViewComponent;
  let fixture: ComponentFixture<DiagnosticsRetentionSurfaceViewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DiagnosticsRetentionSurfaceViewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DiagnosticsRetentionSurfaceViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
