import { Injectable } from '@angular/core';
import {
  ApplianceSessionContext,
  ConfigService,
  OperatorPageModel,
  RetentionEntitiesService,
  WorkflowCheckpointDTO,
} from '@wizardcontroller/sac-appliance-lib';
import { timeStamp } from 'console';
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
    });
  }

  public ensureWorkflowSessionContextSubject(tenantId: string, subscriptionId: string, oid: string) : void{
    this.entityService.getWorkflowCheckpoint(tenantId,oid).subscribe(data =>{
      console.log("workflow context updated");
      this.currentWorkflowCheckpointSource.next(data);
    });
  }

  /**
   * on pagemodel retrieval update dependent DTOs
   */
  public ensurePageModelSubject() : void{

    this.apiConfigService.operatorPageModelChanges$.subscribe((data) => {
      console.log('appliance api service has operator page model');
      try {
        this.ensureServiceUrls(data);
        var tenantid = data.tenantid as string;
        var subscriptionId = data.subscriptionId as string;
        var oid = data.oid as string;

        this.ensureWorkflowSessionContextSubject(tenantid,subscriptionId,oid);

        this.ensureApplianceSessionContextSubject(tenantid,subscriptionId,oid);
      } catch (err) {
        console.log('error starting ApplianceApiSvc');
      }

      return (this.operatorPageModel = data);
    });
  }

  private ensureServiceUrls(data: OperatorPageModel) {
    this.baseUri = data?.applianceUrl?.toString();
    console.log('appliance api service is configuring access token' + this.entityService.configuration.accessToken);

    this.entityService.configuration.basePath =
      this.operatorPageModel?.applianceUrl?.toString();
    console.log(
      'configured easyauth token ' +
      this.operatorPageModel?.easyAuthAccessToken
    );
  }
}
