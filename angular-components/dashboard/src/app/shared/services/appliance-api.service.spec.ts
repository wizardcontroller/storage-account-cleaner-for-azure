/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { ApplianceApiService } from './appliance-api.service';

describe('Service: ApplianceApi', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ApplianceApiService]
    });
  });

  it('should ...', inject([ApplianceApiService], (service: ApplianceApiService) => {
    expect(service).toBeTruthy();
  }));
});
