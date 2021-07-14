import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { ICanBeHiddenFromDisplay } from '../../interfaces/ICanBeHiddenFromDisplay';

@Component({
  selector: 'app-DiagnosticsRetentionSurfaceView',
  templateUrl: './DiagnosticsRetentionSurfaceView.component.html',
  styleUrls: ['./DiagnosticsRetentionSurfaceView.component.css']
})

@AutoUnsubscribe()
export class DiagnosticsRetentionSurfaceViewComponent implements OnDestroy, OnInit, ICanBeHiddenFromDisplay {

  constructor(private route: ActivatedRoute) {
    this.isShow = false;
  }
  ngOnDestroy(): void {
       // nothing yet
  }
  isShow: boolean;
  toggleDisplay(): void {

  }

  ngOnInit() {
    // nothing yet
  }

}
