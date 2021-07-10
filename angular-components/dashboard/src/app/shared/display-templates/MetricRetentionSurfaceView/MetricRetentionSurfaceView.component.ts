import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ApiConfigService } from 'src/app/core/ApiConfig.service';
import { ICanBeHiddenFromDisplay } from '../../interfaces/ICanBeHiddenFromDisplay';
import { ApplianceApiService } from '../../services/appliance-api.service';

import {

  OperatorPageModel,
  RetentionEntitiesService,
  StorageAccountDTO,
  TableStorageEntityRetentionPolicy,
  TableStorageEntityRetentionPolicyEnforcementResult,
} from '@wizardcontroller/sac-appliance-lib';
import { ReplaySubject } from 'rxjs';
import { GlobalOhNoConstants } from '../../GlobalOhNoConstants';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { ApplianceJobOutput, MetricsRetentionSurfaceItemEntity } from '@wizardcontroller/sac-appliance-lib/sac-appliance-api';

@Component({
  templateUrl: './MetricRetentionSurfaceView.component.html',
  styleUrls: ['./MetricRetentionSurfaceView.component.css'],
})
@AutoUnsubscribe()
export class MetricRetentionSurfaceViewComponent
  implements OnInit, OnDestroy, ICanBeHiddenFromDisplay
{
  private acctSubject = new ReplaySubject<StorageAccountDTO>();
  selectedAccountChanges$ = this.acctSubject.asObservable();

  private entityEnforcementResultSource =
    new ReplaySubject<TableStorageEntityRetentionPolicyEnforcementResult>();
  entityEnforcementResultChanges$ =
    this.entityEnforcementResultSource.asObservable();

  private entityRetentionPolicySource =
    new ReplaySubject<TableStorageEntityRetentionPolicy>();
  entityRetentionPolicyChanges$ =
    this.entityRetentionPolicySource.asObservable();

  selectedStorageAccount!: StorageAccountDTO;
  operatorPageModel!: OperatorPageModel;

  currentJobOutput: ApplianceJobOutput[] | undefined;
  private currentJobOutputSource = new ReplaySubject<ApplianceJobOutput[]>();
  currentJobOutputChanges$ = this.currentJobOutputSource.asObservable();

  metricsRetentionSurfaceEntities: MetricsRetentionSurfaceItemEntity[] | undefined;
  private metricsRetentionSurfaceEntitiesSource = new ReplaySubject<MetricsRetentionSurfaceItemEntity[]>();
  metricsRetentionSurfaceEntityChanges$ = this.metricsRetentionSurfaceEntitiesSource.asObservable();
  constructor(
    private apiConfigSvc: ApiConfigService,
    private applianceAPiSvc: ApplianceApiService,
    private route: ActivatedRoute
  ) {
    this.isShow = false;
  }
  ngOnDestroy(): void {

  }

  isShow: boolean;
  toggleDisplay(): void {}

  ngOnInit() {
    this.applianceAPiSvc.applianceSessionContextChanges$.subscribe(
      (applianceContext) => {}
    );

    this.apiConfigSvc.operatorPageModelChanges$.subscribe((data) => {
      this.operatorPageModel = data;

      var tenantId = data.tenantid as string;
      var oid = data.oid as string;

      console.log('tenantid is ' + tenantId);
      console.log('oid is ' + oid);

      this.applianceAPiSvc.storageAccountChanges$.subscribe(
        (storageAccounts) => {
          this.applianceAPiSvc.selectedStorageAccount$.subscribe((data) => {
            console.log('metric tool has selected storage account ');
            var acct = data.pop();
            this.selectedStorageAccount = acct as StorageAccountDTO;

            // configure retention service with selected storage account
            this.applianceAPiSvc.entityService.defaultHeaders.append(
              GlobalOhNoConstants.HEADER_CURRENT_STORAGE_ACCOUNT,
              this.selectedStorageAccount.id as string);

              this.applianceAPiSvc.entityService.getCurrentJobOutput(tenantId,oid,
                this.selectedStorageAccount.subscriptionId as string,
                this.selectedStorageAccount.id as string).subscribe((jobOutput: ApplianceJobOutput[]) =>{

                  console.log("current job output is available for storage account id " + this.selectedStorageAccount.id);
                  this.currentJobOutput = jobOutput;
                  this.currentJobOutputSource.next(jobOutput);

                  var newEntities : MetricsRetentionSurfaceItemEntity[];
                  // emit metricsRetentionEntities
                  this.currentJobOutput.forEach((entity) => {
                    entity.retentionPolicyJobs?.forEach((job) =>{
                      job.tableStorageRetentionPolicy?.
                          tableStorageTableRetentionPolicy?.
                            metricRetentionSurface?.
                              metricsRetentionSurfaceItemEntities?.forEach((metricEntity) =>{
                                console.log("metric retention entity : " + metricEntity.itemDescription);
                                newEntities.push(metricEntity);
                                this.metricsRetentionSurfaceEntitiesSource.next(newEntities);
                              });
                    });
                  })

                  this.entityEnforcementResultSource.next()
              });
          });
        }
      );
    });
  }
}
