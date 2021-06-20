import { PageModelService } from './../shared/page-model.service';
import { Component, Host, OnInit } from '@angular/core';
import { OperatorPageModel } from '@wizardcontroller/storage-account-cleaner-lib/src/appliance-webapi/model/operatorPageModel';

@Component({
  selector: 'app-operator',
  templateUrl: './operator.component.html',
  styleUrls: ['./operator.component.css']
})
export class OperatorComponent implements OnInit {

  operatorPageModel!: OperatorPageModel;

  constructor(public svc: PageModelService) {
    if(svc == null)
    {
      console.log("pageModel service is null");
    }
    else{
    console.log("operator component initializing");
    /*
    const subscription = this.svc.getHomeService().operatorPageModelGet().subscribe(
      {
        next(pageModel){
          console.log("got appliance url : " + pageModel.applianceUrl);
        },
        error(msg){
          console.log('error getting pagemodel ; ' + msg);
        }
      }
    );
    */
    }
  }

  ngOnInit(): void {

  }

}
