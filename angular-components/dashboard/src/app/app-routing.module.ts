import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AppComponent } from './app.component';
import { ApplianceContextViewComponent } from './shared/display-templates/ApplianceContextView/ApplianceContextView.component';
import { DiagnosticsRetentionSurfaceViewComponent } from './shared/display-templates/DiagnosticsRetentionSurfaceView/DiagnosticsRetentionSurfaceView.component';
import { JobOutputViewComponent } from './shared/display-templates/JobOutputView/JobOutputView.component';
import { MetricRetentionSurfaceViewComponent } from './shared/display-templates/MetricRetentionSurfaceView/MetricRetentionSurfaceView.component';
import { ApplianceContextService } from './shared/display-templates/services/ApplianceContext.service';
import { StorageAccountViewComponent } from './shared/display-templates/StorageAccountView/StorageAccountView.component';
import { SubscriptionsViewComponent } from './shared/display-templates/SubscriptionsView/SubscriptionsView.component';

const routes: Routes = [



  { path: 'workbench', component: ApplianceContextViewComponent ,
  children: [
    { path: 'subscriptions', component: SubscriptionsViewComponent },
    { path: 'storageaccounts', component: StorageAccountViewComponent,
    children:[
      { path: 'DiagnosticsRetention', component: DiagnosticsRetentionSurfaceViewComponent },
      { path: 'MetricRetention', component: MetricRetentionSurfaceViewComponent }
    ]
   }

  ]},
  { path: 'report', component: JobOutputViewComponent },
  { path: '', redirectTo: 'workbench', pathMatch: 'full'},
  { path: '**', component: ApplianceContextViewComponent }
];


const routesOrig: Routes = [



  { path: '#workbench', component: ApplianceContextViewComponent ,
  children: [
    { path: 'subscriptions', component: SubscriptionsViewComponent,
      children:[
        { path: 'storageaccounts', component: StorageAccountViewComponent }
      ] },

  ]},
  { path: '#report', component: JobOutputViewComponent },
  { path: '', redirectTo: 'workbench', pathMatch: 'full'},
  { path: '**', component: ApplianceContextViewComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
