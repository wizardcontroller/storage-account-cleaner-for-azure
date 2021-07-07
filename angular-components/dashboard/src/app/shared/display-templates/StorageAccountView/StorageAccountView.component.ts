import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { StorageAccountDTO } from '@wizardcontroller/sac-appliance-lib';
import { ReplaySubject } from 'rxjs';
import { ICanBeHiddenFromDisplay } from '../../interfaces/ICanBeHiddenFromDisplay';

@Component({
  selector: 'lib-StorageAccountView',
  templateUrl: './StorageAccountView.component.html',
  styleUrls: ['./StorageAccountView.component.css']
})
export class StorageAccountViewComponent implements OnInit, ICanBeHiddenFromDisplay {
  cols!: any[];
  private storageAccountSource = new ReplaySubject<StorageAccountDTO[]>();
  storageAccounts$ = this.storageAccountSource.asObservable();

  constructor( private route: ActivatedRoute) {
    this.isShow = false;
   }
  isShow: boolean;
  toggleDisplay(): void {

  }

  ngOnInit() {
    this.cols = [
      { field: 'isSelected', header: 'Is Selected' },
      { field: 'Name', header: 'Name' },
      { field: 'id', header: 'Account ID' },
      { field: 'tenantId', header: 'Tenant Id' }

    ];
  }

}
