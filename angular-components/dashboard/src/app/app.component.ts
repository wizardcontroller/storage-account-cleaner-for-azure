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
import { MessageService } from 'primeng/api';
import { ToastMessage } from './models/ToastMessage';
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

  operatorPageModel!: OperatorPageModel;
  // operator page model change notification support
  private currentPageModelSource = new ReplaySubject<OperatorPageModel>();
  operatorPageModelChanges$ = this.currentPageModelSource.asObservable();

  baseUri: String | undefined;
  title = 'dashboard';
  constructor(private apiConfigSvc: ApiConfigService,
    private messageService: MessageService,
    private applianceSvc: ApplianceApiService) {

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

    // do no thing.

  }

  ngOnInit(): void {
    console.log("app component is initializing page model");
    this.apiConfigSvc.initPageModelSubject()
      .pipe(
        tap(t => {
          const toast = new ToastMessage();
          toast.detail = "loaded operator page model";
          toast.summary = "page model refreshed";
          toast.severity = "info";
          toast.sticky = false;
          this.showToast(toast);
        })
      ).subscribe();


  }

  showToast(message: ToastMessage): void {
    this.messageService.add(message);
  }

}
