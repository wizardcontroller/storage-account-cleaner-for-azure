import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AppComponent } from './app.component';
import { ApplianceContextViewComponent } from './shared/display-templates/ApplianceContextView/ApplianceContextView.component';
import { ApplianceContextService } from './shared/display-templates/services/ApplianceContext.service';
import { StorageAccountViewComponent } from './shared/display-templates/StorageAccountView/StorageAccountView.component';
import { SubscriptionsViewComponent } from './shared/display-templates/SubscriptionsView/SubscriptionsView.component';

const routes: Routes = [



  { path: 'workbench', component: ApplianceContextViewComponent },
  { path: '', redirectTo: 'workbench', pathMatch: 'full'},
  { path: '**', component: ApplianceContextViewComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
