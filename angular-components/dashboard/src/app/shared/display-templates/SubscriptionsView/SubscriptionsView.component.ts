import { ApplianceApiService } from './../../services/appliance-api.service';
import { Component, OnInit } from '@angular/core';
import { OperatorPageModel } from '@wizardcontroller/sac-appliance-lib/';
import { Observable, ReplaySubject } from 'rxjs';
import { ApiConfigService } from '../../../core/ApiConfig.service'
import { TableModule } from 'primeng/table';
import { BrowserModule } from '@angular/platform-browser';
import {
  SubscriptionDTO,
  SubscriptionPoliciesDTO
} from '@wizardcontroller/sac-appliance-lib/sac-appliance-api';
import { ICanBeHiddenFromDisplay } from '../../interfaces/ICanBeHiddenFromDisplay';
import { ActivatedRoute } from '@angular/router';
@Component({
  selector: 'app-SubscriptionsView',
  templateUrl: './SubscriptionsView.component.html',
  styleUrls: ['./SubscriptionsView.component.css']
})

export class SubscriptionsViewComponent implements OnInit, ICanBeHiddenFromDisplay {
  operatorPageModel! : OperatorPageModel;
  cols!: any[];

  // operator page model change notification support
  private currentPageModelSource = new ReplaySubject<OperatorPageModel>();
  operatorPageModelChanges$ = this.currentPageModelSource.asObservable();

  private subscriptionSource = new ReplaySubject<SubscriptionDTO[]>();
  subscriptions$ = this.subscriptionSource.asObservable();

  constructor(
    private apiConfigSvc: ApiConfigService,
    private applianceAPiSvc: ApplianceApiService,
    private route: ActivatedRoute
  ) {
    this.isShow = true;

}
  isShow: boolean;
  toggleDisplay(): void {
    this.isShow = !this.isShow;
  }


private configAuth() : void {

}
  ngOnDestroy(): void {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      var parms = params['id'];

    });
        // this.operatorPageModel$ = this.apiConfigSvc.operatorPageModelChanges$;
    // push operator page model changes
    // push current subscriptions
    this.apiConfigSvc.operatorPageModelChanges$.subscribe(data => {
      console.log("subscriptionviewcomponent has current page model");
      var subscriptions = data.subscriptions as SubscriptionDTO[] | undefined;

      // if only one then is selected
      if(subscriptions?.length == 1){
          subscriptions[0].isSelected = true;
      }

      this.subscriptionSource.next(subscriptions);
      this.operatorPageModel = data;
      this.configAuth();
      console.log("SubscriptionView has easyauth token: " + this.operatorPageModel.easyAuthAccessToken);
      this.currentPageModelSource.next(data);
  });

    this.cols = [
      { field: 'isSelected', header: 'Is Selected' },
      { field: 'displayName', header: 'Display Name' },
      { field: 'tenantId', header: 'Tenant Id' },
    ];


  }
}
