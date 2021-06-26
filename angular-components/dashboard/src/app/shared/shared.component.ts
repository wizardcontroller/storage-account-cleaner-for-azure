import { Component, OnInit } from '@angular/core';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';

@Component({
  selector: 'lib-shared',
  templateUrl: './shared.component.html',
  styleUrls: ['./shared.component.css']
})

export class SharedComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
