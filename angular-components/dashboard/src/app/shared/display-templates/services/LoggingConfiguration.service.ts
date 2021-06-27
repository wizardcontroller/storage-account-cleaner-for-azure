import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class LoggingConfigurationService {

constructor() {
  this.isEnabled = true;
 }

public isEnabled : boolean;
}
