import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'lib-ApplianceContextView',
  templateUrl: './ApplianceContextView.component.html',
  styleUrls: ['./ApplianceContextView.component.css']
})
export class ApplianceContextViewComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

  isShow = false;

  toggleDisplay() {
    this.isShow = !this.isShow;
  }
}
