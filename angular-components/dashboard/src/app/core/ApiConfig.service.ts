
import { Injectable, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ConfigService, OperatorPageModel } from 'index';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { BehaviorSubject, ReplaySubject, Subject } from 'rxjs';
@Injectable({
  providedIn: 'root'
})

export class ApiConfigService implements OnInit {
  operatorPageModel!: OperatorPageModel;
    // operator page model change notification support
    private currentPageModelSource =  new ReplaySubject<OperatorPageModel>();
    operatorPageModelChanges$ = this.currentPageModelSource.asObservable();


constructor(private configService : ConfigService, private router: Router) {
  console.log("ApiConfigService service starting");

  configService.configuration.basePath = window.location.origin;
  this.ngOnInit();
 }

  /*
    currently angular injectables do not participate in all lifecycle hooks
  */
  ngOnInit(): void {

  }

   /*
                call the dashboard and get the operator page model
                including base url for discovery appliance
                and user appliances
            */
                getOperatorPageModel() {
                  console.log("apiconfigsvc is getting operator page model");
                  this.configService.configuration.basePath = window.location.origin;
                  console.log('calling config service');
                  this.configService.getOperatorPageModel().subscribe(
                    (data: OperatorPageModel) => {
                      return this.gotOperatorPageModel(data);
                    },
                    (err: any) => console.log(err),
                    () => {
                      console.log(
                        'done getting operator page model: applianceUrl is ' +
                          this.operatorPageModel?.applianceUrl
                      );
                    }
                  );
                }

                /**
                 * @description signal the new pagemodel subject listeners
                 * @param data
                 */
                private gotOperatorPageModel(data: OperatorPageModel): void {
                  this.operatorPageModel = data;
                  this.currentPageModelSource.next(data);
                }

}
