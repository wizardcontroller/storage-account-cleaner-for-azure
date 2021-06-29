import { TestBed } from '@angular/core/testing';

import { MockOperatorPageModelInterceptor } from './mock-operator-page-model.interceptor';

describe('MockOperatorPageModelInterceptor', () => {
  beforeEach(() => TestBed.configureTestingModule({
    providers: [
      MockOperatorPageModelInterceptor
      ]
  }));

  it('should be created', () => {
    const interceptor: MockOperatorPageModelInterceptor = TestBed.inject(MockOperatorPageModelInterceptor);
    expect(interceptor).toBeTruthy();
  });
});
