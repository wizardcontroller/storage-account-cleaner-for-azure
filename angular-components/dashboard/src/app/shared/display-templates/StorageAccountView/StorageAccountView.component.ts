import { JsonpClientBackend } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { StorageAccountDTO } from '@wizardcontroller/sac-appliance-lib';
import { ReplaySubject } from 'rxjs';
import { ApiConfigService } from 'src/app/core/ApiConfig.service';
import { ICanBeHiddenFromDisplay } from '../../interfaces/ICanBeHiddenFromDisplay';
import { ApplianceApiService } from '../../services/appliance-api.service';

@Component({
  selector: 'lib-StorageAccountView',
  templateUrl: './StorageAccountView.component.html',
  styleUrls: ['./StorageAccountView.component.css']
})
export class StorageAccountViewComponent implements OnInit, ICanBeHiddenFromDisplay {
  cols!: any[];
  storageAccounts! : StorageAccountDTO[]  | undefined | null;
  private storageAccountSource = new ReplaySubject<StorageAccountDTO[]  | undefined | null>();
  storageAccounts$ = this.storageAccountSource.asObservable();

  constructor(     private apiConfigSvc: ApiConfigService,
    private applianceAPiSvc: ApplianceApiService,private route: ActivatedRoute) {
    this.isShow = false;
   }
  isShow: boolean;
  toggleDisplay(): void {

  }

  ngOnInit() {

    console.log("StorageAccountViewComponent is initializing");
    this.applianceAPiSvc.storageAccountChanges$.subscribe(data => {
      console.log("storage accounts available " + JSON.stringify(data));
    });

    this.applianceAPiSvc.applianceSessionContextChanges$.subscribe(data =>{

        console.log("ApplianceSessionContext available" );
        console.log("selected storage account id " + JSON.stringify(data.selectedStorageAccounts));
        this.storageAccounts = data.selectedStorageAccounts;

        var acts = data.selectedStorageAccounts as any;
        console.log("acts " + JSON.stringify(acts));
        this.storageAccountSource.next(this.storageAccounts);
    });

    this.ensureStorageAccountsPTable();

    this.applianceAPiSvc.ensurePageModelSubject();

  }


  private ensureStorageAccountsPTable() {
    this.cols = [
      { field: 'isSelected', header: 'Is Selected' },
      { field: 'Name', header: 'Name' },
      { field: 'id', header: 'Account ID' },
      { field: 'tenantId', header: 'Tenant Id' }
    ];
  }
}
