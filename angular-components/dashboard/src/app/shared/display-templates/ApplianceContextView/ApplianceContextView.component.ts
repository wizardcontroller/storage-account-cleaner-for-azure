import { Component, OnInit } from '@angular/core';
import { ICanBeHiddenFromDisplay } from '../../interfaces/ICanBeHiddenFromDisplay';

@Component({
  selector: 'lib-ApplianceContextView',
  templateUrl: './ApplianceContextView.component.html',
  styleUrls: ['./ApplianceContextView.component.css']
})
export class ApplianceContextViewComponent implements OnInit, ICanBeHiddenFromDisplay {

  constructor() { }

  ngOnInit() {
  }

  isShow = false;

  toggleDisplay() {
    this.isShow = !this.isShow;
  }
}
