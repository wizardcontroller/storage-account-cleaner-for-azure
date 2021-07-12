import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { ICanBeHiddenFromDisplay } from '../../interfaces/ICanBeHiddenFromDisplay';

@Component({
  selector: 'lib-DiagnosticsRetentionSurfaceView',
  templateUrl: './DiagnosticsRetentionSurfaceView.component.html',
  styleUrls: ['./DiagnosticsRetentionSurfaceView.component.css']
})

@AutoUnsubscribe()
export class DiagnosticsRetentionSurfaceViewComponent implements OnDestroy, OnInit, ICanBeHiddenFromDisplay {

  constructor( private route: ActivatedRoute) {
    this.isShow = false;
  }
  ngOnDestroy(): void {

  }
  isShow: boolean;
  toggleDisplay(): void {

  }

  ngOnInit() {
  }

}
