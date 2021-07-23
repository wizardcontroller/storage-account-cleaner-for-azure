/* eslint-disable @typescript-eslint/no-unused-vars */

import { TestBed, async, inject } from '@angular/core/testing';
import { ConfigService, RetentionEntitiesService } from '@wizardcontroller/sac-appliance-lib/';
import { ApplianceApiService } from './appliance-api.service';
import {OperatorPageModel} from '@wizardcontroller/sac-appliance-lib'
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
describe('Service: ApplianceApi', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule, RouterTestingModule
        ],
      providers: [ ApplianceApiService ]
    });
  });

  it('should ...', inject([ApplianceApiService], (service: ApplianceApiService) => {
    expect(service).toBeTruthy();
  }));
});
