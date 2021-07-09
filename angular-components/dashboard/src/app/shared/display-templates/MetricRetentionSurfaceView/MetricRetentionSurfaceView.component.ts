import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ApiConfigService } from 'src/app/core/ApiConfig.service';
import { ICanBeHiddenFromDisplay } from '../../interfaces/ICanBeHiddenFromDisplay';
import { ApplianceApiService } from '../../services/appliance-api.service';

@Component({
  selector: 'lib-MetricRetentionSurfaceView',
  templateUrl: './MetricRetentionSurfaceView.component.html',
  styleUrls: ['./MetricRetentionSurfaceView.component.css']
})
export class MetricRetentionSurfaceViewComponent implements OnInit, ICanBeHiddenFromDisplay {

  selectedStorageAccount$ = this.applianceAPiSvc.selectedStorageAccount$;

  constructor(    private apiConfigSvc: ApiConfigService,
    private applianceAPiSvc: ApplianceApiService,private route: ActivatedRoute) {
    this.isShow = false;
   }

  isShow: boolean;
  toggleDisplay(): void {

  }

  ngOnInit() {
    this.applianceAPiSvc.storageAccountChanges$.subscribe(storageAccounts => {

      this.applianceAPiSvc.selectedStorageAccount$.subscribe(data =>{
        console.log("metric tool has selected storage account " );
      });
    });

  }

}
