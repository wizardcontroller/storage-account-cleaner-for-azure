/* eslint-disable @typescript-eslint/no-unused-vars */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { StorageAccountViewComponent } from './StorageAccountView.component';
import { MatButtonToggle } from '@angular/material/button-toggle';
import { CardModule } from 'primeng/card';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ButtonModule } from 'primeng/button';
import { MessageService } from 'primeng/api';

describe('StorageAccountViewComponent', () => {
  let component: StorageAccountViewComponent;
  let fixture: ComponentFixture<StorageAccountViewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [ ButtonModule, CardModule, HttpClientTestingModule, RouterTestingModule],
      declarations: [ StorageAccountViewComponent ],
      providers: [MessageService]
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
