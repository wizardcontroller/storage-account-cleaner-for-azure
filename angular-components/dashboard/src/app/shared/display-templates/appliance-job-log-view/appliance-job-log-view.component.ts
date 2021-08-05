import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { JobOutputLogEntry, RetentionEntitiesService } from '@wizardcontroller/sac-appliance-lib';
import { combineLatest, iif, Subject, timer } from 'rxjs';
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



  cachedLogJson!: JobOutputLogEntry[];

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

  logChanges$ = this.applianceLogChangesTimer$.pipe(
    withLatestFrom(this.apiConfigSvc.operatorPageModelChanges$),
    filter(([pageModel$, elapsedEvent$]) => {
      if( this.applianceApiSvc.isAutoRefreshWorkflowCheckpoint){
        console.log("timer updates active");
      }
      else{
        console.log("timer updates paused");
      }

      return this.applianceApiSvc.isAutoRefreshWorkflowCheckpoint
    }),
    tap(() => console.log("withlatestFrom() autorefresh filtered applianceLog changes firing")),
    map(([ elapsedEvent, pageModel ]) => {

      this.isRefreshing = true;
    var tenantid = pageModel.tenantid as string;

    var subscriptionId = pageModel.subscriptionId as string;

    var oid = pageModel.oid as string;
    console.log("withLatestFrom() is getting log entries");
    this.applianceApiSvc.entityService
      .getApplianceLogEntries(tenantid as string,
        oid as string,this.applianceLogOffset, this.applianceLogPageSize, this.applianceLogPageCount + 1)
      .pipe(

        map(logEntryEntity => {

          console.log("withLatestFrom() has retrieved log entries. rowcount = " + logEntryEntity.rowCount, " retrieved # of rows=" + logEntryEntity.logEntries?.length);

          // this is the purpose of this pipe - to update the total record count
          this.totalLogEntries = logEntryEntity.rowCount as number;

          // don't do this here
          // this.jobOutputLogSubject.next(logEntryEntity.logEntries);

          // this.cachedLogJson = logEntryEntity.logEntries as JobOutputLogEntry[];

            logEntryEntity.logEntries?.forEach(item => {
              this.cachedLogJson.push(item);
            });
      this.isRefreshing = false;
          return logEntryEntity;
      })
    ).subscribe()
  }),
  tap(t => {
    this.isRefreshing = false;
  })
); //.subscribe(); //.subscribe(); //.subscribe();


  constructor(private apiConfigSvc: ApiConfigService,
    private retentionEntitiesSvc: RetentionEntitiesService,
    private applianceApiSvc: ApplianceApiService) {


  }

  ensureLogEntries(event: LazyLoadEvent) {

    this.applianceLogPageSize = event.rows as number;
    this.applianceLogOffset = event.first as number;
    console.log("ensureLogEntries(): ensuring log entries - component requested # of rows = " + event.rows as string + " at offset " + this.applianceLogOffset as string);

    this.getLatestLogEntries(this.applianceLogOffset, this.applianceLogPageSize * 2 ).subscribe();
  }

  getLatestLogEntries(offset: number, rows: number){

    return this.apiConfigSvc.operatorPageModelChanges$.pipe(
      map(pageModel =>{
        const tenantid = pageModel.tenantid as string;

        const subscriptionId = pageModel.subscriptionId as string;

        const oid = pageModel.oid as string;
        console.log(`getLatestLogEntries() for tenantId= ${tenantid} oid= ${oid} offset= ${offset} rows= ${rows}`);
        this.applianceApiSvc.entityService
        .getApplianceLogEntries(tenantid, oid, offset, rows, 2).subscribe(
          logEntryEntity => {

            this.totalLogEntries = logEntryEntity.rowCount as number;

            this.jobOutputLogSubject.next(logEntryEntity.logEntries);

            console.log(`getLatestLogEntries() for tenantId= ${tenantid} oid= ${oid} offset= ${offset} rows= ${rows} retrieved ${this.totalLogEntries} total log entries and ${logEntryEntity.logEntries?.length} available entities`);

          }
        );
      })
    );
  }

  ngOnInit(): void {

    this.getLatestLogEntries(0, 20).subscribe(
        data => {

         // console.log("appliance log initializng with " + data.lo);
         }
    );

    console.log("appliance job log is validting pagemodel subject");

  }

}
