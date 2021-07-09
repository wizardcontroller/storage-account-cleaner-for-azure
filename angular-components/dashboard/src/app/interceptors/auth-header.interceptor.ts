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
import { ApplianceApiService } from '../shared/services/appliance-api.service';
import { RouterTestingModule } from '@angular/router/testing';
import {CoreModule} from '../core/core.module'
@Injectable()
export class AuthHeaderInterceptor implements HttpInterceptor {
  private pageModel!: OperatorPageModel;
  private applianceUrl!: string;
  private HEADER_IMPERSONATION_TOKEN = "x-table-retention-mgmt-impersonation";
  private HEADER_X_ZUMO_AUTH = "X-ZUMO-AUTH";
  private HEADER_CURRENTSUBSCRIPTION = "x-table-retention-current-subscription";
  private HEADER_CURRENT_STORAGE_ACCOUNT = "x-table-retention-current-storage-account";
  private interceptorIsReady : boolean = false;

  constructor(private apiConfigSvc: ApiConfigService) {
    this.apiConfigSvc.operatorPageModelChanges$.subscribe((data) => {
      this.pageModel = data;
      this.applianceUrl = this.pageModel?.applianceUrl?.toString() as string;
      this.interceptorIsReady = true;
    });


  }

  intercept(
    request: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    const requestClone = request.clone();

    /**
     * this interceptor depends on the current pagemodel context
     */
    if(this.interceptorIsReady)
    {

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

    }}
    return next.handle(requestClone);
  }
}
