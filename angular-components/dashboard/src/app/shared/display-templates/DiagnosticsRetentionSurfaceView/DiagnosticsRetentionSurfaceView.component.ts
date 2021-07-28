import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';

import { ActivatedRoute } from '@angular/router';
import { DiagnosticsRetentionSurfaceItemEntity, OperatorPageModel, TableStorageRetentionPolicy } from '@wizardcontroller/sac-appliance-lib';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { ReplaySubject } from 'rxjs';
import { concatMap, withLatestFrom } from 'rxjs/operators';
import { ApiConfigService } from '../../../core/ApiConfig.service';
import { ICanBeHiddenFromDisplay } from '../../interfaces/ICanBeHiddenFromDisplay';
import { ApplianceApiService } from '../../services/appliance-api.service';
import { MatDrawer, MatSidenavModule } from '@angular/material/sidenav';
import { PrimeIcons } from 'primeng/api';
import { DataView } from 'primeng/dataview';
import { ToggleButton } from 'primeng/togglebutton';

import {FullCalendarModule} from 'primeng/fullcalendar';
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import interactionPlugin from '@fullcalendar/interaction';

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

  events!: any[];
  header!: any;
  options!: any;


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

    this.options = {
      plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin],

      header: {
          left: 'prev,next',
          center: 'title',
          right: 'dayGridMonth,timeGridWeek,timeGridDay'
      },
      editable: true,
      dayMaxEvents: true
  };

  this.events = [
    {
        "id": 1,
    "title": "All Day Event",
    "start": "2017-02-01"
    },
  {
        "id": 2,
    "title": "Long Event",
    "start": "2017-02-07",
    "end": "2017-02-10"
    },
  {
    "id": 3,
    "title": "Repeating Event",
    "start": "2017-02-09T16:00:00"

  }
  ];

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
