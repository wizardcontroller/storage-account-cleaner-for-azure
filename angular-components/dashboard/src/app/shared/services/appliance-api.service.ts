import { Injectable } from '@angular/core';
import {
  ApplianceSessionContext,
  ConfigService,
  OperatorPageModel,
  RetentionEntitiesService,
  StorageAccountDTO,
  WorkflowCheckpointDTO,
} from '@wizardcontroller/sac-appliance-lib';

import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { config, ReplaySubject, Subject } from 'rxjs';
import { ApiConfigService } from 'src/app/core/ApiConfig.service';
import { ApplianceContextService } from '../display-templates/services/ApplianceContext.service';

@Injectable({
  providedIn: 'root',
})
export class ApplianceApiService {
  operatorPageModel!: OperatorPageModel | null;

  // this is the ApplianceSessionContext source of truth
  applianceContext!: ApplianceSessionContext | null;
  private currentApplianceSessionContextSource = new ReplaySubject<ApplianceSessionContext>();
  applianceSessionContextChanges$ = this.currentApplianceSessionContextSource.asObservable();

  workflowCheckPoint!: WorkflowCheckpointDTO | null;
  private currentWorkflowCheckpointSource = new ReplaySubject<WorkflowCheckpointDTO>();
  workflowCheckpointChanges$ = this.currentWorkflowCheckpointSource.asObservable();

  storageAccounts!: StorageAccountDTO[];
  private storageAccountsSource = new ReplaySubject<StorageAccountDTO[]  | undefined | null>();
  storageAccountChanges$ = this.storageAccountsSource.asObservable();

  baseUri: string | undefined;

  constructor(
    private apiConfigService: ApiConfigService,
    private configService: ConfigService,
    private entityService: RetentionEntitiesService
  ) {
    console.log('ApplianceApiService is starting');

    this.ensurePageModelSubject();
    console.log('appliance api service done startup');
  }

  public ensureApplianceSessionContextSubject(tenantId: string, subscriptionId: string, oid: string): void{
    this.entityService.getApplianceSessionContext(tenantId,oid,subscriptionId).subscribe(data =>{
      console.log("apliance session context updated");
      this.currentApplianceSessionContextSource.next(data);


      var accounts = data.selectedStorageAccounts as StorageAccountDTO[];

      this.storageAccountsSource.next(accounts);
    }, error => {
      console.log("error getting session context: " + (error as Error).message);
    });
  }

  public ensureWorkflowSessionContextSubject(tenantId: string, subscriptionId: string, oid: string) : void{
    this.entityService.getWorkflowCheckpoint(tenantId,oid).subscribe(data =>{
      console.log("workflow context updated");
      this.currentWorkflowCheckpointSource.next(data);
    }, error =>{
      console.log("error getting workflow context: " + (error as Error).message);
    });
  }

  /**
   * on pagemodel retrieval update dependent DTOs
   */
  public ensurePageModelSubject() : void{

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
        this.ensureWorkflowSessionContextSubject(tenantid,subscriptionId,oid);

        console.log("getting appliance context");
        this.ensureApplianceSessionContextSubject(tenantid,subscriptionId,oid);
      } catch (ex) {
        console.log('error starting ApplianceApiSvc ' + (ex as Error).message);

      }

      return (this.operatorPageModel = data);
    });
  }

  private ensureServiceUrls(data: OperatorPageModel) {
    var baseUrl = data?.applianceUrl?.toString().replace("/api/","");
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
