import { ApplianceApiService } from './../../services/appliance-api.service';
import { Component, OnInit } from '@angular/core';
import { OperatorPageModel } from 'index';
import { Observable, ReplaySubject } from 'rxjs';
import { ApiConfigService } from 'src/app/core/ApiConfig.service';
import { TableModule } from 'primeng/table';
import {
  SubscriptionDTO,
  SubscriptionPoliciesDTO,
} from '@wizardcontroller/sac-appliance-lib/sac-appliance-api/index';
@Component({
  selector: 'app-SubscriptionsView',
  templateUrl: './SubscriptionsView.component.html',
  styleUrls: ['./SubscriptionsView.component.css'],
})
export class SubscriptionsViewComponent implements OnInit {

  cols!: any[];

  // operator page model change notification support
  private currentPageModelSource = new ReplaySubject<OperatorPageModel>();
  operatorPageModelChanges$ = this.currentPageModelSource.asObservable();

  private subscriptionSource = new ReplaySubject<SubscriptionDTO[]>();
  subscriptions$ = this.subscriptionSource.asObservable();

  constructor(
    private apiConfigSvc: ApiConfigService,
    private applianceAPiSvc: ApplianceApiService
  ) {
    // this.operatorPageModel$ = this.apiConfigSvc.operatorPageModelChanges$;
    // push operator page model changes
    this.apiConfigSvc.operatorPageModelChanges$.subscribe(data =>
      
      {
        this.currentPageModelSource.next(data);
      });
    // push current subscriptions 
    this.apiConfigSvc.operatorPageModelChanges$.subscribe(data => {
      var subscriptions = data.subscriptions as SubscriptionDTO[] | undefined;
      this.subscriptionSource.next(subscriptions);
  });

}


  ngOnDestroy(): void {}

  ngOnInit(): void {
    this.cols = [
      { field: 'isSelected', header: 'Is Selected' },
      { field: 'displayName', header: 'Display Name' },
      { field: 'tenantId', header: 'Tenant Id' },
    ];

    console.log('SubscriptionsViewComponent is initializing page model');
  }
}
