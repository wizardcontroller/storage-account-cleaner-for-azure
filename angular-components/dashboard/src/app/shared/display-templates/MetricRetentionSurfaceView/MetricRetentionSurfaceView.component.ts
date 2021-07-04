import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ICanBeHiddenFromDisplay } from '../../interfaces/ICanBeHiddenFromDisplay';

@Component({
  selector: 'lib-MetricRetentionSurfaceView',
  templateUrl: './MetricRetentionSurfaceView.component.html',
  styleUrls: ['./MetricRetentionSurfaceView.component.css']
})
export class MetricRetentionSurfaceViewComponent implements OnInit, ICanBeHiddenFromDisplay {

  constructor( private route: ActivatedRoute) {
    this.isShow = false;
   }
  isShow: boolean;
  toggleDisplay(): void {

  }

  ngOnInit() {
  }

}
