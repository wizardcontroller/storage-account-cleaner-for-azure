import { ApplianceApiService } from './services/appliance-api.service';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedComponent } from './shared.component';

@NgModule({
  imports: [
    CommonModule
  ],
  exports: [SharedComponent],
  declarations: [SharedComponent],
  providers: [ApplianceApiService]
})
export class SharedModule { }
