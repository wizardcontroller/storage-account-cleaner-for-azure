
import { ApiConfigService } from './core/ApiConfig.service';
import { Component, OnInit } from '@angular/core';
import { CoreComponent } from './core/core.component';
import { OperatorPageModel } from 'index';
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  operatorPageModel: OperatorPageModel | undefined;
  baseUri: String | undefined;
  title = 'dashboard';
  constructor(private apiConfigSvc : ApiConfigService) {

  }

   ngOnInit(): void {
    this.operatorPageModel = this.apiConfigSvc.operatorPageModel;
    this.baseUri = this.operatorPageModel?.applianceUrl?.toString();
  }

}
