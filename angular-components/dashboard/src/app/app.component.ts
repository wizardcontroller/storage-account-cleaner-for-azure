import { ApplianceApiService } from './shared/services/appliance-api.service';

import { ApiConfigService } from './core/ApiConfig.service';
import { Component, OnInit } from '@angular/core';
import { CoreComponent } from './core/core.component';
import { OperatorPageModel } from 'index';
import { BehaviorSubject, Subject } from 'rxjs';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})

@AutoUnsubscribe()
export class AppComponent implements OnInit {


  operatorPageModel!: OperatorPageModel | null;
    // operator page model change notification support
    private currentPageModelSource = new BehaviorSubject<OperatorPageModel | null>(null);
    operatorPageModelChanges$ = this.currentPageModelSource.asObservable();

  baseUri: String | undefined;
  title = 'dashboard';
  constructor(private apiConfigSvc : ApiConfigService, private applianceSvc : ApplianceApiService) {
    this.apiConfigSvc.operatorPageModelChanges$.subscribe(data => {
      console.log("app component has operator page model");
      this.baseUri = data?.applianceUrl?.toString();
      return this.operatorPageModel = data;
    });
  }

   ngOnInit(): void {
     console.log("app component is initializing page model");
     this.apiConfigSvc.getOperatorPageModel();

    // this.operatorPageModel = this.apiConfigSvc.operatorPageModel;
    // this.baseUri = this.operatorPageModel?.applianceUrl?.toString();

  }

}
