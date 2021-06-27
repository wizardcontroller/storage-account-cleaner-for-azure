/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { ApplianceContextService } from './ApplianceContext.service';
import {HttpClientModule} from '@angular/common/http'
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';

describe('Service: ApplianceContext', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule, RouterTestingModule],
      providers: [ApplianceContextService]
    });
  });

  it('should ...', inject([ApplianceContextService], (service: ApplianceContextService) => {
    expect(service).toBeTruthy();
  }));
});
