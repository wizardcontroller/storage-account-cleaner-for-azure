import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RetentionPolicyEditorComponent } from './retention-policy-editor.component';

@NgModule({
  imports: [
    CommonModule,
  ],
  exports: [RetentionPolicyEditorComponent],
  declarations: [RetentionPolicyEditorComponent]
})
export class RetentionPolicyEditorModule { }
