import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import {CoreModule} from '../core/core.module'
import { AuthHeaderInterceptor } from './auth-header.interceptor';
import { ICanBeHiddenFromDisplay } from '../shared/interfaces/ICanBeHiddenFromDisplay';
import { MessageService } from 'primeng/api';
describe('AuthHeaderInterceptor', () => {
  beforeEach(() => TestBed.configureTestingModule({
    imports: [HttpClientTestingModule, RouterTestingModule],

    providers: [
      AuthHeaderInterceptor, MessageService
      ]
  }));

  it('should be created', () => {
    const interceptor: AuthHeaderInterceptor = TestBed.inject(AuthHeaderInterceptor);
    expect(interceptor).toBeTruthy();
  });
});
