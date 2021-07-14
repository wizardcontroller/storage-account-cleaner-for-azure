/* eslint-disable @typescript-eslint/no-unused-vars */

import { TestBed, async, inject } from '@angular/core/testing';
import { ApplianceContextService } from './ApplianceContext.service';
import { HomeGrownLoggingService } from './HomeGrownLogging.service';

describe('Service: HomeGrownLogging', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [HomeGrownLoggingService]
    });
  });

  it('should ...', inject([HomeGrownLoggingService], (service: HomeGrownLoggingService<ApplianceContextService>) => {
    expect(service).toBeTruthy();
  }));
});
