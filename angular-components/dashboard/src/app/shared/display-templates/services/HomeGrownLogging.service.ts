import { Injectable } from '@angular/core';
import { ILogger } from './ILogger';
import { LoggingConfigurationService } from './LoggingConfiguration.service';

@Injectable({
  providedIn: 'root'
})
export class HomeGrownLoggingService<T> implements ILogger<T>{

constructor(private config: LoggingConfigurationService) { }

  log(message: string): void {
    if(this.config.isEnabled){
      console.log( typeof(this) + ": " +   message);
    }
  }

}
