import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { ICanBeHiddenFromDisplay } from '../../interfaces/ICanBeHiddenFromDisplay';

@Component({
  selector: 'app-JobOutputView',
  templateUrl: './JobOutputView.component.html',
  styleUrls: ['./JobOutputView.component.css']
})

@AutoUnsubscribe()
export class JobOutputViewComponent implements OnInit, OnDestroy, ICanBeHiddenFromDisplay {

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
