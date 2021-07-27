import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';

import { ActivatedRoute } from '@angular/router';
import { DiagnosticsRetentionSurfaceItemEntity, OperatorPageModel, TableStorageRetentionPolicy } from '@wizardcontroller/sac-appliance-lib';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { ReplaySubject } from 'rxjs';
import { concatMap, withLatestFrom } from 'rxjs/operators';
import { ApiConfigService } from '../../../core/ApiConfig.service';
import { ICanBeHiddenFromDisplay } from '../../interfaces/ICanBeHiddenFromDisplay';
import { ApplianceApiService } from '../../services/appliance-api.service';
import { MatDrawer, MatSidenavModule } from '@angular/material/sidenav'
import { PrimeIcons } from 'primeng/api'
import { DataView } from 'primeng/dataview';
import { ToggleButton } from 'primeng/togglebutton';
import { CalendarOptions } from '@fullcalendar/angular'; // useful for typechecking
@Component({

  templateUrl: './DiagnosticsRetentionSurfaceView.component.html',
  styleUrls: ['./DiagnosticsRetentionSurfaceView.component.css']
})

@AutoUnsubscribe()
export class DiagnosticsRetentionSurfaceViewComponent implements OnDestroy, OnInit, ICanBeHiddenFromDisplay {
  @ViewChild('dv') dv!: DataView;
  @ViewChild('filterItemsBtn') filterItemsBtn! : ToggleButton;
  showOnlyExistingItems = false;
  isSideNavOpen = false;

  calendarOptions: CalendarOptions = {
    initialView: 'dayGridMonth',
    events: [
      { title: 'event 1', date: '2019-04-01' },
      { title: 'event 2', date: '2019-04-02' }
    ]
  };

  private pageModelSubuject = new ReplaySubject<OperatorPageModel>();
  pageModelChanges$ = this.pageModelSubuject.asObservable();

  diagnosticsItemDependencies$ = this.pageModelChanges$.pipe(
    withLatestFrom(
      this.applianceAPiSvc.selectedStorageAccountAction$
      // this.selectedAccountChanges$
    )
  )

  metricEntities$ = this.pageModelChanges$.pipe(
    concatMap(dependencyData => this.diagnosticsItemDependencies$
    )).subscribe(
      dependencyData => {
        var pageModel = dependencyData[0];
        var storageAccountId = dependencyData[1];

        console.log("getting metric entities");
        this.applianceAPiSvc.entityService.getRetentionPolicyForStorageAccount(pageModel.tenantid as string,
          pageModel.oid as string, pageModel.selectedSubscriptionId as string, storageAccountId as string)
          .subscribe((data: TableStorageRetentionPolicy) => {
            console.log("retention policy " + JSON.stringify(data));
            this.diagnosticsRetentionSurfaceEntitySource
              .next(data?.tableStorageEntityRetentionPolicy?.diagnosticsRetentionSurface
                ?.diagnosticsRetentionSurfaceEntities as DiagnosticsRetentionSurfaceItemEntity[])
          });

      })

  private diagnosticsRetentionSurfaceEntitySource = new ReplaySubject<DiagnosticsRetentionSurfaceItemEntity[]>();
  diagnosticsRetentionSurfaceEntityChanges$ = this.diagnosticsRetentionSurfaceEntitySource.asObservable();

  constructor(
    private apiConfigSvc: ApiConfigService,
    private applianceAPiSvc: ApplianceApiService,
    private route: ActivatedRoute) {
    this.isShow = false;
  }

  ngOnDestroy(): void {
       // nothing yet
  }

  isShow: boolean;
  toggleDisplay(): void {

  }

  ngOnInit() {

    this.apiConfigSvc.operatorPageModelChanges$.subscribe(pageModel => {
      console.log("diagnostics retention view has page model");
      // metrics retention component has operator page model
      this.pageModelSubuject.next(pageModel);
    });
  }

  toggleSideNav(): void {
    console.log("toggling sidenav");
    this.isSideNavOpen = !this.isSideNavOpen;
  }

  filterChanged(e: boolean){

    const filterExpression = e ? "true" : "false";

    this.dv.filter(filterExpression);
    console.log(`filter expression ${filterExpression}`);
    // this.filterItemsBtn.checked = e.returnValue;
  }

}
