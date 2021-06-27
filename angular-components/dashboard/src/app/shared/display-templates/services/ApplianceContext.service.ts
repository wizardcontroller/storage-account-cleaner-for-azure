import { Injectable } from '@angular/core';
import {
  ConfigService,
  OperatorPageModel,
  RetentionEntitiesService,
} from '@wizardcontroller/sac-appliance-lib';
import { ApiConfigService } from 'src/app/core/ApiConfig.service';
import { ApplianceContextServiceBase } from './ApplianceContextServiceBase';

@Injectable({
  providedIn: 'root'
})
export class ApplianceContextService extends ApplianceContextServiceBase {
  pageModel!: OperatorPageModel;

constructor(
  private apiConfigSvc : ApiConfigService,
  private retentionSvc : RetentionEntitiesService
  ) {
  super();

  this.subscribeToOperatorPageModel();

}

  protected subscribeToOperatorPageModel() : void{
    this.apiConfigSvc.operatorPageModelChanges$.subscribe(data => {
      this.pageModel = data;
      this.configureApplianceAuth();
    });
  }

  /**
   * apply the requierd tokens to the appliance service
   */
  protected configureApplianceAuth() : void{
    this.retentionSvc.configuration.basePath =
      this.pageModel?.applianceUrl?.toString();
  }

  getApplianceContext(){

  }
}
