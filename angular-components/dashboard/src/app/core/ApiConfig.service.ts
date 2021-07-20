
import { Injectable, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ConfigService, OperatorPageModel } from '@wizardcontroller/sac-appliance-lib';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { BehaviorSubject, ReplaySubject, Subject } from 'rxjs';
import { map } from 'rxjs/operators';
@Injectable({
  providedIn: 'root'
})

export class ApiConfigService {
  operatorPageModel!: OperatorPageModel;
  // operator page model change notification support
  private currentPageModelSource = new ReplaySubject<OperatorPageModel>();
  operatorPageModelChanges$ = this.currentPageModelSource.asObservable();
  configService!: ConfigService;


  constructor(cfgSvc: ConfigService, private router: Router) {
    console.log("ApiConfigService service starting");
    this.configService = cfgSvc;
    this.configService.configuration.basePath = window.location.origin;
    this.initService();
  }

  /*
    currently angular injectables do not participate in all lifecycle hooks
  */
  initService(): void {
  
  }

  /*
               call the dashboard and get the operator page model
               including base url for discovery appliance
               and user appliances
           */
  initPageModelSubject() {
    console.log("apiconfigsvc is getting operator page model");
    this.configService.configuration.basePath = window.location.origin;
    console.log('calling config service');
    this.configService.getOperatorPageModel()
      .pipe(
        map(data => {
          this.currentPageModelSource.next(data);
        })
      ).subscribe();


      /*
      .subscribe(
      (data: OperatorPageModel) => {
        return this.cacheOperatorPageModelSignalSubscribers(data);
      },
      (err: any) => console.log(err),
      () => {
        console.log(
          'done getting operator page model: applianceUrl is ' +
          this.operatorPageModel?.applianceUrl
        );
      }
    ); */
  }

  /**
   * @description signal the new pagemodel subject listeners
   * @param data
   */
  private cacheOperatorPageModelSignalSubscribers(data: OperatorPageModel): void {
    this.operatorPageModel = data;
    this.currentPageModelSource.next(data);
  }

}
