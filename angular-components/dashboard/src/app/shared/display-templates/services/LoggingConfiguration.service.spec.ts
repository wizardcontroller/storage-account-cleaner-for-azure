/* eslint-disable @typescript-eslint/no-unused-vars */

import { TestBed, async, inject } from '@angular/core/testing';
import { LoggingConfigurationService } from './LoggingConfiguration.service';

describe('Service: LoggingConfiguration', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [LoggingConfigurationService]
    });
  });

  it('should ...', inject([LoggingConfigurationService], (service: LoggingConfigurationService) => {
    expect(service).toBeTruthy();
  }));
});
