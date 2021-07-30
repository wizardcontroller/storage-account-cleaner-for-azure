/* eslint-disable @typescript-eslint/no-unused-vars */

import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TestBed, async, inject } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { MessageService } from 'primeng/api';
import { ApiConfigService } from './ApiConfig.service';

describe('Service: ApiConfig', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [ HttpClientTestingModule, RouterTestingModule````],
      providers: [ApiConfigService, MessageService]
    });
  });

  it('should ...', inject([ApiConfigService], (service: ApiConfigService) => {
    expect(service).toBeTruthy();
  }));
});
