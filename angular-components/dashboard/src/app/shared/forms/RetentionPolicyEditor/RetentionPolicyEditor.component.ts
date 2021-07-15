import { Component, OnDestroy, OnInit } from '@angular/core';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';

@Component({
  selector: 'app-RetentionPolicyEditor',
  templateUrl: './RetentionPolicyEditor.component.html',
  styleUrls: ['./RetentionPolicyEditor.component.css']
})

@AutoUnsubscribe()
export class RetentionPolicyEditorComponent implements OnInit, OnDestroy {

  constructor() {
    // nothing yet
  }

  ngOnDestroy(): void {
    // nothing yet
  }

  ngOnInit() {
    // nothing yet
  }

}
