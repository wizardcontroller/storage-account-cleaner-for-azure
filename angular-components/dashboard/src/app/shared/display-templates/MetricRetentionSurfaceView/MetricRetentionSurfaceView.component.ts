import { Component, OnInit } from '@angular/core';
import { ICanBeHiddenFromDisplay } from '../../interfaces/ICanBeHiddenFromDisplay';

@Component({
  selector: 'lib-MetricRetentionSurfaceView',
  templateUrl: './MetricRetentionSurfaceView.component.html',
  styleUrls: ['./MetricRetentionSurfaceView.component.css']
})
export class MetricRetentionSurfaceViewComponent implements OnInit, ICanBeHiddenFromDisplay {

  constructor() {
    this.isShow = false;
   }
  isShow: boolean;
  toggleDisplay(): void {
    throw new Error('Method not implemented.');
  }

  ngOnInit() {
  }

}
