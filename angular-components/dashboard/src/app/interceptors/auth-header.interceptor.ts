import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiConfigService } from '../core/ApiConfig.service';
import { OperatorPageModel } from '@wizardcontroller/sac-appliance-lib/';

@Injectable()
export class AuthHeaderInterceptor implements HttpInterceptor {
  private pageModel!: OperatorPageModel;
  private applianceUrl!: string;
  private HEADER_IMPERSONATION_TOKEN = "x-table-retention-mgmt-impersonation";
  private HEADER_X_ZUMO_AUTH = "X-ZUMO-AUTH";
  private HEADER_CURRENTSUBSCRIPTION = "x-table-retention-current-subscription";
  private HEADER_CURRENT_STORAGE_ACCOUNT = "x-table-retention-current-storage-account";

  constructor(private apiConfigSvc: ApiConfigService) {
    this.apiConfigSvc.operatorPageModelChanges$.subscribe((data) => {
      this.pageModel = data;
      this.applianceUrl = this.pageModel?.applianceUrl?.toString() as string;
    });
  }

  intercept(
    request: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    const requestClone = request.clone();

    if (requestClone.url.includes(this.applianceUrl)) {

      console.log("http interceptor is adding xumo auth header");
      requestClone.headers.append(
        this.HEADER_X_ZUMO_AUTH,
        this.pageModel?.easyAuthAccessToken?.toString() as string
      );

      console.log("http interceptor is adding impersonation token header");
      requestClone.headers.append(
        this.HEADER_IMPERSONATION_TOKEN,
        this.pageModel?.impersonationToken?.toString() as string
      );
    }
    return next.handle(requestClone);
  }
}
