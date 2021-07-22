import { ApiConfigService } from './ApiConfig.service';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CoreComponent } from './core.component';
import { ToastModule } from 'primeng/toast'
import { RippleModule } from 'primeng/ripple';
import { MessageService } from 'primeng/api';
@NgModule({
  imports: [
    CommonModule,
    ToastModule,
    RippleModule,
    ToastModule
  ],
  exports : [CoreComponent],
  declarations: [CoreComponent],
  providers :[ApiConfigService, MessageService]
})
export class CoreModule { }
