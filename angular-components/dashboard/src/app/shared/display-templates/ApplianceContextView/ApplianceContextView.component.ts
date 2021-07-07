import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ApplianceSessionContext, OperatorPageModel, RetentionEntitiesService } from '@wizardcontroller/sac-appliance-lib';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { ReplaySubject } from 'rxjs';
import { ApiConfigService } from 'src/app/core/ApiConfig.service';
import { ICanBeHiddenFromDisplay } from '../../interfaces/ICanBeHiddenFromDisplay';
import {MatButtonToggleModule} from '@angular/material/button-toggle';
import { Router, ActivatedRoute, ParamMap, Routes } from '@angular/router';
import {MatExpansionModule} from '@angular/material/expansion';
import { MatExpansionPanel } from '@angular/material/expansion';
import { MatAccordion } from '@angular/material/expansion';
import { MatMenuModule} from '@angular/material/menu';
import {MatToolbarModule} from '@angular/material/toolbar';
import { SubscriptionsViewComponent } from '../SubscriptionsView/SubscriptionsView.component';
import { StorageAccountViewComponent } from '../StorageAccountView/StorageAccountView.component';
@Component({

  templateUrl: './ApplianceContextView.component.html',
  styleUrls: ['./ApplianceContextView.component.css']
})


@AutoUnsubscribe()
export class ApplianceContextViewComponent implements OnInit, OnDestroy, ICanBeHiddenFromDisplay {
  @ViewChild(MatAccordion) accordion! : MatAccordion;

  private baseUri : string = '';

  applianceSessionContext!: ApplianceSessionContext;
  private currentApplianceContextSource = new ReplaySubject<ApplianceSessionContext>();
  applianceContextChanges$ = this.currentApplianceContextSource.asObservable();
  id: any;
  constructor(private apiConfigSvc : ApiConfigService,
              private retentionEntitiesSvc: RetentionEntitiesService,
              private router : Router,
              private route: ActivatedRoute) {

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

  ensureApplianceContext(tenantid: string, oid: string){
    this.retentionEntitiesSvc.getApplianceSessionContext(tenantid, oid).subscribe(data => {
      this.currentApplianceContextSource.next(data);
    });
  }

  ensureOperatorPageModel(){
    this.apiConfigSvc.operatorPageModelChanges$.subscribe(data => {
      console.log("app component has operator page model");
      this.baseUri = data.applianceUrl?.toString() as string;
      this.currentPageModelSource.next(data);
      // the component can show
      this.isShow = true;
      this.ensureApplianceContext(data.tenantid as string, data.oid as string);
      return this.operatorPageModel = data;
    });
  }

  toggleDisplay() {
    this.isShow = !this.isShow;
  }
}
