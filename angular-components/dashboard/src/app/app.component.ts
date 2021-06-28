import { ApplianceApiService } from './shared/services/appliance-api.service';

import { ApiConfigService } from './core/ApiConfig.service';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { CoreComponent } from './core/core.component';
import { OperatorPageModel } from '@wizardcontroller/sac-appliance-lib';
import { BehaviorSubject, Operator, ReplaySubject, Subject } from 'rxjs';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { SubscriptionsViewComponent } from './shared/display-templates/SubscriptionsView/SubscriptionsView.component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})

@AutoUnsubscribe()
export class AppComponent implements OnInit, OnDestroy {


  operatorPageModel!: OperatorPageModel;
    // operator page model change notification support
    private currentPageModelSource = new ReplaySubject<OperatorPageModel>();
    operatorPageModelChanges$ = this.currentPageModelSource.asObservable();

  baseUri: String | undefined;
  title = 'dashboard';
  constructor(private apiConfigSvc : ApiConfigService, private applianceSvc : ApplianceApiService) {
    this.apiConfigSvc.operatorPageModelChanges$.subscribe(data => {
      console.log("app component has operator page model");
      this.baseUri = data.applianceUrl?.toString();
      this.currentPageModelSource.next(data);
      return this.operatorPageModel = data;
    });
  }

  toggleExpand(section : any){

  }

  ngOnDestroy(): void {
    throw new Error('Method not implemented.');
  }

   ngOnInit(): void {
     console.log("app component is initializing page model");
     this.apiConfigSvc.getOperatorPageModel();

    // this.operatorPageModel = this.apiConfigSvc.operatorPageModel;
    // this.baseUri = this.operatorPageModel?.applianceUrl?.toString();

  }

}
