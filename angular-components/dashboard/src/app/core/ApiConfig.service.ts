
import { Injectable, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ConfigService, OperatorPageModel } from '@wizardcontroller/sac-appliance-lib';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { BehaviorSubject, of, ReplaySubject, Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})

@AutoUnsubscribe()
export class ApiConfigService implements OnDestroy{
  operatorPageModel!: OperatorPageModel;
  // operator page model change notification support
  private currentPageModelSource = new ReplaySubject<OperatorPageModel>();
  operatorPageModelChanges$ = this.currentPageModelSource.asObservable();


  constructor(private configService: ConfigService, private router: Router) {
    console.log("ApiConfigService service starting");

    configService.configuration.basePath = window.location.origin;
    this.initService();
  }
  ngOnDestroy(): void {

  }

  /*
    currently angular injectables do not participate in all lifecycle hooks
  */
  initService(): void {
    this.initPageModelSubject();
  }

  /*
               call the dashboard and get the operator page model
               including base url for discovery appliance
               and user appliances
           */
  private initPageModelSubject() {
    console.log("apiconfigsvc is getting operator page model");
    this.configService.configuration.basePath = window.location.origin;
    console.log('calling config service');
    this.configService.getOperatorPageModel().subscribe(
      (data: OperatorPageModel) => {
        this.operatorPageModel = data;
        this.currentPageModelSource.next(data);
        // return this.cacheOperatorPageModelSignalSubscribers(data);
      },
      (err: any) => console.log(err),
      () => {
        console.log(
          'done getting operator page model: applianceUrl is ' +
          this.operatorPageModel?.applianceUrl
        );
      }
    );
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
