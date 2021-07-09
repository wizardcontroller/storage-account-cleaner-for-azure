import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ApiConfigService } from 'src/app/core/ApiConfig.service';
import { ICanBeHiddenFromDisplay } from '../../interfaces/ICanBeHiddenFromDisplay';
import { ApplianceApiService } from '../../services/appliance-api.service';
import { CommonModule } from '@angular/common';
import { RetentionEntitiesService, StorageAccountDTO, TableStorageEntityRetentionPolicy, TableStorageEntityRetentionPolicyEnforcementResult } from '@wizardcontroller/sac-appliance-lib';
import { ReplaySubject } from 'rxjs';
import { GlobalOhNoConstants } from '../../GlobalOhNoConstants';
@Component({
  selector: 'lib-MetricRetentionSurfaceView',
  templateUrl: './MetricRetentionSurfaceView.component.html',
  styleUrls: ['./MetricRetentionSurfaceView.component.css']
})
export class MetricRetentionSurfaceViewComponent implements OnInit, ICanBeHiddenFromDisplay {

  private acctSubject = new ReplaySubject<StorageAccountDTO>();
  selectedAccountChanges$ = this.acctSubject.asObservable();

  private entityEnforcementResultSource = new ReplaySubject<TableStorageEntityRetentionPolicyEnforcementResult>();
  entityEnforcementResultChanges$ = this.entityEnforcementResultSource.asObservable();

  private entityRetentionPolicySource = new ReplaySubject<TableStorageEntityRetentionPolicy>();
  entityRetentionPolicyChanges$ = this.entityRetentionPolicySource.asObservable();

  selectedStorageAccount! : StorageAccountDTO;
  constructor(    private apiConfigSvc: ApiConfigService,
    private applianceAPiSvc: ApplianceApiService,
    private entitySvc : RetentionEntitiesService,
    private route: ActivatedRoute) {
    this.isShow = false;
   }

  isShow: boolean;
  toggleDisplay(): void {

  }

  ngOnInit() {

    this.applianceAPiSvc.currentJobOutputChanges$.subscribe(data => {
      // potentially obsolete
    });

    this.applianceAPiSvc.storageAccountChanges$.subscribe(storageAccounts => {

      this.applianceAPiSvc.selectedStorageAccount$.subscribe(data =>{
        console.log("metric tool has selected storage account " );
        var acct = data.pop();
        this.selectedStorageAccount = acct as StorageAccountDTO;

        // configure retention service with selected storage account
        this.entitySvc.defaultHeaders.append(GlobalOhNoConstants.HEADER_CURRENT_STORAGE_ACCOUNT,
          this.selectedStorageAccount.id as string);

        // get the retention entitites for the selected storage account

      });
    });

  }

}
