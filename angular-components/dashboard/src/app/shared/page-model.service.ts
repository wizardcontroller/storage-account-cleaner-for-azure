

import { environment } from '../../environments/environment';
import { Inject, Injectable, OnInit } from '@angular/core';
import { HomeService } from '@wizardcontroller/storage-account-cleaner-lib/src/public-api';
import { OperatorPageModel } from '@wizardcontroller/storage-account-cleaner-lib/src/appliance-webapi/model/operatorPageModel';
import { Observable, of } from 'rxjs';

const dashboardUrl = environment.BASE_PATH;
@Injectable()
export class PageModelService implements OnInit{


  constructor(private homeService: HomeService) {
    console.log("pagemodel service starting");

  }

  ngOnInit(): void {
    console.log("page model service initializing");

    this.pageModel$ = this.homeService.operatorPageModelGet()
    /**
    this.homeService.operatorPageModelGet()
    .subscribe(data => {
      this.operatorPageModel = data;
    });
    */
  }

  pageModel$!: Observable<OperatorPageModel>;

  getHomeService() : HomeService{
    return this.homeService;;
  }
  }


