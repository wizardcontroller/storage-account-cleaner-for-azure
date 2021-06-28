import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiConfigService } from '../core/ApiConfig.service';
import { OperatorPageModel } from '@wizardcontroller/sac-appliance-lib';

@Injectable()
export class AuthHeaderInterceptorInterceptor implements HttpInterceptor {
  private pageModel!: OperatorPageModel;
  private applianceUrl!: string;

  constructor(private apiConfigSvc: ApiConfigService) {
    this.apiConfigSvc.operatorPageModelChanges$.subscribe((data) => {
      this.pageModel = data;
      this.applianceUrl = this.pageModel?.applianceUrl?.toString() as string;
    });
  }

  intercept(
    request: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    const requestClone = request.clone();

    if (requestClone.url.includes(this.applianceUrl)) {
      console.log("http interceptor is adding xumo auth header");
      requestClone.headers.append(
        'X-ZUMO-AUTH',
        this.pageModel?.easyAuthAccessToken?.toString() as string
      );
    }
    return next.handle(requestClone);
  }
}
