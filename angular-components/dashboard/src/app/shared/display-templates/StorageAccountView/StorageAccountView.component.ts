import { Component, OnInit } from '@angular/core';
import { ICanBeHiddenFromDisplay } from '../../interfaces/ICanBeHiddenFromDisplay';

@Component({
  selector: 'lib-StorageAccountView',
  templateUrl: './StorageAccountView.component.html',
  styleUrls: ['./StorageAccountView.component.css']
})
export class StorageAccountViewComponent implements OnInit, ICanBeHiddenFromDisplay {

  constructor() {
    this.isShow = false;
   }
  isShow: boolean;
  toggleDisplay(): void {

  }

  ngOnInit() {
  }

}
