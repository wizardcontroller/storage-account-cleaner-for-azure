import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppComponent } from './app.component';
import { RetentionPolicyEditorModule } from './retention-policy-editor/retention-policy-editor.module';
import { NotcoreModule } from './notcore/notcore.module';
import { ApiModule } from 'appliance-apiclient';
@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule, RetentionPolicyEditorModule,
    NotcoreModule,
    ApiModule
  ],
  exports: [RetentionPolicyEditorModule],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
