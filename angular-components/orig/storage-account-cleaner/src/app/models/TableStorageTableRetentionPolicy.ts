import { PolicyEnforcementMode } from './PolicyEnforcementMode.enum';
import { Injectable } from "@angular/core";



@Injectable()
export class TableStorageTableRetentionPolicy {
  constructor() {}

  public id : String = new String();

  public policyEnforcementMode : PolicyEnforcementMode  = PolicyEnforcementMode.WhatIf;

  public oldestRetainedTable : Date = new Date();

  public mostRecentRetainedTable : Date = new Date();
}
