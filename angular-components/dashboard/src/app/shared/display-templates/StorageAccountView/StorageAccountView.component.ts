import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { StorageAccountDTO } from '@wizardcontroller/sac-appliance-lib';
import { BehaviorSubject, combineLatest, ReplaySubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiConfigService } from 'src/app/core/ApiConfig.service';
import { ICanBeHiddenFromDisplay } from '../../interfaces/ICanBeHiddenFromDisplay';
import { ApplianceApiService } from '../../services/appliance-api.service';
import { ButtonModule } from 'primeng/button';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import {
  MatButtonToggleChange,
  MatButtonToggleGroup,
} from '@angular/material/button-toggle';
@Component({
  selector: 'app-StorageAccountView',
  templateUrl: './StorageAccountView.component.html',
  styleUrls: ['./StorageAccountView.component.css'],
})
@AutoUnsubscribe()
export class StorageAccountViewComponent
  implements OnInit, OnDestroy, ICanBeHiddenFromDisplay
{
  @ViewChild('toolSelector') toolSelector!: MatButtonToggleGroup;

  cols!: any[];
  storageAccounts!: StorageAccountDTO[] | undefined | null;
  private storageAccountSource = new ReplaySubject<
    StorageAccountDTO[] | undefined | null
  >();
  storageAccounts$ = this.storageAccountSource.asObservable();

  selectedStorageAccount!: StorageAccountDTO;
  selectedRadio! : string;

  private toolSelectionSource = new ReplaySubject<string>();
  toolSelectionChanges$ = this.toolSelectionSource.asObservable();
  public selectedTool: string = 'DiagnosticsRetention';

  constructor(
    private apiConfigSvc: ApiConfigService,
    private applianceAPiSvc: ApplianceApiService,
    private route: ActivatedRoute
  ) {
    this.isShow = false;

    // preselect a tool
    this.toolSelectionSource.next(this.selectedTool);
  }
  ngOnDestroy(): void {
    // nothing yet
  }
  isShow: boolean;
  toggleDisplay(): void {}

  ngOnInit() {
    console.log('StorageAccountViewComponent is initializing');
    this.applianceAPiSvc.storageAccountChanges$.subscribe((data) => {
      console.log('storage accounts available ' + JSON.stringify(data));
    });

    this.applianceAPiSvc.applianceSessionContextChanges$.subscribe((data) => {
      console.log('ApplianceSessionContext available');
      console.log(
        'selected storage account id ' +
          JSON.stringify(data.selectedStorageAccounts)
      );
      this.storageAccounts = data.selectedStorageAccounts;

      var acts = data.selectedStorageAccounts as any;
      console.log('acts ' + JSON.stringify(acts));
      this.storageAccountSource.next(this.storageAccounts);
    });

    this.ensureStorageAccountsPTable();

    this.applianceAPiSvc.ensurePageModelSubject();
  }

  selectStorageAccount(storageAccount: StorageAccountDTO) {
    // this.messageService.add({severity:'info', summary:'Product Selected', detail: product.name});
    console.log('account selected : ' + this.selectedRadio);
    this.selectedStorageAccount = storageAccount;
    this.applianceAPiSvc.selectedStorageAccountSource.next(
      storageAccount.id as string
    );
  }

  toolChanged(e: MatButtonToggleChange) {
    console.log('tool selected ' + this.selectedTool);
    this.toolSelectionSource.next(e.value);
  }

  private ensureStorageAccountsPTable() {
    this.cols = [
      { field: 'name', header: 'Name' },
      { field: 'id', header: 'Account ID' },
      { field: 'tenantId', header: 'Tenant Id' },
    ];
  }
}
