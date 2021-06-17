import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CoreComponent } from './core.component';

@NgModule({
  imports: [
    CommonModule
  ],
  exports: [CoreComponent],
  declarations: [CoreComponent]
})
export class CoreModule { }
