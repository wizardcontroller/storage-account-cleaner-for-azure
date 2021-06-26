import { Component, OnInit } from '@angular/core';
import { OperatorPageModel } from 'index';
import { ReplaySubject } from 'rxjs';
import { ApiConfigService } from 'src/app/core/ApiConfig.service';
import {TableModule} from 'primeng/table';
import { SubscriptionDTO} from '@wizardcontroller/sac-appliance-lib/sac-appliance-api/index';
@Component({
  selector: 'app-SubscriptionsView',
  templateUrl: './SubscriptionsView.component.html',
  styleUrls: ['./SubscriptionsView.component.css']
})
export class SubscriptionsViewComponent implements OnInit {
  operatorPageModel!: OperatorPageModel;
  subscriptions! : any[];
  cols!: any[];

  // operator page model change notification support
  private currentPageModelSource = new ReplaySubject<OperatorPageModel>();
  operatorPageModelChanges$ = this.currentPageModelSource.asObservable();

  constructor(private apiConfigSvc : ApiConfigService) {

    this.apiConfigSvc.operatorPageModelChanges$.subscribe(data => {
      console.log("SubscriptionsViewComponent has operator page model");
      this.subscriptions = this.operatorPageModel.subscriptions as any[];

      return this.operatorPageModel = data;
    });
  }

  ngOnDestroy(): void {

  }

   ngOnInit(): void {

    this.cols = [
      { field: 'isSelected', header: 'Is Selected' },
      { field: 'displayName', header: 'Display Name' },
      { field: 'tenantId', header: 'Tenant Id' }
  ];

    console.log("SubscriptionsViewComponent is initializing page model");
     this.apiConfigSvc.getOperatorPageModel();

    // this.operatorPageModel = this.apiConfigSvc.operatorPageModel;
    // this.baseUri = this.operatorPageModel?.applianceUrl?.toString();

  }


}
