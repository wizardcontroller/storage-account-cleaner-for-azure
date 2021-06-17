import { TestBed } from '@angular/core/testing';

import { TableStorageRetentionPolicyServiceService } from './table-storage-retention-policy-service.service';

describe('TableStorageRetentionPolicyServiceService', () => {
  let service: TableStorageRetentionPolicyServiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TableStorageRetentionPolicyServiceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
