import { TableStorageRetentionPolicy } from './TableStorageRetentionPolicy';
import { TableStorageTableRetentionPolicy } from './TableStorageTableRetentionPolicy';
import { TableStorageEntityRetentionPolicy } from './TableStorageEntityRetentionPolicy';
import { PolicyEnforcementMode } from './PolicyEnforcementMode.enum';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ModelsComponent } from './models.component';

export {TableStorageRetentionPolicy} from './TableStorageRetentionPolicy'
@NgModule({
  imports: [
    CommonModule
  ],
  declarations: [ModelsComponent]
})
export class ModelsModule {
  constructor() {}


}
