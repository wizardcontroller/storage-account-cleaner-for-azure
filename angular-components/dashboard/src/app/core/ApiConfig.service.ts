
import { Injectable, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ConfigService, OperatorPageModel } from 'index';
@Injectable({
  providedIn: 'root'
})
export class ApiConfigService implements OnInit {
  operatorPageModel: OperatorPageModel | undefined;

constructor(private configService : ConfigService, private router: Router) {
  console.log("config service starting");

  configService.configuration.basePath = window.location.origin;
  this.ngOnInit();
 }

  /*
    currently angular injectables do not participate in all lifecycle hooks
  */
  ngOnInit(): void {
    console.log("calling config service");
    this.configService.getOperatorPageModel().subscribe
    (
        (data: OperatorPageModel) => this.operatorPageModel = data,
        (err: any) => console.log(err),
        () => {
          console.log("done getting operator page model: applianceUrl is " + this.operatorPageModel?.applianceUrl);
        }
    );


  }

}
