import { TestBed } from '@angular/core/testing';

import { AuthHeaderInterceptorInterceptor } from './auth-header-interceptor.interceptor';

describe('AuthHeaderInterceptorInterceptor', () => {
  beforeEach(() => TestBed.configureTestingModule({
    providers: [
      AuthHeaderInterceptorInterceptor
      ]
  }));

  it('should be created', () => {
    const interceptor: AuthHeaderInterceptorInterceptor = TestBed.inject(AuthHeaderInterceptorInterceptor);
    expect(interceptor).toBeTruthy();
  });
});
