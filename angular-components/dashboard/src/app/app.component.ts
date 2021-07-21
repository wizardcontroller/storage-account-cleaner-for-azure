import { ApplianceApiService } from './shared/services/appliance-api.service';

import { ApiConfigService } from './core/ApiConfig.service';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { CoreComponent } from './core/core.component';
import { OperatorPageModel } from '@wizardcontroller/sac-appliance-lib';
import { BehaviorSubject, Operator, ReplaySubject, Subject } from 'rxjs';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { SubscriptionsViewComponent } from './shared/display-templates/SubscriptionsView/SubscriptionsView.component';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { ICanBeHiddenFromDisplay } from './shared/interfaces/ICanBeHiddenFromDisplay';
import { map, tap } from 'rxjs/operators';
import { ThemePalette } from '@angular/material/core'
import { MatSlideToggleModule } from '@angular/material/slide-toggle'
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})

@AutoUnsubscribe()
export class AppComponent implements OnInit, OnDestroy, ICanBeHiddenFromDisplay {
  isAutoRefreshWorkflowCheckpoint = this.applianceSvc.isAutoRefreshWorkflowCheckpoint;
  color: ThemePalette = 'accent';

  isRefreshing!: boolean;

  isRefreshingPipe = this.applianceSvc.isRefreshingChanges$
    .pipe(
      map(isRefreshing => {
        isRefreshing = isRefreshing;
      })
  ).subscribe();

  operatorPageModel!: OperatorPageModel;
  // operator page model change notification support
  private currentPageModelSource = new ReplaySubject<OperatorPageModel>();
  operatorPageModelChanges$ = this.currentPageModelSource.asObservable();

  baseUri: String | undefined;
  title = 'dashboard';
  constructor(private apiConfigSvc: ApiConfigService, private applianceSvc: ApplianceApiService) {

    this.isShow = false;
    this.apiConfigSvc.operatorPageModelChanges$.subscribe(data => {
      console.log("app component has operator page model");
      this.baseUri = data.applianceUrl?.toString();
      this.currentPageModelSource.next(data);
      return this.operatorPageModel = data;
    });
  }
  isShow!: boolean;
  toggleDisplay(): void {
    console.log("toggling");
    this.isShow = !this.isShow;
  }



  ngOnDestroy(): void {

    // do nothing.

  }

  ngOnInit(): void {
    console.log("app component is initializing page model");
    this.apiConfigSvc.initPageModelSubject();


  }

}
