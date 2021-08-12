import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import dayGridPlugin from '@fullcalendar/daygrid';
import interactionPlugin from '@fullcalendar/interaction';
import timeGridPlugin from '@fullcalendar/timegrid';
import {
  MetricRetentionSurfaceEntity,
  OperatorPageModel,
  RetentionEntitiesService,
  StorageAccountDTO,
  TableStorageEntityRetentionPolicy,
  TableStorageEntityRetentionPolicyEnforcementResult,
  TableStorageTableRetentionPolicy,
} from '@wizardcontroller/sac-appliance-lib';
import { PolicyEnforcementMode } from '@wizardcontroller/sac-appliance-lib';
import {
  ApplianceJobOutput,
  MetricsRetentionSurfaceItemEntity,
  TableStorageRetentionPolicy,
} from '@wizardcontroller/sac-appliance-lib';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { PrimeIcons } from 'primeng/api';
import { DataView } from 'primeng/dataview';
import { FullCalendarModule } from 'primeng/fullcalendar';
import { of } from 'rxjs';
import { combineLatest, ReplaySubject } from 'rxjs';
import { catchError, concatMap, map, switchMap, tap, withLatestFrom } from 'rxjs/operators';
import { ApiConfigService } from 'src/app/core/ApiConfig.service';
import { GlobalOhNoConstants } from '../../GlobalOhNoConstants';
import { ICanBeHiddenFromDisplay } from '../../interfaces/ICanBeHiddenFromDisplay';
import { DatesToTimeLineEventsPipePipe } from '../../pipes/dates-to-time-line-events-pipe.pipe';
import { RetentionPeriodForFullCalendarPipe } from '../../pipes/retention-Period-For-FullCalendar.pipe';
import { ApplianceApiService } from '../../services/appliance-api.service';
@Component({
  selector: 'app-MetricsRetentionSurfaceView',
  templateUrl: './MetricRetentionSurfaceView.component.html',
  styleUrls: ['./MetricRetentionSurfaceView.component.css'],
})
@AutoUnsubscribe()
export class MetricRetentionSurfaceViewComponent
  implements OnInit, OnDestroy, ICanBeHiddenFromDisplay
{
  @ViewChild('dv') dv!: DataView;

  events!: Array<any>;
  header!: any;
  options!: any;
  showOnlyExistingItems = false;

  isShow: boolean;

  enforcementMode = [
    PolicyEnforcementMode.applyPolicy,
    PolicyEnforcementMode.whatIf
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
          `metricsCurrentStorageAccount$ updated with storage account ${this.curentStorageAccount.name}`
        );
      }
    });

  metricEntitiesPipe$ = combineLatest([
    this.pageModelChanges$,
    this.applianceAPiSvc.currentStorageAccountChanges$,
    this.applianceAPiSvc.workflowCheckpointTimer$,
  ])
    .pipe(
      tap((t) => {
        console.log('metricEntitiesPipe$ updated with dependencies');
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
                `metrics has updated retention surface for ${storageAccountId.id}`
              );
            })
          )
          .subscribe((data: TableStorageRetentionPolicy) => {
            this.metricsRetentionSurfaceEntitiesSource.next(
              data?.tableStorageTableRetentionPolicy?.metricRetentionSurface
                ?.metricsRetentionSurfaceItemEntities as Array<MetricsRetentionSurfaceItemEntity>
            );

            this.currentRetentionPolicy = data;
          });
      })
    )
    .subscribe();

  private entityRetentionPolicySource =
    new ReplaySubject<TableStorageEntityRetentionPolicy>(1);
  entityRetentionPolicyChanges$ =
    this.entityRetentionPolicySource.asObservable();

  metricsRetentionSurfaceEntities!: Array<MetricsRetentionSurfaceItemEntity>;
  private metricsRetentionSurfaceEntitiesSource = new ReplaySubject<
    Array<MetricsRetentionSurfaceItemEntity>
  >();
  metricsRetentionSurfaceEntityChanges$ =
    this.metricsRetentionSurfaceEntitiesSource.asObservable();

  pageModelPipe = this.apiConfigSvc.operatorPageModelChanges$.pipe(
    map((pageModel) => {
      console.log('metrics retention view has updated page model');
      // metrics retention component has operator page model
      this.pageModelSubuject.next(pageModel);
    })
  );

  constructor(
    private apiConfigSvc: ApiConfigService,
    private applianceAPiSvc: ApplianceApiService,
    private route: ActivatedRoute
  ) {
    this.isShow = false;
  }
  ngOnDestroy(): void {
    // nothing yet
  }

  toggleDisplay(): void {}

  ngOnInit() {
    this.pageModelPipe.subscribe();

    this.options = {
      plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin],

      header: {
        left: 'prev,next',
        center: 'title',
        right: 'dayGridMonth,timeGridWeek,timeGridDay'
      },
      editable: true,
      dayMaxEvents: true,
      height: 310,
      contentHeight: 300
    };
  }

  filterChanged(e: boolean) {
    const filterExpression = e ? 'true' : 'false';

    this.dv.filter(filterExpression);
    console.log(`filter expression ${filterExpression}`);
    // this.filterItemsBtn.checked = e.returnValue;
  }

  public setEditMode(e: boolean): void {
    this.isShow = e;
  }

  updateRetentionPolicy(e: MetricsRetentionSurfaceItemEntity): void {
    this.isShow = false;
    this.applianceAPiSvc.isAutoRefreshWorkflowCheckpoint = false;

    const id = e.id as string;
    console.log(`sent ${id}`);
    let metricPolicy = this.currentRetentionPolicy.tableStorageTableRetentionPolicy as TableStorageTableRetentionPolicy;
    let surface = metricPolicy.metricRetentionSurface as MetricRetentionSurfaceEntity;
    let entities = surface.metricsRetentionSurfaceItemEntities;
    entities = [];
    entities.push(e);
    surface.metricsRetentionSurfaceItemEntities = entities;
    metricPolicy.metricRetentionSurface = surface;

    const submitPipe = combineLatest([
      this.pageModelChanges$,
      this.applianceAPiSvc.currentStorageAccountChanges$
    ])
    .pipe(
      tap(t =>{
        console.log("submitting metrics retention policy");
      }),
      switchMap(dependencies => {
        const pageModel = dependencies[0] as OperatorPageModel;
        const currentStorageAccount = dependencies[1] as StorageAccountDTO;

        this.applianceAPiSvc.entityService
        .setTableRetentionPolicyForStorageAccount(
          pageModel.tenantid as string,
          pageModel.oid as string,
          metricPolicy.id as string,
          e.id as string,
          pageModel.selectedSubscriptionId as string,
          this.curentStorageAccount.id as string, metricPolicy)
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
    ).subscribe().unsubscribe();

    console.log("policy submitted");
  }
}
