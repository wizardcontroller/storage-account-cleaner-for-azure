import { PageModelService } from './page-model.service';

import { ApiModule} from '../../../node_modules/@wizardcontroller/storage-account-cleaner-lib/src/public-api';
import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
@NgModule({
  declarations: [],
  imports: [
    CommonModule, HttpClientModule, ApiModule
  ],
  providers: [
    PageModelService
  ]
})
export class SharedModule { }
