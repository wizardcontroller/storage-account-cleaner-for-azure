import { Component, OnInit } from '@angular/core';
import { ICanBeHiddenFromDisplay } from '../../interfaces/ICanBeHiddenFromDisplay';

@Component({
  selector: 'lib-JobOutputView',
  templateUrl: './JobOutputView.component.html',
  styleUrls: ['./JobOutputView.component.css']
})
export class JobOutputViewComponent implements OnInit, ICanBeHiddenFromDisplay {

  constructor() {
    this.isShow = false;
   }
  isShow: boolean;
  toggleDisplay(): void {

  }

  ngOnInit() {
  }

}
