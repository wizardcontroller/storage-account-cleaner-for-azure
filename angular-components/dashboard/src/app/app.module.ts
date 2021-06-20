import { BASE_PATH } from '@wizardcontroller/storage-account-cleaner-lib/src/appliance-webapi/variables';
import { CoreModule } from './core/core.module';
import { OperatorModule } from './features/operator/operator.module';
import { HttpClientModule } from '@angular/common/http';
import { PageModelService } from './shared/page-model.service';
import { SharedModule } from './shared/shared.module';
import { NgModule, Host } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { WorkflowCheckpointComponent } from './features/operator/workflow-checkpoint/workflow-checkpoint.component';
import { AppComponent } from './app.component';
import { OperatorComponent } from './operator/operator.component';
import { Configuration, ConfigurationParameters } from '@wizardcontroller/storage-account-cleaner-lib/src/appliance-webapi/configuration';
import { environment } from '../environments/environment';
import { ApiModule } from '@wizardcontroller/storage-account-cleaner-lib/src/public-api';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
     SharedModule,
     CoreModule,
     HttpClientModule
  ],
  providers: [{ provide: BASE_PATH, useValue: environment.BASE_PATH }],
  bootstrap: [AppComponent]
})
export class AppModule { }
