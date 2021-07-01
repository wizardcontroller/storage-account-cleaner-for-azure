import { Injectable } from '@angular/core';
import {
  ApplianceSessionContext,
  ConfigService,
  OperatorPageModel,
  RetentionEntitiesService,
} from '@wizardcontroller/sac-appliance-lib';
import { ReplaySubject } from 'rxjs';
import { ApiConfigService } from 'src/app/core/ApiConfig.service';
import { ApplianceContextServiceBase } from './ApplianceContextServiceBase';

@Injectable({
  providedIn: 'root'
})
export class ApplianceContextService extends ApplianceContextServiceBase {


constructor(
  apiConfigSvc : ApiConfigService,
  retentionSvc : RetentionEntitiesService
  ) {
  super(apiConfigSvc, retentionSvc);

  this.subscribeToOperatorPageModel();


}

  getApplianceContext(){

  }
}
