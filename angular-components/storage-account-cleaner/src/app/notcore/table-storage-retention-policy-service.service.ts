import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import {TableStorageRetentionPolicy } from '../models/models.module'
import {TableStorageEntityRetentionPolicy } from '../models/TableStorageEntityRetentionPolicy'
import {TableStorageTableRetentionPolicy} from '../models/TableStorageTableRetentionPolicy'


@Injectable({
  providedIn: 'root'
})

export class TableStorageRetentionPolicyServiceService {

  constructor(private http: HttpClient) { }

  public TableStorageRetentionPolicy : TableStorageRetentionPolicy = new TableStorageRetentionPolicy();

  public TableStorageEntityRetentionPolicy : TableStorageEntityRetentionPolicy = new TableStorageEntityRetentionPolicy();

}
