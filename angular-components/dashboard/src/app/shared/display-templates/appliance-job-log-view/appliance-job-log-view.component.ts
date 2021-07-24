import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { JobOutputLogEntry, RetentionEntitiesService } from '@wizardcontroller/sac-appliance-lib';
import { combineLatest, iif, timer } from 'rxjs';
import { concatMap, filter, map, mergeMap, tap, withLatestFrom } from 'rxjs/operators';
import { ApiConfigService } from 'src/app/core/ApiConfig.service';
import { ApplianceApiService } from '../../services/appliance-api.service';
import { OperatorPageModel, JobOutputLogEntity } from '@wizardcontroller/sac-appliance-lib'
import { ReplaySubject } from 'rxjs';
import { DataViewModule } from 'primeng/dataview';
import {LazyLoadEvent } from 'primeng/api'
@Component({
  selector: 'app-appliance-job-log-view',
  templateUrl: './appliance-job-log-view.component.html',
  styleUrls: ['./appliance-job-log-view.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ApplianceJobLogViewComponent implements OnInit {


  isRefreshing = false;
  applianceLogChangesTimerPollingInterval: number = 1000 * 30;
  applianceLogPageSize = 5;
  applianceLogPageCount = 1;

  applianceLogOffset = 0;
  totalLogEntries = 0;

  jobOutputLogSubject = new ReplaySubject<JobOutputLogEntry[] | null | undefined>();
  jobOutputLogChanges$ = this.jobOutputLogSubject.asObservable();

  applianceLogChangesTimer$ = timer(0,
    this.applianceLogChangesTimerPollingInterval);

  applianceLogChanges$ = combineLatest(
    this.apiConfigSvc.operatorPageModelChanges$,
    this.applianceLogChangesTimer$
  )
    .pipe(
      filter(([pageModel$, elapsedEvent$]) => this.applianceApiSvc.isAutoRefreshWorkflowCheckpoint),
      tap(() => console.log("autorefresh filtered applianceLog changes firing")),
      map(([pageModel, elapsedEvent ]) => {

        this.isRefreshing = true;
      var tenantid = pageModel.tenantid as string;

      var subscriptionId = pageModel.subscriptionId as string;

      var oid = pageModel.oid as string;

      this.applianceApiSvc.entityService
        .getApplianceLogEntries(tenantid as string,
          oid as string, 0, this.applianceLogPageSize, 1)
        .pipe(

          map(logEntryEntity => {
            this.totalLogEntries = logEntryEntity.rowCount as number;
            this.jobOutputLogSubject.next(logEntryEntity.logEntries);


        this.isRefreshing = false;
            return logEntryEntity;
        })
      )
      .subscribe() ;
    })
  )
  .subscribe();

  constructor(private apiConfigSvc: ApiConfigService,
    private retentionEntitiesSvc: RetentionEntitiesService,
    private applianceApiSvc: ApplianceApiService) {


  }

  ensureLogEntries(event: LazyLoadEvent) {
    this.applianceLogOffset = event.first as number;
    
    this.applianceLogPageSize = event.rows as number;
    console.log("ensuring log entries - component requested # of rows = " + event.rows);

    this.jobOutputLogChanges$.subscribe(data => {

    });
  }

  ngOnInit(): void {

  }

}
