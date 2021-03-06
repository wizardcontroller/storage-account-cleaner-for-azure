
import { Injectable, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ConfigService, OperatorPageModel } from '@wizardcontroller/sac-appliance-lib';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { BehaviorSubject, Observable, ReplaySubject, Subject } from 'rxjs';
import { distinctUntilChanged, map, share } from 'rxjs/operators';
import { MessageService } from 'primeng/api';
import { ToastMessage } from '../models/ToastMessage';
@Injectable({
  providedIn: 'root'
})
@AutoUnsubscribe()
export class ApiConfigService implements OnDestroy{
  operatorPageModel!: OperatorPageModel;
  // operator page model change notification support
  private currentPageModelSource = new ReplaySubject<OperatorPageModel>(1);
  operatorPageModelChanges$ = this.currentPageModelSource.asObservable();
  configService!: ConfigService;


  constructor(cfgSvc: ConfigService, private router: Router, private messageService: MessageService) {
    console.log("ApiConfigService service starting");
    this.configService = cfgSvc;
    this.configService.configuration.basePath = window.location.origin;
    this.initService();
  }

  ngOnDestroy(): void {

  }

  showToast(message: ToastMessage): void{
    this.messageService.add(message);
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
  initPageModelSubject(): Observable<OperatorPageModel>{
    console.log("apiconfigsvc is getting operator page model");
    this.configService.configuration.basePath = window.location.origin;
    console.log('calling config service');
    return this.configService.getOperatorPageModel()
      .pipe(
        distinctUntilChanged(),
        map(data => {
          this.currentPageModelSource.next(data);
          return data;
        }),     share()
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
