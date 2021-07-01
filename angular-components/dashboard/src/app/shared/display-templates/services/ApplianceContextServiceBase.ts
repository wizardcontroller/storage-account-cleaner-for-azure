import { ApplianceSessionContext, OperatorPageModel, RetentionEntitiesService } from "@wizardcontroller/sac-appliance-lib";
import { ReplaySubject } from "rxjs";
import { ApiConfigService } from "src/app/core/ApiConfig.service";

export class ApplianceContextServiceBase {
  constructor(  protected apiConfigSvc : ApiConfigService,
    protected retentionSvc : RetentionEntitiesService){

  }

  pageModel!: OperatorPageModel;
  applianceContext! : ApplianceSessionContext
  private currentApplianceContextSource = new ReplaySubject<ApplianceSessionContext>();
  currentApplianceContextChanges$ = this.currentApplianceContextSource.asObservable();

  protected subscribeToApplianceContext(tenantId : string, oid: string, subscriptionId: string) : void{
    this.retentionSvc.getApplianceSessionContext(tenantId, oid, subscriptionId).subscribe(data => {
      this.applianceContext = data;
      this.configureApplianceAuth();
    }, error => {
      console.log("error getting appliance sesssion context " + JSON.stringify(error));
    });
  }

  protected subscribeToOperatorPageModel() : void{
    this.apiConfigSvc.operatorPageModelChanges$.subscribe(data => {
      this.pageModel = data;
      this.configureApplianceAuth();

      // new page model - update dependencies

    });
  }

  /**
   * apply the requierd tokens to the appliance service
   */
  protected configureApplianceAuth() : void{
    this.retentionSvc.configuration.basePath =
      this.pageModel?.applianceUrl?.toString();
  }

}
