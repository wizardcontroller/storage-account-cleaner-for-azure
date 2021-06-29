import { ApplianceApiService } from './services/appliance-api.service';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedComponent } from './shared.component';
import { SubscriptionsViewComponent } from './display-templates/SubscriptionsView/SubscriptionsView.component';
import {TableModule} from 'primeng/table';
import { BrowserModule } from '@angular/platform-browser';
import { ApplianceContextViewComponent } from './display-templates/ApplianceContextView/ApplianceContextView.component';
import { StorageAccountViewComponent } from './display-templates/StorageAccountView/StorageAccountView.component';
import { RetentionPolicyEditorComponent } from './forms/RetentionPolicyEditor/RetentionPolicyEditor.component';
import { DiagnosticsRetentionSurfaceViewComponent } from './display-templates/DiagnosticsRetentionSurfaceView/DiagnosticsRetentionSurfaceView.component';
import { ApplianceContextService } from './display-templates/services/ApplianceContext.service';
import { HomeGrownLoggingService } from './display-templates/services/HomeGrownLogging.service';
import { LoggingConfigurationService } from './display-templates/services/LoggingConfiguration.service';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { CardModule } from 'primeng/card';
@NgModule({
  imports: [
    CommonModule, TableModule, BrowserModule, CardModule, HttpClientModule
  ],
  exports: [
    SharedComponent,
    SubscriptionsViewComponent,
    ApplianceContextViewComponent,
    StorageAccountViewComponent,
    RetentionPolicyEditorComponent,
    DiagnosticsRetentionSurfaceViewComponent
  ],
  declarations: [
    SharedComponent,
    SubscriptionsViewComponent,
    ApplianceContextViewComponent,
    StorageAccountViewComponent,
    RetentionPolicyEditorComponent,
    DiagnosticsRetentionSurfaceViewComponent
  ],
  providers: [
    LoggingConfigurationService,
    HomeGrownLoggingService,
    ApplianceContextService,
    ApplianceApiService]
})
export class SharedModule { }
