
import { Injectable, OnInit } from '@angular/core';
import { ConfigService, OperatorPageModel } from 'index';
@Injectable({
  providedIn: 'root'
})
export class ApiConfigService implements OnInit {
  operatorPageModel: OperatorPageModel | undefined;

constructor(private configService : ConfigService) {
  console.log("config service starting");
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
        () => console.log("done getting operator page model")
    );
  }

}
