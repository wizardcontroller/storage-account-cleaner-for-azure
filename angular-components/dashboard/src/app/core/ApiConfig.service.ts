
import { Injectable, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ConfigService, OperatorPageModel } from '@wizardcontroller/sac-appliance-lib';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { BehaviorSubject, ReplaySubject, Subject } from 'rxjs';
import { map } from 'rxjs/operators';
import { MessageService } from 'primeng/api';
import { ToastMessage } from '../models/ToastMessage';
@Injectable({
  providedIn: 'root'
})

export class ApiConfigService {
  operatorPageModel!: OperatorPageModel;
  // operator page model change notification support
  private currentPageModelSource = new ReplaySubject<OperatorPageModel>();
  operatorPageModelChanges$ = this.currentPageModelSource.asObservable();
  configService!: ConfigService;


  constructor(cfgSvc: ConfigService, private router: Router, private messageService: MessageService) {
    console.log("ApiConfigService service starting");
    this.configService = cfgSvc;
    this.configService.configuration.basePath = window.location.origin;
    this.initService();
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
  initPageModelSubject() {
    console.log("apiconfigsvc is getting operator page model");
    this.configService.configuration.basePath = window.location.origin;
    console.log('calling config service');
    this.configService.getOperatorPageModel()
      .pipe(
        map(data => {
          this.currentPageModelSource.next(data);
          const toast = new ToastMessage();
          toast.detail = "loaded operator page model";
          toast.detail = "page model refreshed";
          toast.severity = "info";
          this.showToast(toast);
        })
      ).subscribe();
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
