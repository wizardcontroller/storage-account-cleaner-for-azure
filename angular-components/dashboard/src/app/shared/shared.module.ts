import { HttpClientModule } from '@angular/common/http';
import { Operator } from 'rxjs';
import { PageModelService } from './page-model.service';
import { OperatorComponent } from './../operator/operator.component';

import { ApiModule} from '../../../node_modules/@wizardcontroller/storage-account-cleaner-lib/src/public-api';

import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { environment } from '../../environments/environment';
import { BASE_PATH } from '@wizardcontroller/storage-account-cleaner-lib/src/appliance-webapi/variables';

@NgModule({
  declarations: [
    OperatorComponent
  ],
  exports: [OperatorComponent],
  imports: [
    CommonModule, HttpClientModule
  ],
  providers: [
      PageModelService
  ]
})
export class SharedModule { }
