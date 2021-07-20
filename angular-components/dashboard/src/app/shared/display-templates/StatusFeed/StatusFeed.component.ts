import { Component, OnInit } from '@angular/core';
import { ApplianceApiService } from '../../services/appliance-api.service';

@Component({
  selector: 'app-StatusFeed',
  templateUrl: './StatusFeed.component.html',
  styleUrls: ['./StatusFeed.component.css']
})
export class StatusFeedComponent implements OnInit {


  constructor(
    private applianceApiSvc: ApplianceApiService
  ) { }

  ngOnInit() {

  }

}
