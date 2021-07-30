/* eslint-disable @typescript-eslint/no-unused-vars */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';
import { TimelineModule, Timeline } from 'primeng/timeline';
import { MetricRetentionSurfaceViewComponent } from './MetricRetentionSurfaceView.component';
import { MatButtonToggle } from '@angular/material/button-toggle';
import { CardModule, Card } from 'primeng/card';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ButtonModule } from 'primeng/button';
import { MessageService } from 'primeng/api';
import { ApiConfigService } from 'src/app/core/ApiConfig.service';
import { ApplianceApiService } from '../../services/appliance-api.service';

describe('MetricRetentionSurfaceViewComponent', () => {
  let component: MetricRetentionSurfaceViewComponent;
  let fixture: ComponentFixture<MetricRetentionSurfaceViewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [ ButtonModule, CardModule, HttpClientTestingModule, RouterTestingModule],
      declarations: [ MetricRetentionSurfaceViewComponent ],
      providers: [MessageService, ApiConfigService, ApplianceApiService]
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
