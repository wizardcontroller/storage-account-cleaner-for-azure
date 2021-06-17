import { SharedComponent } from './../../../orig/storage-account-cleaner/src/app/shared/shared.component';
import { CoreComponent } from './core/core.component';
import { CoreModule } from './core/core.module';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { SharedModule } from './shared/shared.module';
import { AppComponent } from './app.component';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule, CoreModule, SharedModule
  ],
  providers: [],
  bootstrap: [AppComponent, CoreComponent, SharedComponent]
})
export class AppModule { }
