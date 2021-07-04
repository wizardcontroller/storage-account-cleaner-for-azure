import { Component, OnDestroy, OnInit } from '@angular/core';
import { ApplianceSessionContext, OperatorPageModel, RetentionEntitiesService } from '@wizardcontroller/sac-appliance-lib';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { ReplaySubject } from 'rxjs';
import { ApiConfigService } from 'src/app/core/ApiConfig.service';
import { ICanBeHiddenFromDisplay } from '../../interfaces/ICanBeHiddenFromDisplay';
import {MatButtonToggleModule} from '@angular/material/button-toggle';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';

@Component({

  templateUrl: './ApplianceContextView.component.html',
  styleUrls: ['./ApplianceContextView.component.css']
})

@AutoUnsubscribe()
export class ApplianceContextViewComponent implements OnInit, OnDestroy, ICanBeHiddenFromDisplay {
  private baseUri : string = '';

  applianceSessionContext!: ApplianceSessionContext;
  private currentApplianceContextSource = new ReplaySubject<ApplianceSessionContext>();
  applianceContextChanges$ = this.currentApplianceContextSource.asObservable();
  id: any;
  constructor(private apiConfigSvc : ApiConfigService, private retentionEntitiesSvc: RetentionEntitiesService,  private route: ActivatedRoute) {

   }


    operatorPageModel!: OperatorPageModel;
    // operator page model change notification support
    private currentPageModelSource = new ReplaySubject<OperatorPageModel>();
    operatorPageModelChanges$ = this.currentPageModelSource.asObservable();

  ngOnDestroy(): void {

  }

  ngOnInit() {

    this.ensureOperatorPageModel();

    this.route.queryParams.subscribe(params => {
      this.id = params['id'];
    });
  }

  isShow = false;

  ensureOperatorPageModel(){
    this.apiConfigSvc.operatorPageModelChanges$.subscribe(data => {
      console.log("app component has operator page model");
      this.baseUri = data.applianceUrl?.toString() as string;
      this.currentPageModelSource.next(data);

      // the component can show
      this.isShow = true;
      return this.operatorPageModel = data;
    });
  }

  toggleDisplay() {
    this.isShow = !this.isShow;
  }
}
