import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { RetentionEntitiesService } from '@wizardcontroller/sac-appliance-lib';
import { combineLatest } from 'rxjs';
import { ApiConfigService } from 'src/app/core/ApiConfig.service';
import { ApplianceApiService } from '../../services/appliance-api.service';

@Component({
  selector: 'app-appliance-job-log-view',
  templateUrl: './appliance-job-log-view.component.html',
  styleUrls: ['./appliance-job-log-view.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ApplianceJobLogViewComponent implements OnInit {
  applianceLogChangesTimer$ = timer(0,
    this.applianceLogChangesTimerPollingInterval);
  applianceLogChanges$ = combineLatest(
    this.applianceApiSvc.operatorPageModelChanges$,
    this.applianceLogChangesTimer$
  )
  .pipe(
    map(([elapsedEvent, pageModel]) => {

      var tenantid = pageModel.tenantid as string;

      var subscriptionId = pageModel.subscriptionId as string;

      var oid = pageModel.oid as string;

    })
  );
  applianceLogChangesTimerPollingInterval: any;
  constructor(private apiConfigSvc: ApiConfigService,
    private retentionEntitiesSvc: RetentionEntitiesService,
    private applianceApiSvc: ApplianceApiService) { }

  ngOnInit(): void {
  }

}
