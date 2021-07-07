/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { DiagnosticsRetentionSurfaceViewComponent } from './DiagnosticsRetentionSurfaceView.component';
import { MatButtonToggle } from '@angular/material/button-toggle';
import { CardModule } from 'primeng/card';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ButtonModule } from 'primeng/button';

describe('DiagnosticsRetentionSurfaceViewComponent', () => {
  let component: DiagnosticsRetentionSurfaceViewComponent;
  let fixture: ComponentFixture<DiagnosticsRetentionSurfaceViewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [ ButtonModule, CardModule, HttpClientTestingModule, RouterTestingModule],
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
