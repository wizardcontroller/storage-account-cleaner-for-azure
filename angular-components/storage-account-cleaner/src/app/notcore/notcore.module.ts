import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';

// as per Pluralsight Angular Architecture and Best Practicves

import { NgModule, Optional, SkipSelf } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotcoreComponent } from './notcore.component';
import { EnsureModuleLoadedOnceGuard } from './EnsureModuleLoadedOnceGuard';
import { AuthInterceptor } from './interceptors/auth.interceptor';

@NgModule({
  imports: [
    CommonModule, HttpClientModule
  ],
  declarations: [NotcoreComponent],
  providers:[
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true  }

  ]
})

export class NotcoreModule extends EnsureModuleLoadedOnceGuard {
  constructor(@Optional() @SkipSelf() parentModule: NotcoreModule){
    super(parentModule);
  }
 }
