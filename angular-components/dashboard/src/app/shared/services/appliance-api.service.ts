import { Injectable } from '@angular/core';
import {
  ConfigService,
  OperatorPageModel,
  RetentionEntitiesService,
} from '@wizardcontroller/sac-appliance-lib';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { Subject } from 'rxjs';
import { ApiConfigService } from 'src/app/core/ApiConfig.service';

@Injectable({
  providedIn: 'root',
})

@AutoUnsubscribe()
export class ApplianceApiService {
  operatorPageModel!: OperatorPageModel | null;
  baseUri: string | undefined;



  constructor(
    private apiConfigService : ApiConfigService,
    private configService: ConfigService,
    private entityService: RetentionEntitiesService
  ) {

    this.apiConfigService.operatorPageModelChanges$.subscribe(data => {
      console.log("appliance api service has operator page model");
      this.baseUri = data?.applianceUrl?.toString();
      return this.operatorPageModel = data;
    });
  }


}
