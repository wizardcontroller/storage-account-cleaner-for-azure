import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { AppComponent } from './app.component';
import { RetentionPolicyEditorModule } from './retention-policy-editor/retention-policy-editor.module';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    HttpClientModule,BrowserModule, RetentionPolicyEditorModule
  ],
  exports: [RetentionPolicyEditorModule,HttpClientModule],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
