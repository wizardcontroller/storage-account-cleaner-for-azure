import { ApplianceApiService } from './services/appliance-api.service';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedComponent } from './shared.component';
import { SubscriptionsViewComponent } from './display-templates/SubscriptionsView/SubscriptionsView.component';
import {TableModule} from 'primeng/table';
@NgModule({
  imports: [
    CommonModule, TableModule
  ],
  exports: [SharedComponent, SubscriptionsViewComponent],
  declarations: [SharedComponent,SubscriptionsViewComponent],
  providers: [ApplianceApiService]
})
export class SharedModule { }
