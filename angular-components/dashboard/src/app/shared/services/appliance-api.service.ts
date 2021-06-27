import { Injectable } from '@angular/core';
import {
  ConfigService,
  OperatorPageModel,
  RetentionEntitiesService,
} from '@wizardcontroller/sac-appliance-lib';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { config, Subject } from 'rxjs';
import { ApiConfigService } from 'src/app/core/ApiConfig.service';

@Injectable({
  providedIn: 'root',
})
export class ApplianceApiService {
  operatorPageModel!: OperatorPageModel | null;
  baseUri: string | undefined;

  constructor(
    private apiConfigService: ApiConfigService,
    private configService: ConfigService,
    private entityService: RetentionEntitiesService
  ) {
    console.log('ApplianceApiService is starting');

    this.apiConfigService.operatorPageModelChanges$.subscribe((data) => {
      console.log('appliance api service has operator page model');
      try {
        this.baseUri = data?.applianceUrl?.toString();
        this.entityService.configuration.accessToken = this.operatorPageModel
        ?.easyAuthAccessToken as string;
        console.log('appliance api service is configuring access token' + this.entityService.configuration.accessToken);

        this.entityService.configuration.basePath =
          this.operatorPageModel?.applianceUrl?.toString();
        console.log(
          'configured easyauth token ' +
            this.entityService.configuration.accessToken
        );
      } catch (err) {
        console.log('error starting ApplianceApiSvc');
      }

      return (this.operatorPageModel = data);
    });

    console.log('appliance api service done startup');
  }
}
