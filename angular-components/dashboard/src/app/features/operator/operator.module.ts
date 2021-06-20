import { SharedModule } from './../../shared/shared.module';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WorkflowCheckpointComponent } from './workflow-checkpoint/workflow-checkpoint.component';



@NgModule({
  declarations: [
    WorkflowCheckpointComponent
  ],
  imports: [
    CommonModule, SharedModule
  ]
})
export class OperatorModule { }
