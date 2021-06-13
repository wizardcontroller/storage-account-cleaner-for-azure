
// as per Pluralsight Angular Architecture and Best Practicves

import { NgModule, Optional, SkipSelf } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotcoreComponent } from './notcore.component';
import { EnsureModuleLoadedOnceGuard } from './EnsureModuleLoadedOnceGuard';

@NgModule({
  imports: [
    CommonModule
  ],
  declarations: [NotcoreComponent]
})

export class NotcoreModule extends EnsureModuleLoadedOnceGuard {
  constructor(@Optional() @SkipSelf() parentModule: NotcoreModule){
    super(parentModule);
  }
 }
