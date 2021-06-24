import { ApiConfigService } from './ApiConfig.service';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CoreComponent } from './core.component';

@NgModule({
  imports: [
    CommonModule
  ],
  exports : [CoreComponent],
  declarations: [CoreComponent],
  providers :[ApiConfigService]
})
export class CoreModule { }
