import { TestBed } from '@angular/core/testing';

import { PageModelService } from './page-model.service';

describe('PageModelService', () => {
  let service: PageModelService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PageModelService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
