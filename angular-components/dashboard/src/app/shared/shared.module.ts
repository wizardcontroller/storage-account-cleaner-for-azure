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
import { TimelineModule } from 'primeng/timeline';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatSidenavModule } from '@angular/material/sidenav'
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ButtonModule } from 'primeng/button';
import { RouterModule, Routes } from '@angular/router';
import { MatExpansionModule } from '@angular/material/expansion';
import { DataViewModule } from 'primeng/dataview'
import { MatAccordion } from '@angular/material/expansion';
import { MatMenuModule, MatMenu } from '@angular/material/menu';
import { MatToolbarModule, MatToolbar } from '@angular/material/toolbar';
import { RetentionEntitiesService } from '@wizardcontroller/sac-appliance-lib'
import { MetricRetentionSurfaceViewComponent } from './display-templates/MetricRetentionSurfaceView/MetricRetentionSurfaceView.component';
import { DatesToTimeLineEventsPipePipe } from './pipes/dates-to-time-line-events-pipe.pipe';
import { CommandPaletteComponent } from './display-templates/command-palette/command-palette.component';
import { WorkflowOperationCommandImpl } from './models/WorkflowOperationCommandImpl';
import { ToggleButtonModule } from 'primeng/togglebutton';
import { FormsModule } from '@angular/forms';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner'
import { MatSlideToggleModule } from '@angular/material/slide-toggle'
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
@NgModule({
  imports: [
    CommonModule,
    TableModule,
    BrowserModule,
    FormsModule,
    BrowserAnimationsModule,
    CardModule,
    DataViewModule,
    HttpClientModule,
    MatButtonToggleModule,
    MatSidenavModule,
    BrowserAnimationsModule,
    ButtonModule,
    TableModule,
    TimelineModule,
    MatExpansionModule,
    MatProgressSpinnerModule,
    ToggleButtonModule,
    RouterModule,
    ToastModule
  ],
  exports: [
    SharedComponent,
    SubscriptionsViewComponent,
    ApplianceContextViewComponent,
    StorageAccountViewComponent,
    RetentionPolicyEditorComponent,
    DiagnosticsRetentionSurfaceViewComponent,
    MetricRetentionSurfaceViewComponent,
    DatesToTimeLineEventsPipePipe,
    CommandPaletteComponent
  ],
  declarations: [
    SharedComponent,
    SubscriptionsViewComponent,
    ApplianceContextViewComponent,
    StorageAccountViewComponent,
    RetentionPolicyEditorComponent,
    DiagnosticsRetentionSurfaceViewComponent,
    MetricRetentionSurfaceViewComponent,
    DatesToTimeLineEventsPipePipe,
    CommandPaletteComponent
  ],
  providers: [
    LoggingConfigurationService,
    HomeGrownLoggingService,
    ApplianceContextService,
    ApplianceApiService,
    MessageService
  ]
})
export class SharedModule { }
