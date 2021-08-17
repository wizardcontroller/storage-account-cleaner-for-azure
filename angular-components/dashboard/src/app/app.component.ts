import { ApplianceApiService } from './shared/services/appliance-api.service';

import { ApiConfigService } from './core/ApiConfig.service';
import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { CoreComponent } from './core/core.component';
import { OperatorPageModel } from '@wizardcontroller/sac-appliance-lib';
import { BehaviorSubject, Operator, ReplaySubject, Subject } from 'rxjs';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { SubscriptionsViewComponent } from './shared/display-templates/SubscriptionsView/SubscriptionsView.component';
import { MatButtonToggleChange, MatButtonToggleGroup, MatButtonToggleModule } from '@angular/material/button-toggle';
import { ICanBeHiddenFromDisplay } from './shared/interfaces/ICanBeHiddenFromDisplay';
import { map, tap } from 'rxjs/operators';
import { ThemePalette } from '@angular/material/core';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MessageService } from 'primeng/api';
import { ToastMessage } from './models/ToastMessage';
import { PrimeNGConfig } from 'primeng/api';
import { IToggleToolChooser } from './shared/interfaces/IToggleToolChooser';
import jwt_decode from "jwt-decode";
import { Jwt } from 'jsonwebtoken';
export enum ToggleEnum {
  workbench,
  report
}
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
@AutoUnsubscribe()
export class AppComponent
  implements OnInit, OnDestroy, ICanBeHiddenFromDisplay, IToggleToolChooser {
  @ViewChild('toolSelector') toolSelector!: MatButtonToggleGroup;

  toggleEnum = ToggleEnum;
  selectedState = ToggleEnum.workbench;

  authExpiryTime: Date = new Date();

  isAutoRefreshWorkflowCheckpoint =
    this.applianceSvc.isAutoRefreshWorkflowCheckpoint;
  color: ThemePalette = 'accent';

  isRefreshing!: boolean;

  operatorPageModel!: OperatorPageModel;
  // operator page model change notification support
  private currentPageModelSource = new ReplaySubject<OperatorPageModel>();
  operatorPageModelChanges$ = this.currentPageModelSource.asObservable();

  baseUri: String | undefined;
  title = 'dashboard';
  public selectedView: string = 'workbench';
  toolSelectionSource = new BehaviorSubject<string>(this.selectedView);
  toolSelectionChanges$ = this.toolSelectionSource.asObservable();


  constructor(
    private apiConfigSvc: ApiConfigService,
    private primengConfig: PrimeNGConfig,
    private messageService: MessageService,
    private applianceSvc: ApplianceApiService
  ) {

        // preselect a tool
        this.toolSelectionSource.next( this.selectedView);

    this.primengConfig.ripple = true;
    this.isShow = false;
    this.apiConfigSvc.operatorPageModelChanges$.subscribe((data) => {
      console.log('app component has operator page model');

      try{

        const decodedJwt = jwt_decode(data.accessToken as string);
        // console.log("jwt decoded as " + JSON.stringify(decodedJwt));

        const jsonToken = JSON.parse(JSON.stringify(decodedJwt as string));
        // console.log(`jwt jsontoken ${jsonToken}`);
        const newDate = jsonToken.exp;

        console.log(`jwt exp = ${newDate}`);
        this.authExpiryTime = new Date((newDate as number) * 1000);
        console.log(`jwt expiry time ${this.authExpiryTime}`);
        const now = new Date();

        if(this.authExpiryTime.getTime() < now.getTime()){
          console.log("expired jwt");
        }
        else{
          console.log("not expired jwt");
        }
      }
      catch(e){
        console.log(`jwt exception ${e}`);
      }

      this.baseUri = data.applianceUrl?.toString();
      this.currentPageModelSource.next(data);
      return (this.operatorPageModel = data);
    });

  }
  isShow!: boolean;
  toggleDisplay(): void {
    console.log('toggling');
    this.isShow = !this.isShow;
  }

  ngOnDestroy(): void {
    // do no thing.
  }

  ngOnInit(): void {
    console.log('app component is initializing page model');
    this.apiConfigSvc
      .initPageModelSubject()
      .pipe(
        tap((t) => {
          const toast = new ToastMessage();
          toast.detail = 'loaded operator page model';
          toast.summary = 'page model refreshed';
          toast.severity = 'info';
          toast.sticky = false;
          this.showToast(toast);
        })
      )
      .subscribe();

        this.toolSelectionSource.next(this.selectedView);
  }

  toolChanged(e: MatButtonToggleChange): void{
    // console.log("tool selected " + this.selectedView);
    this.selectedView = e.value;
    this.selectedState = e.value;
    // this.toolSelectionSource.next(e.value);

  }

  showToast(message: ToastMessage): void {

    this.messageService.add(message);
  }
}
