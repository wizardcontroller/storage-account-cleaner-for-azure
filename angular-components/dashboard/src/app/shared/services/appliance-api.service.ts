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
import { BehaviorSubject, combineLatest, config, ReplaySubject, Subject } from 'rxjs';
import { flatMap, map, tap } from 'rxjs/operators';
import { ApiConfigService } from 'src/app/core/ApiConfig.service';
import { ApplianceContextService } from '../display-templates/services/ApplianceContext.service';
import { GlobalOhNoConstants } from '../GlobalOhNoConstants';

@Injectable({
  providedIn: 'root',
})

@AutoUnsubscribe()
export class ApplianceApiService implements OnDestroy {
  operatorPageModel!: OperatorPageModel | null;

  // this is the ApplianceSessionContext source of truth
  applianceContext!: ApplianceSessionContext | null;
  private currentApplianceSessionContextSource = new ReplaySubject<ApplianceSessionContext>();
  applianceSessionContextChanges$ = this.currentApplianceSessionContextSource.asObservable();

  currentJobOutput!: ApplianceJobOutput | null;
  private currentJobOutputSource = new ReplaySubject<ApplianceJobOutput>();
  currentJobOutputChanges$ = this.currentJobOutputSource.asObservable();

  workflowCheckPoint!: WorkflowCheckpointDTO | null;
  private currentWorkflowCheckpointSource = new ReplaySubject<WorkflowCheckpointDTO>();
  workflowCheckpointChanges$ = this.currentWorkflowCheckpointSource.asObservable();

  storageAccounts: StorageAccountDTO[] = [];
  private storageAccountsSource = new ReplaySubject<StorageAccountDTO[] | undefined | null>();
  storageAccountChanges$ = this.storageAccountsSource.asObservable();

  // support action stream of storage accounts selections

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
      ));






  private entityRetentionPolicySource = new ReplaySubject<TableStorageEntityRetentionPolicy>();
  entityRetentionPolicyChanges$ = this.entityRetentionPolicySource.asObservable();

  baseUri: string | undefined;

  constructor(
    private apiConfigService: ApiConfigService,
    private configService: ConfigService,
    public entityService: RetentionEntitiesService
  ) {
    console.log('ApplianceApiService is starting');

    this.ensurePageModelSubject();
    console.log('appliance api service done startup');
  }

  ngOnDestroy(): void {
    // nothing yet
  }

  public ensureApplianceSessionContextSubject(tenantId: string, subscriptionId: string, oid: string): void {
    this.entityService.getApplianceSessionContext(tenantId, oid, subscriptionId).subscribe(data => {
      console.log("apliance session context updated");
      this.currentApplianceSessionContextSource.next(data);
      this.currentJobOutputSource.next(data.currentJobOutput);

      var accounts = data.selectedStorageAccounts as StorageAccountDTO[];

      this.storageAccounts = accounts;
      this.storageAccountsSource.next(accounts);

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

  }

  public ensureWorkflowSessionContextSubject(tenantId: string, subscriptionId: string, oid: string): void {
    this.entityService.getWorkflowCheckpoint(tenantId, oid).subscribe(data => {
      console.log("workflow context updated");
      this.currentWorkflowCheckpointSource.next(data);
    }, error => {
      console.log("error getting workflow context: " + (error as Error).message);
    });
  }

  /**
   * on pagemodel retrieval update dependent DTOs
   */
  public ensurePageModelSubject(): void {


    this.apiConfigService.operatorPageModelChanges$.subscribe((data) => {
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

      return (this.operatorPageModel = data);
    });

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
