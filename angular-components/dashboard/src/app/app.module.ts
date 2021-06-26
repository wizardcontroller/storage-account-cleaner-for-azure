import { SharedModule } from './shared/shared.module';

import { CoreModule } from './core/core.module';
import { HttpClientModule } from '@angular/common/http';

import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BASE_PATH } from '@wizardcontroller/sac-appliance-lib';
import { environment } from '../environments/environment';
import {CardModule} from 'primeng/card';
import {ButtonModule} from 'primeng/button';
import {ApiModule} from '@wizardcontroller/sac-appliance-lib'
import { OperatorPageModel } from '@wizardcontroller/sac-appliance-lib/sac-appliance-api/index';
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
    HttpClientModule
  ],
  providers: [{ provide: BASE_PATH, useValue: environment.API_BASE_PATH }],
  bootstrap: [AppComponent]
})
export class AppModule { }
