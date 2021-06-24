import { HttpClientModule } from '@angular/common/http';

import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ApiModule } from './sac-appliance-api/api.module';



@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    ApiModule,
    BrowserModule,
    AppRoutingModule,
    HttpClientModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
