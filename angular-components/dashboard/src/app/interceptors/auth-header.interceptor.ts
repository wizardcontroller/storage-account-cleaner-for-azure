import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpHeaders,
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiConfigService } from '../core/ApiConfig.service';
import { OperatorPageModel } from '@wizardcontroller/sac-appliance-lib/';
import { ApplianceApiService } from '../shared/services/appliance-api.service';
import { RouterTestingModule } from '@angular/router/testing';
import { CoreModule } from '../core/core.module';
import { map } from 'rxjs/operators';
@Injectable()
export class AuthHeaderInterceptor implements HttpInterceptor {
  private pageModel!: OperatorPageModel;
  private applianceUrl!: string;
  private HEADER_IMPERSONATION_TOKEN = 'x-table-retention-mgmt-impersonation';
  private HEADER_X_ZUMO_AUTH = 'X-ZUMO-AUTH';
  private HEADER_CURRENTSUBSCRIPTION = 'x-table-retention-current-subscription';
  private HEADER_CURRENT_STORAGE_ACCOUNT =
    'x-table-retention-current-storage-account';
  private interceptorIsReady: boolean = false;
  private selectedStorageAccount!: string;

  constructor(private apiConfigSvc: ApiConfigService, private applianceSvc: ApplianceApiService) {


    this.apiConfigSvc.operatorPageModelChanges$.subscribe((data) => {
      console.log("auth interceptor has easyauth token: " + data.easyAuthAccessToken);
      console.log("auth interceptor has impersonation token: " + data.impersonationToken);
      this.pageModel = data;
      this.applianceUrl = this.pageModel?.applianceUrl?.toString() as string;
      this.interceptorIsReady = true;
    });
  }

  intercept(
    request: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {

    var headers = new HttpHeaders();
    /**
     * this interceptor depends on the current pagemodel context
     */
    if (this.interceptorIsReady) {

      const requestClone = request.clone({
        headers: request.
          headers.set(this.HEADER_X_ZUMO_AUTH,
            this.pageModel?.easyAuthAccessToken?.toString() as string).
          set(this.HEADER_IMPERSONATION_TOKEN,
            this.pageModel?.impersonationToken?.toString() as string)
      });

      return next.handle(requestClone);
    }

    return next.handle(request.clone());
  }
}
