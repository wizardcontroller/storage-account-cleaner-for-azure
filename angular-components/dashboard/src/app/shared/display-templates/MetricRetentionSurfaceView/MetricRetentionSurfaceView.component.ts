import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import dayGridPlugin from '@fullcalendar/daygrid';
import interactionPlugin from '@fullcalendar/interaction';
import timeGridPlugin from '@fullcalendar/timegrid';
import {
  OperatorPageModel,
  RetentionEntitiesService,
  StorageAccountDTO,
  TableStorageEntityRetentionPolicy,
  TableStorageEntityRetentionPolicyEnforcementResult
} from '@wizardcontroller/sac-appliance-lib';
import {
  ApplianceJobOutput,
  MetricsRetentionSurfaceItemEntity,
  TableStorageRetentionPolicy
} from '@wizardcontroller/sac-appliance-lib';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { PrimeIcons } from 'primeng/api';
import { DataView } from 'primeng/dataview';
import { FullCalendarModule } from 'primeng/fullcalendar';
import { combineLatest, ReplaySubject } from 'rxjs';
import { concatMap, map, withLatestFrom } from 'rxjs/operators';
import { ApiConfigService } from 'src/app/core/ApiConfig.service';
import { GlobalOhNoConstants } from '../../GlobalOhNoConstants';
import { ICanBeHiddenFromDisplay } from '../../interfaces/ICanBeHiddenFromDisplay';
import { DatesToTimeLineEventsPipePipe } from '../../pipes/dates-to-time-line-events-pipe.pipe';
import { RetentionPeriodForFullCalendarPipe } from '../../pipes/retention-Period-For-FullCalendar.pipe';
import { ApplianceApiService } from '../../services/appliance-api.service';
@Component({
  selector: 'app-MetricsRetentionSurfaceView',
  templateUrl: './MetricRetentionSurfaceView.component.html',
  styleUrls: ['./MetricRetentionSurfaceView.component.css']
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

  private acctSubject = new ReplaySubject<string>();
  selectedAccountChanges$ = this.acctSubject.asObservable();

  private pageModelSubuject = new ReplaySubject<OperatorPageModel>();
  pageModelChanges$ = this.pageModelSubuject.asObservable();


  metricsItemDependencies$ = this.pageModelChanges$.pipe(
    withLatestFrom(
      this.applianceAPiSvc.selectedStorageAccountAction$
      // this.selectedAccountChanges$
    )
  );

  // metricEntities$ = this.metricsItemDependencies$.subscribe(
  metricEntities$ = this.pageModelChanges$
    .pipe(
      concatMap((dependencyData) => this.metricsItemDependencies$)
    )
    .subscribe((dependencyData) => {
      const pageModel = dependencyData[0];
      const storageAccountId = dependencyData[1];

      console.log('getting metric entities');
      this.applianceAPiSvc.entityService
        .getRetentionPolicyForStorageAccount(
          pageModel.tenantid as string,
          pageModel.oid as string,
          pageModel.selectedSubscriptionId as string,
          storageAccountId as string
        )
        .subscribe((data: TableStorageRetentionPolicy) => {
          console.log('retention policy ' + JSON.stringify(data));
          this.metricsRetentionSurfaceEntitiesSource.next(
            data?.tableStorageTableRetentionPolicy?.metricRetentionSurface
              ?.metricsRetentionSurfaceItemEntities as Array<MetricsRetentionSurfaceItemEntity>
          );
        });
    });


  private entityRetentionPolicySource =
    new ReplaySubject<TableStorageEntityRetentionPolicy>();
  entityRetentionPolicyChanges$ =
    this.entityRetentionPolicySource.asObservable();

  selectedStorageAccount!: StorageAccountDTO;
  operatorPageModel!: OperatorPageModel;

  currentJobOutput: Array<ApplianceJobOutput> | undefined;
  private currentJobOutputSource = new ReplaySubject<Array<ApplianceJobOutput>>();
  currentJobOutputChanges$ = this.currentJobOutputSource.asObservable();

  metricsRetentionSurfaceEntities!: Array<MetricsRetentionSurfaceItemEntity>;
  private metricsRetentionSurfaceEntitiesSource = new ReplaySubject<
    Array<MetricsRetentionSurfaceItemEntity>
  >();
  metricsRetentionSurfaceEntityChanges$ =
    this.metricsRetentionSurfaceEntitiesSource.asObservable();

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

  isShow: boolean;
  toggleDisplay(): void {}

  ngOnInit() {

    this.options = {
      plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin],

      header: {
        left: 'prev,next',
        center: 'title',
        right: 'dayGridMonth,timeGridWeek,timeGridDay',
      },
      editable: true,
      dayMaxEvents: true
    };

    this.applianceAPiSvc.selectedStorageAccountAction$.subscribe((data) => {
      console.log('metrics retention surface has new selected storage account');

      this.acctSubject.next(data);

    });

    this.apiConfigSvc.operatorPageModelChanges$.subscribe((pageModel) => {
      // metrics retention component has operator page model
      this.pageModelSubuject.next(pageModel);
    });
  }

  filterChanged(e: boolean) {
    const filterExpression = e ? 'true' : 'false';

    this.dv.filter(filterExpression);
    console.log(`filter expression ${filterExpression}`);
    // this.filterItemsBtn.checked = e.returnValue;
  }
}
