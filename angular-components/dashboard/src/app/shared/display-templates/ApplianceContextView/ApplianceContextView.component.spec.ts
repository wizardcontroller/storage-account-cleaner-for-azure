/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { ApplianceContextViewComponent } from './ApplianceContextView.component';
import { MatButtonToggle } from '@angular/material/button-toggle';
import { RouterTestingModule } from '@angular/router/testing';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatToolbarModule } from '@angular/material/toolbar';

describe('ApplianceContextViewComponent', () => {
  let component: ApplianceContextViewComponent;
  let fixture: ComponentFixture<ApplianceContextViewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [ ButtonModule, CardModule, HttpClientTestingModule, RouterTestingModule, MatMenuModule, MatToolbarModule],
      declarations: [ ApplianceContextViewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ApplianceContextViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
