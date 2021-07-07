/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { StorageAccountViewComponent } from './StorageAccountView.component';
import { MatButtonToggle } from '@angular/material/button-toggle';
import { CardModule } from 'primeng/card';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ButtonModule } from 'primeng/button';

describe('StorageAccountViewComponent', () => {
  let component: StorageAccountViewComponent;
  let fixture: ComponentFixture<StorageAccountViewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [ ButtonModule, CardModule, HttpClientTestingModule, RouterTestingModule],
      declarations: [ StorageAccountViewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StorageAccountViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
