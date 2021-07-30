import { Injectable, OnDestroy } from '@angular/core';
import {
  ApplianceSessionContext,
  ConfigService,
  OperatorPageModel,
  RetentionEntitiesService,
} from '@wizardcontroller/sac-appliance-lib';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { ReplaySubject } from 'rxjs';
import { ApiConfigService } from 'src/app/core/ApiConfig.service';
import { ApplianceContextServiceBase } from './ApplianceContextServiceBase';

@Injectable({
  providedIn: 'root'
})
@AutoUnsubscribe()
export class ApplianceContextService  extends ApplianceContextServiceBase implements OnDestroy{


constructor(
  apiConfigSvc : ApiConfigService,
  retentionSvc : RetentionEntitiesService
  ) {
  super(apiConfigSvc, retentionSvc);

  this.subscribeToOperatorPageModel();


}
  ngOnDestroy(): void {

  }

  initApplianceContextSubject(){

  }

  initWorkflowCheckpointSubject(){

  }
}
