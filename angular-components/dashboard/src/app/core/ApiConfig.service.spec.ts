/* tslint:disable:no-unused-variable */

import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TestBed, async, inject } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ApiConfigService } from './ApiConfig.service';

describe('Service: ApiConfig', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [ HttpClientTestingModule, RouterTestingModule],
      providers: [ApiConfigService]
    });
  });

  it('should ...', inject([ApiConfigService], (service: ApiConfigService) => {
    expect(service).toBeTruthy();
  }));
});
