import { Injectable } from "@angular/core";
import { TableStorageTableRetentionPolicy } from "./TableStorageTableRetentionPolicy";
import { TableStorageEntityRetentionPolicy } from "./TableStorageEntityRetentionPolicy";

@Injectable()


export class TableStorageRetentionPolicyEntity {
  constructor() {}
  public id : String = new String();

  public tableStorageEntityRetentionPolicy : TableStorageEntityRetentionPolicy = new TableStorageEntityRetentionPolicy();

  public tableStorageTableRetentionPolicy : TableStorageTableRetentionPolicy = new TableStorageTableRetentionPolicy();
}
