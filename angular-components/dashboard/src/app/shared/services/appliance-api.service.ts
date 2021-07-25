import { Injectable, OnDestroy } from '@angular/core';
import { MAT_MENU_DEFAULT_OPTIONS_FACTORY } from '@angular/material/menu/menu';
import {
  ApplianceJobOutput,
  ApplianceSessionContext,
  ConfigService,
  MetricsRetentionSurfaceItemEntity,
  OperatorPageModel,
  RetentionEntitiesService,
  StorageAccountDTO,
  TableStorageEntityRetentionPolicy,
  WorkflowCheckpointDTO,
} from '@wizardcontroller/sac-appliance-lib';

import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { BehaviorSubject, combineLatest, config, from, interval, of, ReplaySubject, Subject, timer } from 'rxjs';
import { concatMap, debounce, debounceTime, flatMap, map, mergeMap, publishReplay, refCount, share, shareReplay, switchMap, tap } from 'rxjs/operators';
import { ApiConfigService } from 'src/app/core/ApiConfig.service';
import { ApplianceContextService } from '../display-templates/services/ApplianceContext.service';
import { GlobalOhNoConstants } from '../GlobalOhNoConstants';
import { OperationStatus } from '../models/OperationStatus';

@Injectable({
  providedIn: 'root',
})

@AutoUnsubscribe()
export class ApplianceApiService implements OnDestroy {
  operatorPageModel!: OperatorPageModel | null;

  isRefreshingSource = new ReplaySubject<boolean>();
  isRefreshingChanges$ = this.isRefreshingSource.asObservable();

  statusFeedSource = new ReplaySubject<OperationStatus>();
  statusFeedChanges$ = this.statusFeedSource.asObservable();

  // this is the ApplianceSessionContext source of truth
  applianceContext!: ApplianceSessionContext | null;
  private currentApplianceSessionContextSource = new ReplaySubject<ApplianceSessionContext>();
  applianceSessionContextChanges$ = this.currentApplianceSessionContextSource.asObservable();

  currentJobOutput!: ApplianceJobOutput | null;
  private currentJobOutputSource = new ReplaySubject<ApplianceJobOutput>();
  currentJobOutputChanges$ = this.currentJobOutputSource.asObservable();

  workflowCheckPoint!: WorkflowCheckpointDTO | null;
  private currentWorkflowCheckpointSource = new Subject<WorkflowCheckpointDTO>();
  workflowCheckpointChanges$ = this.currentWorkflowCheckpointSource.asObservable();

  isAutoRefreshWorkflowCheckpoint = true;
  workflowCheckpointPollingStartDelay = 1500; //1.5 seconds
  workflowCheckpointPollingInterval = (1000 * 1) * ( 1 * 60); // seconds * minutes
  storageAccounts: StorageAccountDTO[] = [];
  private storageAccountsSource = new ReplaySubject<StorageAccountDTO[] | undefined | null>();
  storageAccountChanges$ = this.storageAccountsSource.asObservable();


  // support action stream of storage accounts selections
  private stopPolling = new Subject();
  workflowCheckpointTimer$ = timer(this.workflowCheckpointPollingStartDelay,
    this.workflowCheckpointPollingInterval);
  workflowCheckpoints$ = combineLatest(
    this.workflowCheckpointTimer$,
    this.apiConfigService.operatorPageModelChanges$
  )
    .pipe(

      tap(t => {

      }),
      map(([elapsedEvent, pageModel]) => {

        this.isRefreshingSource.next(true);

        var tenantid = pageModel.tenantid as string;

        var subscriptionId = pageModel.subscriptionId as string;

        var oid = pageModel.oid as string;

        this.ensureWorkflowSessionContextSubject(tenantid, subscriptionId, oid);

        this.isRefreshingSource.next(false);

      }),    share()

  ).subscribe();

  public selectedStorageAccountSource = new BehaviorSubject<string>("");
  selectedStorageAccountAction$ = this.selectedStorageAccountSource.asObservable();
  selectedStorageAccount$ = combineLatest(
    [
      this.storageAccountChanges$,
      this.selectedStorageAccountAction$
    ]
  )
    .pipe(

      map(([storageAccounts, selectedStorageAccountId]) => this.storageAccounts.
        filter(storageAccount => selectedStorageAccountId ? storageAccount.id === selectedStorageAccountId : true)
      )
    ,     share());


  private entityRetentionPolicySource = new ReplaySubject<TableStorageEntityRetentionPolicy>();
  entityRetentionPolicyChanges$ = this.entityRetentionPolicySource.asObservable();

  baseUri: string | undefined;

  constructor(
    private apiConfigService: ApiConfigService,
    private configService: ConfigService,
    public entityService: RetentionEntitiesService
  ) {
    console.log('ApplianceApiService is starting');
    // this.isRefreshingSource.next(true);

    this.ensurePageModelSubject();
    // this.isRefreshingSource.next(false);

    console.log('appliance api service done startup');
  }

  ngOnDestroy(): void {
    // nothing yet

  }

  public ensureApplianceSessionContextSubject(tenantId: string, subscriptionId: string, oid: string): void {

    this.entityService.getApplianceSessionContext(tenantId, oid)
      .pipe(

        map(data => {

          console.log("apliance session context updated");
          this.currentApplianceSessionContextSource.next(data);
          this.currentJobOutputSource.next(data.currentJobOutput);

          var accounts = data.selectedStorageAccounts as Array<StorageAccountDTO>;

          this.storageAccounts = accounts;
          this.storageAccountsSource.next(accounts);

          // 'select' a newly available storage account
          this.selectedStorageAccountSource.next(accounts[0].id as string);


        }),     share()
      ).subscribe();

    /**
    this.entityService.getApplianceSessionContext(tenantId, oid).subscribe(data => {
      console.log("apliance session context updated");
      this.currentApplianceSessionContextSource.next(data);
      this.currentJobOutputSource.next(data.currentJobOutput);

      var accounts = data.selectedStorageAccounts as Array<StorageAccountDTO>;

      this.storageAccounts = accounts;
      this.storageAccountsSource.next(accounts);

      // 'select' a newly available storage account
      this.selectedStorageAccountSource.next(accounts[0].id as string);

      // can listen selected storage account changes
      // required for calls to retention service that require storage account header
      //this.selectedStorageAccount$.subscribe(selectedAcct => {
      //var acct = selectedAcct.pop();

      // this is where this code would go if needed
      //console.log("using account name " + acct?.name);
      // set the storage account header

      //      });
      //    }, error => {
      //      console.log("error getting session context: " + (error as Error).message);
    });
    */
  }

  public ensureWorkflowSessionContextSubject(tenantId: string, subscriptionId: string, oid: string): void {

    if (this.isAutoRefreshWorkflowCheckpoint) {

      this.entityService.getWorkflowCheckpoint(tenantId, oid)
        .pipe(

          map(data => {

            console.log("workflow context updated");


            this.workflowCheckPoint = data;
            this.currentWorkflowCheckpointSource.next(data);


          }),     share()
      ).subscribe();

      /*
      this.entityService.getWorkflowCheckpoint(tenantId, oid).subscribe(data => {
        console.log("workflow context updated");


        this.workflowCheckPoint = data;
        this.currentWorkflowCheckpointSource.next(data);
      }, error => {
        console.log("error getting workflow context: " + (error as Error).message);
      });
      */
    }
    else {

    }
    }

  /**
   * on pagemodel retrieval update dependent DTOs
   */
  public ensurePageModelSubject(): void {


    this.apiConfigService.operatorPageModelChanges$
      .pipe(

        map(data => {


          console.log('appliance api service has operator page model');
          try {
            this.operatorPageModel = data
            this.ensureServiceUrls(data);
            var tenantid = data.tenantid as string;
            console.log("ensurePageModelSubject(): tenantId: " + tenantid);

            var subscriptionId = data.subscriptionId as string;
            console.log("ensurePageModelSubject(): subscriptionId: " + subscriptionId);

            var oid = data.oid as string;
            console.log("ensurePageModelSubject(): oid: " + oid);

            console.log("getting workflow checkpoint");
            this.ensureWorkflowSessionContextSubject(tenantid, subscriptionId, oid);

            console.log("getting appliance context");
            this.ensureApplianceSessionContextSubject(tenantid, subscriptionId, oid);

          } catch (ex) {
            console.log('error starting ApplianceApiSvc ' + (ex as Error).message);

          }

          this.operatorPageModel = data;


        }),     share()
      ).subscribe();
  }

  private ensureServiceUrls(data: OperatorPageModel) {
    var baseUrl = data?.applianceUrl?.toString().replace("/api/", "");
    this.baseUri = baseUrl;
    console.log('appliance api service is configuring baseUri' + this.baseUri);

    this.entityService.configuration.basePath =
      baseUrl;
    console.log(
      'configured appliance base path ' +
      baseUrl
    );
  }
}
