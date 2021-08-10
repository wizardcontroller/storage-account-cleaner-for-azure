import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';

import { ActivatedRoute } from '@angular/router';
import {
  DiagnosticsRetentionSurfaceItemEntity,
  OperatorPageModel,
  PolicyEnforcementMode,
  StorageAccountDTO,
  TableStorageEntityRetentionPolicy,
  TableStorageRetentionPolicy,
} from '@wizardcontroller/sac-appliance-lib';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { combineLatest, ReplaySubject } from 'rxjs';
import { catchError, concatMap, map, mergeMap, tap, withLatestFrom } from 'rxjs/operators';
import { ApiConfigService } from '../../../core/ApiConfig.service';
import { ICanBeHiddenFromDisplay } from '../../interfaces/ICanBeHiddenFromDisplay';
import { ApplianceApiService } from '../../services/appliance-api.service';
import { MatDrawer, MatSidenavModule } from '@angular/material/sidenav';
import { PrimeIcons } from 'primeng/api';
import { MessageService } from 'primeng/api';
import { DataView } from 'primeng/dataview';
import { ToggleButton } from 'primeng/togglebutton';

import { FullCalendarModule } from 'primeng/fullcalendar';
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import interactionPlugin from '@fullcalendar/interaction';
import { RetentionPeriodForFullCalendarPipe } from '../../pipes/retention-Period-For-FullCalendar.pipe';
import { RetentionSurfaceToolBase } from '../RetentionSurfaceToolBase';
import { of } from 'rxjs/internal/observable/of';
@Component({
  selector: 'app-DiagnosticsRetentionSurfaceView',
  templateUrl: './DiagnosticsRetentionSurfaceView.component.html',
  styleUrls: ['./DiagnosticsRetentionSurfaceView.component.css']
})
@AutoUnsubscribe()
export class DiagnosticsRetentionSurfaceViewComponent
  implements OnDestroy, OnInit, ICanBeHiddenFromDisplay
{
  @ViewChild('dv') dv!: DataView;
  @ViewChild('filterItemsBtn') filterItemsBtn!: ToggleButton;

  showOnlyExistingItems = false;
  isSideNavOpen = false;
  rangeDates!: Array<Date>;
  isShow!: boolean;
  events!: any[];
  header!: any;
  options!: any;

  enforcementMode = [
    PolicyEnforcementMode.whatIf,
    PolicyEnforcementMode.applyPolicy
  ];
  currentRetentionPolicy!: TableStorageRetentionPolicy;

  private pageModelSubuject = new ReplaySubject<OperatorPageModel>(1);
  pageModelChanges$ = this.pageModelSubuject.asObservable();

  curentStorageAccount!: StorageAccountDTO;
  currentStorageAccount$ =
    this.applianceAPiSvc.currentStorageAccountChanges$.subscribe((data) => {
      const newData = data;
      if (newData) {
        this.curentStorageAccount = newData;
        console.log(
          `diagnosticsCurrentStorageAccount$ updated with storage account ${this.curentStorageAccount.name}`
        );
      }
    });

  diagnosticEntitiesPipe$ = combineLatest([
    this.pageModelChanges$,
    this.applianceAPiSvc.currentStorageAccountChanges$,
    this.applianceAPiSvc.workflowCheckpointTimer$,
  ])
    .pipe(
      tap((t) => {
        console.log('diagnosticEntitiesPipe$ updated with dependencies');
      }),
      map((dependencyData) => {
        var pageModel = dependencyData[0];
        var storageAccountId = dependencyData[1];

        this.applianceAPiSvc.entityService
          .getRetentionPolicyForStorageAccount(
            pageModel.tenantid as string,
            pageModel.oid as string,
            pageModel.selectedSubscriptionId as string,
            storageAccountId.id as string
          )
          .pipe(
            tap((t) => {
              console.log(
                `diagnostics has updated retention surface for ${storageAccountId.id}`
              );
            })
          )
          .subscribe((data: TableStorageRetentionPolicy) => {
            this.diagnosticsRetentionSurfaceEntitySource.next(
              data?.tableStorageEntityRetentionPolicy
                ?.diagnosticsRetentionSurface
                ?.diagnosticsRetentionSurfaceEntities as DiagnosticsRetentionSurfaceItemEntity[]
            );

            this.currentRetentionPolicy = data;
            // console.log(`diagnosticentities$ updated storage account ${storageAccountId}`)
          });
      })
    )
    .subscribe();

  private diagnosticsRetentionSurfaceEntitySource = new ReplaySubject<
    DiagnosticsRetentionSurfaceItemEntity[]
  >(1);

  diagnosticsRetentionSurfaceEntityChanges$ =
    this.diagnosticsRetentionSurfaceEntitySource.asObservable();

  pageModelPipe = this.apiConfigSvc.operatorPageModelChanges$.pipe(
    map((pageModel) => {
      console.log('diagnostics retention view has updated page model');
      // metrics retention component has operator page model
      this.pageModelSubuject.next(pageModel);
    })
  );

  constructor(
    private apiConfigSvc: ApiConfigService,
    private applianceAPiSvc: ApplianceApiService
  ) {
    this.isShow = false;

  }

  ngOnDestroy(): void {
    // nothing yet
  }

  ngOnInit() {
    this.pageModelPipe.subscribe();

    this.options = {
      plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin],

      header: {
        left: 'prev,next',
        center: 'title',
        right: 'dayGridMonth,timeGridWeek,timeGridDay',
      },
      editable: true,
      dayMaxEvents: true,
      height: 310,
      contentHeight: 300,
    };
  }

  toggleSideNav(): void {
    console.log('toggling sidenav');
    this.isSideNavOpen = !this.isSideNavOpen;
  }

  filterChanged(e: boolean): void {
    const filterExpression = e ? 'true' : 'false';

    this.dv.filter(filterExpression);
    console.log(`filter expression ${filterExpression}`);
    // this.filterItemsBtn.checked = e.returnValue;
  }

  public setEditMode(e: boolean): void {
    this.isShow = e;
  }

  public updateRetentionPolicy(e: DiagnosticsRetentionSurfaceItemEntity): void {
    this.isShow = false;
    this.applianceAPiSvc.isAutoRefreshWorkflowCheckpoint = false;

    const id = e.id as string;
    console.log(`sent ${id}`);
    const diagnosticPolicy = this.currentRetentionPolicy.tableStorageEntityRetentionPolicy as TableStorageEntityRetentionPolicy;

    const submitPipe = combineLatest([
      this.pageModelChanges$,
      this.applianceAPiSvc.currentStorageAccountChanges$
    ])
    .pipe(
      tap(t =>{
        console.log("submitting metrics retention policy");
      }),
      map(dependencies => {
        const pageModel = dependencies[0] as OperatorPageModel;
        const currentStorageAccount = dependencies[1] as StorageAccountDTO;

        this.applianceAPiSvc.entityService
        .setEntityRetentionPolicyForStorageAccount(
          pageModel.tenantid as string,
          pageModel.oid as string,
          diagnosticPolicy.id as string,
          e.id as string,
          pageModel.selectedSubscriptionId as string,
          this.curentStorageAccount.id as string, e)
          .pipe(
            map(result => {
              this.applianceAPiSvc.isAutoRefreshWorkflowCheckpoint = true;

              return result;
            }),
            catchError(err=> {
              console.log("error submitting metric retention policy");
              return of([]);
            })
          ).subscribe();

          return dependencies;
      }),
      catchError(err=> {
        console.log("error submitting metric retention policy");
        this.applianceAPiSvc.isAutoRefreshWorkflowCheckpoint = true;

        return of([]);
      })
    ).subscribe();

    console.log("policy submitted");

  }

  toggleDisplay(): void {}
}
