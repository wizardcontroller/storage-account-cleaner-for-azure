/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { ApiConfigService } from './ApiConfig.service';

describe('Service: ApiConfig', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ApiConfigService]
    });
  });

  it('should ...', inject([ApiConfigService], (service: ApiConfigService) => {
    expect(service).toBeTruthy();
  }));
});
