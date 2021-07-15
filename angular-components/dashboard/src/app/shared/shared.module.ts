import { ApplianceApiService } from './services/appliance-api.service';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedComponent } from './shared.component';
import { SubscriptionsViewComponent } from './display-templates/SubscriptionsView/SubscriptionsView.component';
import { TableModule } from 'primeng/table';
import { BrowserModule } from '@angular/platform-browser';
import { ApplianceContextViewComponent } from './display-templates/ApplianceContextView/ApplianceContextView.component';
import { StorageAccountViewComponent } from './display-templates/StorageAccountView/StorageAccountView.component';
import { RetentionPolicyEditorComponent } from './forms/RetentionPolicyEditor/RetentionPolicyEditor.component';
import { DiagnosticsRetentionSurfaceViewComponent } from './display-templates/DiagnosticsRetentionSurfaceView/DiagnosticsRetentionSurfaceView.component';
import { ApplianceContextService } from './display-templates/services/ApplianceContext.service';
import { HomeGrownLoggingService } from './display-templates/services/HomeGrownLogging.service';
import { LoggingConfigurationService } from './display-templates/services/LoggingConfiguration.service';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { CardModule, Card } from 'primeng/card';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatSidenavModule } from '@angular/material/sidenav'
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ButtonModule } from 'primeng/button';
import { RouterModule, Routes } from '@angular/router';
import { MatExpansionModule } from '@angular/material/expansion';
import {DataViewModule } from 'primeng/dataview'
import { MatAccordion } from '@angular/material/expansion';
import {MatMenuModule, MatMenu} from '@angular/material/menu';
import {MatToolbarModule, MatToolbar} from '@angular/material/toolbar';
import {RetentionEntitiesService} from '@wizardcontroller/sac-appliance-lib'
import { MetricRetentionSurfaceViewComponent } from './display-templates/MetricRetentionSurfaceView/MetricRetentionSurfaceView.component';
@NgModule({
  imports: [
    CommonModule,
    TableModule,
    BrowserModule,
    BrowserAnimationsModule,
    CardModule,
    DataViewModule,
    HttpClientModule,
    MatButtonToggleModule,
    MatSidenavModule,
    BrowserAnimationsModule,
    ButtonModule,
    TableModule,
    MatExpansionModule,
    RouterModule
  ],
  exports: [
    SharedComponent,
    SubscriptionsViewComponent,
    ApplianceContextViewComponent,
    StorageAccountViewComponent,
    RetentionPolicyEditorComponent,
    DiagnosticsRetentionSurfaceViewComponent,
    MetricRetentionSurfaceViewComponent
  ],
  declarations: [
    SharedComponent,
    SubscriptionsViewComponent,
    ApplianceContextViewComponent,
    StorageAccountViewComponent,
    RetentionPolicyEditorComponent,
    DiagnosticsRetentionSurfaceViewComponent,
    MetricRetentionSurfaceViewComponent
  ],
  providers: [
    LoggingConfigurationService,
    HomeGrownLoggingService,
    ApplianceContextService,
    ApplianceApiService,
  ],
})
export class SharedModule {}
