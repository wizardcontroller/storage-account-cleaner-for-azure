import { SharedModule } from './shared/shared.module';

import { CoreModule } from './core/core.module';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';

import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BASE_PATH } from '@wizardcontroller/sac-appliance-lib/';
import { environment } from '../environments/environment';
import {CardModule} from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { ApiModule } from '@wizardcontroller/sac-appliance-lib/'
import { OperatorPageModel } from '@wizardcontroller/sac-appliance-lib/';
import { TableModule } from 'primeng/table';
import { AuthHeaderInterceptor } from './interceptors/auth-header.interceptor';
import { ApiConfigService } from './core/ApiConfig.service';
import { MockOperatorPageModelInterceptor } from './interceptors/mock-operator-page-model.interceptor';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    ApiModule,
    CoreModule,
    SharedModule,
    BrowserModule,
    AppRoutingModule,
    CardModule,
    ButtonModule,
    TableModule,
    HttpClientModule,
    BrowserAnimationsModule,
    MatButtonToggleModule
  ],
  providers: [
  {
    provide: BASE_PATH,
    useValue: environment.API_BASE_PATH
  },
  {
    provide: HTTP_INTERCEPTORS,
    useClass: AuthHeaderInterceptor,
    multi: true,
    deps: [ApiConfigService]
  },
  {
    provide: HTTP_INTERCEPTORS,
    useClass: MockOperatorPageModelInterceptor,
    multi: true,
    deps: []
  }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
