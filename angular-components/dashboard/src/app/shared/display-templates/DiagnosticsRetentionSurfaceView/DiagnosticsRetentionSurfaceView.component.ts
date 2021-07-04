import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ICanBeHiddenFromDisplay } from '../../interfaces/ICanBeHiddenFromDisplay';

@Component({
  selector: 'lib-DiagnosticsRetentionSurfaceView',
  templateUrl: './DiagnosticsRetentionSurfaceView.component.html',
  styleUrls: ['./DiagnosticsRetentionSurfaceView.component.css']
})
export class DiagnosticsRetentionSurfaceViewComponent implements OnInit, ICanBeHiddenFromDisplay {

  constructor( private route: ActivatedRoute) {
    this.isShow = false;
  }
  isShow: boolean;
  toggleDisplay(): void {

  }

  ngOnInit() {
  }

}
