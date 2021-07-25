import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ApiConfigService } from 'src/app/core/ApiConfig.service';
import { ICanBeHiddenFromDisplay } from '../../interfaces/ICanBeHiddenFromDisplay';
import { ApplianceApiService } from '../../services/appliance-api.service';
import { DatesToTimeLineEventsPipePipe } from '../../pipes/dates-to-time-line-events-pipe.pipe';
import { PrimeIcons } from 'primeng/api';
import {
  OperatorPageModel,
  RetentionEntitiesService,
  StorageAccountDTO,
  TableStorageEntityRetentionPolicy,
  TableStorageEntityRetentionPolicyEnforcementResult,
} from '@wizardcontroller/sac-appliance-lib';
import { combineLatest, ReplaySubject } from 'rxjs';
import { GlobalOhNoConstants } from '../../GlobalOhNoConstants';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import {
  ApplianceJobOutput,
  MetricsRetentionSurfaceItemEntity,
  TableStorageRetentionPolicy,
} from '@wizardcontroller/sac-appliance-lib';
import { concatMap, map, withLatestFrom } from 'rxjs/operators';
import { DataView } from 'primeng/dataview';
@Component({
  templateUrl: './MetricRetentionSurfaceView.component.html',
  styleUrls: ['./MetricRetentionSurfaceView.component.css'],
})
@AutoUnsubscribe()
export class MetricRetentionSurfaceViewComponent
  implements OnInit, OnDestroy, ICanBeHiddenFromDisplay
{
  @ViewChild('dv') dv!: DataView;
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
    .pipe(concatMap((dependencyData) => this.metricsItemDependencies$))
    .subscribe((dependencyData) => {
      var pageModel = dependencyData[0];
      var storageAccountId = dependencyData[1];

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
              ?.metricsRetentionSurfaceItemEntities as MetricsRetentionSurfaceItemEntity[]
          );
        });
    });

  private entityRetentionPolicySource =
    new ReplaySubject<TableStorageEntityRetentionPolicy>();
  entityRetentionPolicyChanges$ =
    this.entityRetentionPolicySource.asObservable();

  selectedStorageAccount!: StorageAccountDTO;
  operatorPageModel!: OperatorPageModel;

  currentJobOutput: ApplianceJobOutput[] | undefined;
  private currentJobOutputSource = new ReplaySubject<ApplianceJobOutput[]>();
  currentJobOutputChanges$ = this.currentJobOutputSource.asObservable();

  metricsRetentionSurfaceEntities!: MetricsRetentionSurfaceItemEntity[];
  private metricsRetentionSurfaceEntitiesSource = new ReplaySubject<
    MetricsRetentionSurfaceItemEntity[]
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
