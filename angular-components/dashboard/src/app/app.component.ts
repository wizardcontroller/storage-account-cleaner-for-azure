import { CoreModule } from './core/core.module';
import { HomeService } from '@wizardcontroller/storage-account-cleaner-lib/src/public-api';
import { Component, ElementRef, Input, OnInit } from '@angular/core';
import { OperatorPageModel } from '@wizardcontroller/storage-account-cleaner-lib/src/appliance-webapi/model/operatorPageModel';
import { Operator, Subscription } from 'rxjs';

import { HttpClient, HttpClientModule } from '@angular/common/http';
import { PageModelService } from './shared/page-model.service';
import { WorkflowCheckpointComponent } from './features/operator/workflow-checkpoint/workflow-checkpoint.component';

/**
 *
 * tihs component needs to be initialized with the
 * appliance base url and an auth token
 *
 * @author rvs
 * @export
 * @class AppComponent
 * @implements {OnInit}
 */
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})

export class AppComponent implements OnInit {

  title = 'dashboard';

  @Input()
  applianceUrl : String = "";

  @Input()
  authToken: String = "";

  private native: any;
  constructor(el: ElementRef){

    this.native = el.nativeElement;
    var token = this.native.getAttribute('authToken');
    var applianceUrl = this.native.getAttribute('applianceUrl');
    console.log('token is: ' + token);
    console.log('applianceUrl is: ' + applianceUrl);


  }


  ngOnInit(): void {

   }
}
